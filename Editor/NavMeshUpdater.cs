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
        [Tooltip("What surfaces should be rebuild when UpdateNavMesh is called?")]
        public RebuildType rebuildType = RebuildType.AllSurfaces;

        [Tooltip("NavMesh surfaces to update when using SelectedSurfaces.")]
        public NavMeshSurface[]? surfacesToUpdate;

        /// <summary>
        /// Updates the current scene's NavMesh based on <see cref="rebuildType"/>
        /// </summary>
        public void UpdateNavMesh()
        {
            // Did the moon creator specify surfaces for us to rebake
            NavMeshSurface[] updateNavMesh;
            switch (rebuildType)
            {
                case RebuildType.OutsideSurfaces:
                    NavMeshUtil.RebakeExteriorNavMesh();
                    return;
                case RebuildType.InsideSurfaces:
                    NavMeshUtil.RebakeDunGenNavMesh();
                    return;
                case RebuildType.Custom:
                    updateNavMesh = surfacesToUpdate ?? Array.Empty<NavMeshSurface>();
                    if (updateNavMesh.Length == 0)
                    {
                        Plugin.LogWarning($"NavMeshUpdater on '{gameObject.name}' is configured for Custom but has no NavMesh surfaces assigned.");
                        return;
                    }
                    break;
                case RebuildType.ActiveSurfaces:
                case RebuildType.AllSurfaces:
                default:
                    var includeInactive = rebuildType == RebuildType.ActiveSurfaces ? FindObjectsInactive.Exclude : FindObjectsInactive.Include;
                    updateNavMesh = UnityEngine.Object.FindObjectsByType<NavMeshSurface>(includeInactive, FindObjectsSortMode.None);
                    break;
            }

            // Go and prep all surfaces to rebake
            for (int i = 0; i < updateNavMesh.Length; i++)
            {
                NavMeshSurface? navMeshSurface = updateNavMesh[i];
                if (navMeshSurface != null)
                {
                    // Actually build the mesh
                    NavMeshData navMeshData = navMeshSurface.navMeshData;
                    if (navMeshData == null)
                    {
                        navMeshData = new NavMeshData(navMeshSurface.GetBuildSettings().agentTypeID)
                        {
                            position = navMeshSurface.transform.position,
                            rotation = navMeshSurface.transform.rotation
                        };
                        navMeshSurface.navMeshData = navMeshData;
                    }
                    else
                    {
                        navMeshData.position = navMeshSurface.transform.position;
                        navMeshData.rotation = navMeshSurface.transform.rotation;
                    }
                }
            }

            // Actually rebake the meshes
            StartCoroutine(NavMeshUtil.UpdateNavMeshDelayed(updateNavMesh));
        }
    }
}
