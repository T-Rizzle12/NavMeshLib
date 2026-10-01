using NavMeshLib.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace NavMeshLib.Editor
{
    /// <summary>
    /// A helper mono behavior that allows moon makers to rebake the current scene's NavMesh.
    /// </summary>
    public class NavMeshUpdater : MonoBehaviour
    {
        /// <summary>
        /// What surfaces should be rebuild when UpdateNavMesh is called?
        /// </summary>
        [Tooltip("What surfaces should be rebuild when UpdateNavMesh is called?")]
        public RebuildType rebuildType = RebuildType.AllSurfaces;

        /// <summary>
        /// NavMesh surfaces to update when using Custom.
        /// </summary>
        [Tooltip("NavMesh surfaces to update when using Custom.")]
        public NavMeshSurface[]? surfacesToUpdate;

        /// <summary>
        /// The environment GameObject to update the NavMesh for when using OutsideSurfaces.
        /// </summary>
        /// <remarks>
        /// If left empty, we attempt to find a GameObject with the tag OutsideLevelNavMesh.
        /// </remarks>
        [Tooltip("The environment GameObject to update the NavMesh for when using OutsideSurfaces. If left empty, we attempt to find a GameObject with the tag OutsideLevelNavMesh.")]
        public GameObject? environmentObject;

        /// <summary>
        /// Updates the current scene's NavMesh based on <see cref="rebuildType"/>
        /// </summary>
        public void UpdateNavMesh()
        {
            // Did the moon creator specify surfaces for us to rebake
            NavMeshSurface[] foundNavMeshSurfaces;
            switch (rebuildType)
            {
                case RebuildType.OutsideSurfaces:
                    NavMeshUtil.RebakeExteriorNavMesh(environmentObject: environmentObject);
                    return;
                case RebuildType.InsideSurfaces:
                    NavMeshUtil.RebakeDunGenNavMesh();
                    return;
                case RebuildType.Custom:
                    foundNavMeshSurfaces = surfacesToUpdate ?? Array.Empty<NavMeshSurface>();
                    if (foundNavMeshSurfaces.Length == 0)
                    {
                        Plugin.LogWarning($"NavMeshUpdater on '{gameObject.name}' is configured for Custom but has no NavMesh surfaces assigned.");
                        return;
                    }
                    break;
                case RebuildType.ActiveSurfaces:
                case RebuildType.AllSurfaces:
                default:
                    var includeInactive = rebuildType == RebuildType.ActiveSurfaces ? FindObjectsInactive.Exclude : FindObjectsInactive.Include;
                    foundNavMeshSurfaces = UnityEngine.Object.FindObjectsByType<NavMeshSurface>(includeInactive, FindObjectsSortMode.None);
                    break;
            }

            // Actually rebake the meshes
            NavMeshUtil.BuildNavMeshAsync(foundNavMeshSurfaces);
        }
    }
}
