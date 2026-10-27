using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using NavMeshLib.Patches;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace NavMeshLib
{
    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "T-Rizzle.NavMeshLib";
        public const string PLUGIN_NAME = "NavMeshLib";
        public const string PLUGIN_VERSION = "3.2.0";
    }

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = null!;
        private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

        private static readonly HashSet<string> ignoredScenes = new HashSet<string>() 
        {
            "InitSceneLaunchOptions",
            "InitScene",
            "InitSceneLANMode",
            "ColdOpen1",
            "ColdOpen2",
            "MainMenu"
        };

        private void Awake()
        {
            Logger = base.Logger;

            // Log what we are about todo
            Plugin.LogInfo("Loading Required Patches");

            // Load our patches
            PatchClass<NavMeshPatch>();
            PatchClass<RoundManagerPatch>();
            PatchClass<UnityNavMeshAdapterPatch>();
            PatchClass<NavMeshLinkPatch>();

            // Add our OnSceneLoaded hooks
            SceneManager.sceneLoaded += OnSceneLoaded_CopyOffMeshLinks;
            SceneManager.sceneLoaded += OnSceneLoaded_CompanyCruiser;
            SceneManager.sceneLoaded += OnSceneLoaded_MineshaftElevator;

            Plugin.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnSceneLoaded_CopyOffMeshLinks(Scene scene, LoadSceneMode mode)
        {
            // Don't do this for certain scenes
            if (ignoredScenes.Contains(scene.name))
            {
                return;
            }

            Plugin.LogInfo($"Copying OffMeshLinks with NavMeshLinks to support custom agent types for scene {scene.name}.");
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                NavMeshUtil.CopyOffMeshLinks(rootObjects[i]);
            }
            Plugin.LogInfo("Finished copying OffMeshLinks with NavMeshLinks to support custom agent types.");
        }

        private void OnSceneLoaded_CompanyCruiser(Scene scene, LoadSceneMode mode)
        {
            // Add a custom NavMeshSurface to the back of the cruiser, so custom agent ids can pathfind in the trunk.
            var prefabs = Resources.FindObjectsOfTypeAll<VehicleController>();
            if (prefabs == null || prefabs.Length == 0) return;

            // Flag for if we successfully added what we wanted
            bool replaced = false;
            try
            {
                // Loop through all prefabs
                for (int i = 0; i < prefabs.Length; i++)
                {
                    // Can't use **is VehicleController** because it will return true for all subclasses, we only want the base class.
                    var vehicle = prefabs[i];
                    if (vehicle != null
                        && vehicle.GetType() == typeof(VehicleController)
                        && !vehicle.gameObject.scene.IsValid())
                    {
                        // Add a custom NavMeshSurface with our custom agent ID
                        Plugin.LogInfo($"Found VehicleController prefab {vehicle}");

                        // Add custom Agent IDs to the back of the crusier
                        NavMeshUtil.AddMissingNavMeshSurfaces(vehicle.gameObject);

                        // Update OffMeshLinks
                        NavMeshUtil.CopyOffMeshLinks(vehicle.gameObject);

                        // Mark it as updated
                        replaced = true;
                        Plugin.LogInfo("Added NavMeshSurface to Company Cruiser Prefab!");
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.LogError($"An error occured when attempting to edit Company Cruiser Prefab. \n Error: {e}");
            }
            finally
            {
                // Check if we succeded
                if (replaced)
                {
                    // Remove our hook as our work here is done
                    SceneManager.sceneLoaded -= OnSceneLoaded_CompanyCruiser;
                }
            }
        }

        private void OnSceneLoaded_MineshaftElevator(Scene scene, LoadSceneMode mode)
        {
            // Add a custom NavMeshSurface and NavMeshLink to mineshaft elevator, so custom agent ids can pathfind in the elevator.
            var prefabs = Resources.FindObjectsOfTypeAll<MineshaftElevatorController>();
            if (prefabs == null || prefabs.Length == 0) return;

            // Flag for if we successfully added what we wanted
            bool replaced = false;
            try
            {
                // Loop through all prefabs
                for (int i = 0; i < prefabs.Length; i++)
                {
                    // We check for MineshaftElevatorController to account for modded elevators that may inherit from the base class.
                    var mineshaftElevator = prefabs[i];
                    if (mineshaftElevator != null
                        && !mineshaftElevator.gameObject.scene.IsValid())
                    {
                        // Add a custom NavMeshSurface with our custom agent ID
                        Plugin.LogInfo($"Found MineshaftElevatorController prefab {mineshaftElevator}");

                        // Add custom Agent IDs to the inside of the elevator
                        NavMeshUtil.AddMissingNavMeshSurfaces(mineshaftElevator.gameObject);

                        // Copy OffMeshLinks in the prefab
                        NavMeshUtil.CopyOffMeshLinks(mineshaftElevator.gameObject);

                        // Mark it as updated
                        replaced = true;
                        Plugin.LogInfo("Added NavMeshSurface to Mineshaft Elevator Prefab!");
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.LogError($"An error occured when attempting to edit Mineshaft Elevator Prefab. \n Error: {e}");
            }
            finally
            {
                // Check if we succeded
                if (replaced)
                {
                    // Remove our hook as our work here is done
                    SceneManager.sceneLoaded -= OnSceneLoaded_MineshaftElevator;
                }
            }
        }

        private void PatchClass<T>()
            where T : class
        {
            try
            {
                Plugin.LogDebug($"Patching {typeof(T)}");
                _harmony.PatchAll(typeof(T));
            }
            catch (Exception e)
            {
                Plugin.LogError($"An error occured while patching {typeof(T)}: {e}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogDebug(string debugLog)
        {
            Logger.LogDebug(debugLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogInfo(string infoLog)
        {
            Logger.LogInfo(infoLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning(string warningLog)
        {
            Logger.LogWarning(warningLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError(string errorLog)
        {
            Logger.LogError(errorLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogFatal(string fatalLog)
        {
            Logger.LogFatal(fatalLog);
        }

        /// <summary>
        /// Finds a child transform with the specified name in the hierarchy of the given parent transform.
        /// </summary>
        /// <remarks>
        /// Only exists since the default Unity <see cref="Transform.Find(string)"/> method only searches for direct children, 
        /// not grandchildren or deeper descendants. <br/>
        /// WARNING: This method is recursive and may have performance implications if the hierarchy is deep or has many child objects. Use with caution. <br/>
        /// You should consider caching the result of this method if you need to access the same child multiple times.
        /// </remarks>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static Transform? FindChildWithName(Transform parent, string name)
        {
            // Check if the parent itself has the name we are looking for
            for (int i = 0; i < parent.childCount; i++)
            {
                // Get the child transform at index i
                Transform child = parent.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }

                // Recursively search in the child's children
                Transform? result = FindChildWithName(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            // If we reach here, it means we didn't find the child with the specified name in this branch of the hierarchy
            return null;
        }
    }
}
