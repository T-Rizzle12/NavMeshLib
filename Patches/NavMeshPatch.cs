using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AI;

namespace NavMeshLib.Patches
{
    [HarmonyPatch(typeof(NavMesh))]
    public class NavMeshPatch
    {
        [HarmonyPatch("GetSettingsByID")]
        [HarmonyPostfix]
        public static void GetSettingsByID_Postfix(ref NavMeshBuildSettings __result)
        {
            Plugin.LogDebug($"GetSettingsByID returned settings with ID: {__result.agentTypeID}");
            CustomAgentManager.OverrideNavMeshSettings(ref __result);
        }

        [HarmonyPatch("GetSettingsByIndex")]
        [HarmonyPostfix]
        public static void GetSettingsByIndex_Postfix(ref NavMeshBuildSettings __result)
        {
            Plugin.LogDebug($"GetSettingsByIndex returned settings with ID: {__result.agentTypeID}");
            CustomAgentManager.OverrideNavMeshSettings(ref __result);
        }

        [HarmonyPatch("GetSettingsNameFromID")]
        [HarmonyPostfix]
        public static void GetSettingsNameFromID_Postfix(int agentTypeID, ref string __result)
        {
            Plugin.LogDebug($"GetSettingsNameFromID returned name for agent with ID: {__result}");
            CustomAgentManager.GetSettingsNameFromID(agentTypeID, ref __result);
        }
    }
}
