using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshLib.Editor
{
    /// <summary>
    /// A helper mono behavior made to allow moon makers to automatically have <see cref="NavMeshSurface"/>s created for
    /// custom agent types at runtime. Since custom agent IDs are registered at runtime, custom <see cref="NavMeshSurface"/>s
    /// must be baked for them at runtime. This allows moon makers and other modders to have the surfaces auto created
    /// on object awake.
    /// </summary>
    /// <remarks>
    /// This object is not needed for Interior and OutsideNavMesh game object as NavMeshLib already handles those cases.
    /// </remarks>
    [RequireComponent(typeof(NavMeshSurface))]
    public class AutoNavSurfaceCreator : MonoBehaviour
    {
        /// <summary>
        /// The NavMeshSurface to copy.
        /// </summary>
        [Tooltip("The NavMeshSurface to copy.")]
        public NavMeshSurface navMeshSurface = null!;

        /// <summary>
        /// When creating the new <see cref="NavMeshSurface"/>s, should they be baked Asynchronously?
        /// </summary>
        /// <remarks>
        /// WARNING: <see cref="buildAsync"/> will be FORCED Synchronous if <see cref="navMeshSurface"/>'s GameObject is disabled. This is a Unity limitaition.
        /// </remarks>
        [Tooltip("When creating the new NavMeshSurfaces, should they be rebaked Asynchronously? \n WARNING: buildAsync will be FORCED Synchronous if navMeshSurface's GameObject is disabled. This is a Unity limitaition.")]
        public bool buildAsync = true;

        /// <summary>
        /// Should the surface to copy override the tile size set by the agent creator?
        /// </summary>
        /// <remarks>
        /// Does nothing if overrideTileSize is not set on the surface to copy.
        /// </remarks>
        [Tooltip("Should the surface to copy override the tile size set the by agent creator? \n Does nothing if overrideTileSize is not set on the surface to copy.")]
        public bool overrideTileSize = false;

        /// <summary>
        /// Should the surface to copy override the voxel size set by the agent creator?
        /// </summary>
        /// <remarks>
        /// Does nothing if overrideVoxelSize is not set on the surface to copy.
        /// </remarks>
        [Tooltip("Should the surface to copy override the voxel size set by the agent creator? \n Does nothing if overrideVoxelSize is not set on the surface to copy.")]
        public bool overrideVoxelSize = false;

        private void Start()
        {
            if (navMeshSurface == null)
            {
                Plugin.LogWarning($"[AutoNavSurfaceCreator] ({name}) navMeshSurface was not set, attempting to find NavMeshSurface component.");
                navMeshSurface = GetComponent<NavMeshSurface>();
                if (navMeshSurface == null)
                {
                    Plugin.LogError($"[AutoNavSurfaceCreator] ({name}) failed to find NavMeshSurface to copy! Skipping!");
                    return;
                }
            }

            // Simple enough, just use our internal helper
            Plugin.LogInfo($"[AutoNavSurfaceCreator] ({name}) starting auto creation of missing NavMeshSurface.");
            NavMeshUtil.AddMissingNavMeshSurfaces(navMeshSurface, buildAsync, overrideTileSize, overrideVoxelSize);
        }
    }
}
