using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshLib.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    public class RoundManagerPatch
    {
        [HarmonyPatch("BakeDunGenNavMesh")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> BakeDunGenNavMesh_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var startIndex = -1;
            var codes = new List<CodeInstruction>(instructions);

            // Target function: navMeshSurface.BuildNavMesh();
            MethodInfo buildNavMeshMethod = AccessTools.Method(typeof(NavMeshSurface), nameof(NavMeshSurface.BuildNavMesh));

            // ----------------------------------------------------------------------
            for (var i = 0; i < codes.Count - 1; i++)
            {
                if ((codes[i].opcode == OpCodes.Ldloc || codes[i].opcode == OpCodes.Ldloc_S)
                    && codes[i].operand is LocalBuilder localVar && localVar.LocalIndex == 6 // According to IL Spy, the local variable we want is at index 6
                    && codes[i + 1].Calls(buildNavMeshMethod))
                {
                    startIndex = i;
                    break;
                }
            }
            if (startIndex > -1)
            {
                // Override the BuildNavMesh with our own custom call
                codes[startIndex + 1].opcode = OpCodes.Call;
                codes[startIndex + 1].operand = AccessTools.Method(typeof(RoundManagerPatch), nameof(BuildInteriorNavMesh));

                startIndex = -1;
            }
            else
            {
                Plugin.LogError($"NavMeshLib.Patches.RoundManagerPatch.BakeDunGenNavMesh_Transpiler could not change interior NavMesh generation to be asynchronous!");
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch("SpawnOutsideHazards")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SpawnOutsideHazards_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var startIndex = -1;
            var codes = new List<CodeInstruction>(instructions);

            // Target function: gameObject2.GetComponent<NavMeshSurface>().BuildNavMesh();
            MethodInfo buildNavMeshMethod = AccessTools.Method(typeof(NavMeshSurface), nameof(NavMeshSurface.BuildNavMesh));
            MethodInfo getComponentMethod = AccessTools.Method(typeof(GameObject), nameof(GameObject.GetComponent), null, new Type[] { typeof(NavMeshSurface) });

            // ----------------------------------------------------------------------
            const int blockLength = 2;
            for (var i = 0; i < codes.Count - blockLength; i++)
            {
                if ((codes[i].opcode == OpCodes.Ldloc || codes[i].opcode == OpCodes.Ldloc_S)
                    && codes[i].operand is LocalBuilder localVar && localVar.LocalIndex == 26 // According to IL Spy, the local variable we want is at index 26
                    && codes[i + 1].Calls(getComponentMethod)
                    && codes[i + 2].Calls(buildNavMeshMethod))
                {
                    startIndex = i;
                    break;
                }
            }
            if (startIndex > -1)
            {
                // Remove the BuildNavMesh
                // Our Postfix will handle this
                for (int i = 0; i <= blockLength; i++)
                {
                    codes[startIndex + i].opcode = OpCodes.Nop;
                    codes[startIndex + i].operand = null;
                }
                startIndex = -1;
            }
            else
            {
                Plugin.LogError($"NavMeshLib.Patches.RoundManagerPatch.SpawnOutsideHazards_Transpiler could not change exterior NavMesh generation to be asynchronous!");
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch("SpawnOutsideHazards")]
        [HarmonyPostfix]
        static void SpawnOutsideHazards_Postfix()
        {
            Plugin.LogInfo($"[SpawnOutsideHazards] Starting Async NavMesh Generation for exterior.");
            NavMeshUtil.RebakeExteriorNavMesh(generateNewSurfaces: true);
        }

        internal static void BuildInteriorNavMesh(NavMeshSurface navMeshSurface)
        {
            Plugin.LogInfo($"[BuildInteriorNavMesh] Starting Async NavMesh Generation for interior surface {navMeshSurface} with agent ID {navMeshSurface.agentTypeID}");
            navMeshSurface.BuildNavMeshAsync();
        }
    }
}
