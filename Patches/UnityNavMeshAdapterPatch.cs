using DunGen.Adapters;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Unity.AI.Navigation;

namespace NavMeshLib.Patches
{
    [HarmonyPatch(typeof(UnityNavMeshAdapter))]
    public class UnityNavMeshAdapterPatch
    {
        [HarmonyPatch("BakeFullDungeon")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> BakeFullDungeon_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var startIndex = -1;
            var codes = new List<CodeInstruction>(instructions);

            // Target function: navMeshSurface.BuildNavMesh();
            MethodInfo buildNavMeshMethod = AccessTools.Method(typeof(NavMeshSurface), nameof(NavMeshSurface.BuildNavMesh));

            // ----------------------------------------------------------------------
            for (var i = 0; i < codes.Count - 1; i++)
            {
                if ((codes[i].opcode == OpCodes.Ldloc || codes[i].opcode == OpCodes.Ldloc_S)
                    && codes[i].operand is LocalBuilder localVar && localVar.LocalIndex == 5 // According to IL Spy, the local variable we want is at index 5
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
                codes[startIndex + 1].operand = AccessTools.Method(typeof(RoundManagerPatch), nameof(RoundManagerPatch.BuildInteriorNavMesh));

                startIndex = -1;
            }
            else
            {
                Plugin.LogError($"NavMeshLib.Patches.UnityNavMeshAdapterPatch.BakeFullDungeon_Transpiler could not change interior NavMesh generation to be asynchronous!");
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch("Generate")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Generate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var startIndex = -1;
            var codes = new List<CodeInstruction>(instructions);

            // Target function: navMeshSurface.BuildNavMesh();
            MethodInfo buildNavMeshMethod = AccessTools.Method(typeof(NavMeshSurface), nameof(NavMeshSurface.BuildNavMesh));

            // ----------------------------------------------------------------------
            for (var i = 0; i < codes.Count - 2; i++)
            {
                if ((codes[i].opcode == OpCodes.Ldloc || codes[i].opcode == OpCodes.Ldloc_S)
                    && codes[i].operand is LocalBuilder localVar && localVar.LocalIndex == 5 // According to IL Spy, the local variable we want is at index 5
                    && codes[i + 2].Calls(buildNavMeshMethod))
                {
                    startIndex = i;
                    break;
                }
            }
            if (startIndex > -1)
            {
                // Override the BuildNavMesh with our own custom call
                codes[startIndex + 2].opcode = OpCodes.Call;
                codes[startIndex + 2].operand = AccessTools.Method(typeof(RoundManagerPatch), nameof(RoundManagerPatch.BuildInteriorNavMesh));

                startIndex = -1;
            }
            else
            {
                Plugin.LogError($"NavMeshLib.Patches.UnityNavMeshAdapterPatch.Generate_Transpiler could not change interior NavMesh generation to be asynchronous!");
            }

            return codes.AsEnumerable();
        }
    }
}