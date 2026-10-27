using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.AI.Navigation;

namespace NavMeshLib.Patches
{
    [HarmonyPatch(typeof(NavMeshLink))]
    public class NavMeshLinkPatch
    {
        /// <summary>
        /// This only exists due to PathfindingLib not null checking the transform when calling this function.
        /// A race condition causes the game to hard crash....this is my Hacky fix for the time being.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPatch(nameof(NavMeshLink.HasTransformChanged))]
        [HarmonyPrefix]
        static bool HasTransformChanged_Prefix(NavMeshLink __instance, ref bool __result)
        {
            if (__instance == null)
            {
                Plugin.LogWarning($"NavMeshLink.HasTransformChanged was called on a null NavMeshLink {__instance}!");
                NavMeshLink.RemoveTracking(__instance); // For some reason, some links fail to be removed from the auto update list....this force removes this link from it
                __result = true; // Make PathfiningLib mark the NavMesh as being editied.
                return false;
            }

            if (__instance.transform == null)
            {
                Plugin.LogWarning($"NavMeshLink.HasTransformChanged was called on a NavMeshLink with a null transform {__instance}!");
                NavMeshLink.RemoveTracking(__instance); // For some reason, some links fail to be removed from the auto update list....this force removes this link from it
                __result = true; // Make PathfiningLib mark the NavMesh as being editied.
                return false;
            }

            return true;
        }
    }
}
