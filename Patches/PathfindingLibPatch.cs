using HarmonyLib;
using PathfindingLib.API;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.AI.Navigation;

namespace NavMeshLib.Patches
{
    internal class PathfindingLibPatch
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void BeginNavMeshWrite()
        {
            NavMeshLock.BeginWrite();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void EndNavMeshWrite()
        {
            NavMeshLock.EndWrite();
        }

        //[HarmonyPatch(typeof(NavMeshLock), "BeginWrite")]
        //[HarmonyPrefix]
        //public static void BeginWrite_Prefix()
        //{
        //    Plugin.LogInfo("NavMeshLock: BeginWrite called");
        //    Plugin.LogInfo(Environment.StackTrace);
        //}

        //[HarmonyPatch(typeof(NavMeshLock), "BeginWrite")]
        //[HarmonyPostfix]
        //public static void BeginWrite_Postfix()
        //{
        //    Plugin.LogInfo("NavMeshLock: BeginWrite ended");
        //    Plugin.LogInfo(Environment.StackTrace);
        //}

        //[HarmonyPatch(typeof(NavMeshLock), "EndWrite")]
        //[HarmonyPrefix]
        //public static void EndWrite_Prefix()
        //{
        //    Plugin.LogInfo("NavMeshLock: EndWrite called");
        //    Plugin.LogInfo(Environment.StackTrace);
        //}

        //[HarmonyPatch(typeof(NavMeshLock), "EndWrite")]
        //[HarmonyPostfix]
        //public static void EndWrite_Postfix()
        //{
        //    Plugin.LogInfo("NavMeshLock: EndWrite ended");
        //    Plugin.LogInfo(Environment.StackTrace);
        //}
    }
}
