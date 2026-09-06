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
    public class NavMeshUpdater : MonoBehaviour
    {
        [Tooltip("NavMesh surfaces to update. Leave empty to update all active NavMesh surfaces.")]
        public NavMeshSurface[]? surfacesToUpdate;

        public void UpdateNavMesh()
        {
            // Did the moon creator specify surfaces for us to rebake
            NavMeshSurface[]? updateNavMesh = surfacesToUpdate;
            if (updateNavMesh == null || updateNavMesh.Length == 0)
            {
                // Just grab every active surface
                updateNavMesh = NavMeshSurface.s_NavMeshSurfaces.ToArray();
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
