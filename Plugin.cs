using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using NavMeshLib.Patches;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshLib
{
    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "T-Rizzle.NavMeshLib";
        public const string PLUGIN_NAME = "NavMeshLib";
        public const string PLUGIN_VERSION = "1.0.0";
    }

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = null!;
        private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            Logger = base.Logger;

            // Load our patches
            PatchClass<NavMeshPatch>();
            PatchClass<RoundManagerPatch>();

            Plugin.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void PatchClass<T>()
            where T : class
        {
            try
            {
                _harmony.PatchAll(typeof(T));
            }
            catch (Exception e)
            {
                Plugin.LogError($"An error occured while patching {nameof(T)}: {e}");
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
    }
}
