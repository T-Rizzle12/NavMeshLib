using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshLib
{
    /// <summary>
    /// Utility class for NavMesh related functions
    /// </summary>
    public static class NavMeshUtil
    {
        #region Pathfinding

        /// <summary>
        /// Helper method that determines whether a complete and valid NavMesh path exists between two points.
        /// </summary>
        /// <remarks>
        /// This is an enhanced check that wraps <see cref="NavMesh.CalculatePath(Vector3, Vector3, int, NavMeshPath)"/> with additional validation:
        /// <list type="bullet">
        ///   <item>Ensures the path calculation succeeds</item>
        ///   <item>Confirms the path is not empty</item>
        ///   <item>Verifies that the last path corner is sufficiently close to the destination, ensuring the path is complete</item>
        /// </list>
        /// NOTE: This method only works with the default agent type of 0. If you need to use a different agent type or custom area costs, 
        /// use the overload that accepts a <see cref="NavMeshQueryFilter"/>.
        /// </remarks>
        /// <param name="startPosition">The starting position of the path</param>
        /// <param name="endPosition">The target position to reach</param>
        /// <param name="areaMask">The NavMesh area mask to use when calculating the path</param>
        /// <param name="path">A reference to the <see cref="NavMeshPath"/> that will contain the calculated path if valid</param>
        /// <param name="calculatePathDistance">This updates <paramref name="pathDistance"/> with the length of the path. <paramref name="pathDistance"/> is set to zero on failure</param>
        /// <param name="nearestNavAreaRange">The range the game will search for a nearby NavArea for targetPos</param>
        /// <param name="maxRangeToEnd">The maximum range the nearest NavArea will the path be considered vaild</param>
        /// <param name="pathDistance">The entire length of the path</param>
        /// <returns><see langword="true"/> if a valid and complete path exists; otherwise, <see langword="false"/></returns>

        public static bool IsValidPathToTarget(Vector3 startPosition, Vector3 endPosition, int areaMask, ref NavMeshPath path, out float pathDistance, bool calculatePathDistance = false, float nearestNavAreaRange = 2.7f, float maxRangeToEnd = 1.5f)
        {
            // Check if we can create a path there first!
            pathDistance = 0f;
            if (!NavMesh.CalculatePath(startPosition, endPosition, areaMask, path))
            {
                return false;
            }

            // Check to make sure the path is valid!
            Vector3[] storedPathCorners = path.corners;
            if (storedPathCorners.Length <= 0)
            {
                return false;
            }

            // This may be a partial path, make sure the end of the path actually reaches our target destiniation!
            if ((storedPathCorners[storedPathCorners.Length - 1] - RoundManager.Instance.GetNavMeshPosition(endPosition, RoundManager.Instance.navHit, nearestNavAreaRange)).sqrMagnitude > maxRangeToEnd * maxRangeToEnd)
            {
                return false;
            }

            // Calculate the path distance as needed
            if (calculatePathDistance)
            {
                for (int i = 1; i < storedPathCorners.Length; i++)
                {
                    pathDistance += Vector3.Distance(storedPathCorners[i - 1], storedPathCorners[i]);
                }
            }

            return true;
        }

        /// <summary>
        /// Helper method that determines whether a complete and valid NavMesh path exists between two points.
        /// </summary>
        /// <remarks>
        /// This is an enhanced check that wraps <see cref="NavMesh.CalculatePath(Vector3, Vector3, NavMeshQueryFilter, NavMeshPath)"/> with additional validation:
        /// <list type="bullet">
        ///   <item>Ensures the path calculation succeeds</item>
        ///   <item>Confirms the path is not empty</item>
        ///   <item>Verifies that the last path corner is sufficiently close to the destination, ensuring the path is complete</item>
        /// </list>
        /// </remarks>
        /// <param name="startPosition">The starting position of the path</param>
        /// <param name="endPosition">The target position to reach</param>
        /// <param name="queryFilter">A filter to use during the pathfinding</param>
        /// <param name="path">A reference to the <see cref="NavMeshPath"/> that will contain the calculated path if valid</param>
        /// <param name="calculatePathDistance">This updates <paramref name="pathDistance"/> with the length of the path. <paramref name="pathDistance"/> is set to zero on failure</param>
        /// <param name="nearestNavAreaRange">The range the game will search for a nearby NavArea for targetPos</param>
        /// <param name="maxRangeToEnd">The maximum range the nearest NavArea will the path be considered vaild</param>
        /// <param name="pathDistance">The entire length of the path</param>
        /// <returns><see langword="true"/> if a valid and complete path exists; otherwise, <see langword="false"/></returns>

        public static bool IsValidPathToTarget(Vector3 startPosition, Vector3 endPosition, NavMeshQueryFilter queryFilter, ref NavMeshPath path, out float pathDistance, bool calculatePathDistance = false, float nearestNavAreaRange = 2.7f, float maxRangeToEnd = 1.5f)
        {
            // Check if we can create a path there first!
            pathDistance = 0f;
            if (!NavMesh.CalculatePath(startPosition, endPosition, queryFilter, path))
            {
                return false;
            }

            // Check to make sure the path is valid!
            Vector3[] storedPathCorners = path.corners;
            if (storedPathCorners.Length <= 0)
            {
                return false;
            }

            // This may be a partial path, make sure the end of the path actually reaches our target destiniation!
            if ((storedPathCorners[storedPathCorners.Length - 1] - RoundManager.Instance.GetNavMeshPosition(endPosition, RoundManager.Instance.navHit, nearestNavAreaRange)).sqrMagnitude > maxRangeToEnd * maxRangeToEnd)
            {
                return false;
            }

            // Calculate the path distance as needed
            if (calculatePathDistance)
            {
                for (int i = 1; i < storedPathCorners.Length; i++)
                {
                    pathDistance += Vector3.Distance(storedPathCorners[i - 1], storedPathCorners[i]);
                }
            }

            return true;
        }

        /// <summary>
        /// Helper method that determines whether a complete and valid NavMesh path exists between two points for the given agent.
        /// </summary>
        /// <param name="navMeshAgent">The agent to test the path for</param>
        /// <inheritdoc cref="IsValidPathToTarget(Vector3, Vector3, NavMeshQueryFilter, ref NavMeshPath, out float, bool, float, float)"/>
        #pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        public static bool IsValidPathToTarget(this NavMeshAgent navMeshAgent, Vector3 startPosition, Vector3 endPosition, ref NavMeshPath path, out float pathDistance, bool calculatePathDistance = false, float nearestNavAreaRange = 2.7f, float maxRangeToEnd = 1.5f)
        #pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        {
            // Get the area costs from the agent
            float[] costs = new float[32];
            if (navMeshAgent.enabled)
            {
                // If the agent is enabled, we can get the area costs directly from the agent
                for (int i = 0; i < costs.Length; i++)
                {
                    costs[i] = navMeshAgent.GetAreaCost(i);
                }
            }
            else
            {
                // If the agent is disabled, we can still get the area costs from the NavMesh itself
                // This is the default area costs as defined in the Unity Editor.
                for (int i = 0; i < costs.Length; i++)
                {
                    costs[i] = NavMesh.GetAreaCost(i);
                }
            }

            NavMeshQueryFilter navMeshQuery = new NavMeshQueryFilter() { agentTypeID = navMeshAgent.agentTypeID, areaMask = navMeshAgent.areaMask, costs = costs };
            return IsValidPathToTarget(startPosition, endPosition, navMeshQuery, ref path, out pathDistance, calculatePathDistance, nearestNavAreaRange, maxRangeToEnd);
        }

        #endregion

        #region NavArea Utilities

        /// <summary>
        /// Helper method that returns the nearest valid NavMesh position to the given position. 
        /// If no valid NavMesh position is found, the original position is returned.
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="RoundManager.GetNavMeshPosition(Vector3, NavMeshHit, float, int)"/>, 
        /// but allows you to specify a <see cref="NavMeshQueryFilter"/> for custom agent types.
        /// </remarks>
        /// <param name="roundManager">The RoundManager instance.</param>
        /// <param name="pos">The position to sample.</param>
        /// <param name="queryFilter">The query filter to use for sampling.</param>
        /// <param name="navMeshHit">Output parameter containing the NavMesh hit information.</param>
        /// <param name="sampleRadius">The radius within which to sample the NavMesh.</param>
        /// <returns>The nearest valid NavMesh position, or the original position if none is found.</returns>
        public static Vector3 GetNavMeshPosition(this RoundManager roundManager, Vector3 pos, NavMeshQueryFilter queryFilter, NavMeshHit navMeshHit = default(NavMeshHit), float sampleRadius = 5f)
        {
            if (NavMesh.SamplePosition(pos, out navMeshHit, sampleRadius, queryFilter))
            {
                roundManager.GotNavMeshPositionResult = true;
                return navMeshHit.position;
            }
            roundManager.GotNavMeshPositionResult = false;
            return pos;
        }

        /// <summary>
        /// Helper method that returns a random valid NavMesh position for the given radius. 
        /// If no valid NavMesh position is found, the original position is returned.
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="RoundManager.GetRandomNavMeshPositionInRadius(Vector3, float, NavMeshHit)"/>, 
        /// but allows you to specify a <see cref="NavMeshQueryFilter"/> for custom agent types.
        /// </remarks>
        /// <param name="roundManager">The RoundManager instance.</param>
        /// <param name="pos">The position to sample.</param>
        /// <param name="queryFilter">The query filter to use for sampling.</param>
        /// <param name="radius">The radius within which to sample the NavMesh.</param>
        /// <param name="navHit">Output parameter containing the NavMesh hit information.</param>
        /// <returns>A random valid NavMesh position within the specified radius.</returns>
        public static Vector3 GetRandomNavMeshPositionInRadius(this RoundManager roundManager, Vector3 pos, NavMeshQueryFilter queryFilter, float radius = 10f, NavMeshHit navHit = default(NavMeshHit))
        {
            float y = pos.y;
            pos = UnityEngine.Random.insideUnitSphere * radius + pos;
            pos.y = y;
            if (NavMesh.SamplePosition(pos, out navHit, radius, queryFilter))
            {
                return navHit.position;
            }
            return pos;
        }

        /// <summary>
        /// Helper method that returns the nearest valid NavMesh position to the given position. 
        /// If no valid NavMesh position is found, the original position is returned.
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="RoundManager.GetRandomNavMeshPositionInBoxPredictable(Vector3, float, NavMeshHit, System.Random, int, float)"/>, 
        /// but allows you to specify a <see cref="NavMeshQueryFilter"/> for custom agent types.
        /// </remarks>
        /// <param name="roundManager">The RoundManager instance.</param>
        /// <param name="pos">The position to sample.</param>
        /// <param name="queryFilter">The query filter to use for sampling.</param>
        /// <param name="radius">The radius within which to sample the NavMesh.</param>
        /// <param name="navHit">Output parameter containing the NavMesh hit information.</param>
        /// <param name="randomSeed">The random seed to use for generating the random position.</param>
        /// <param name="verticalScale">The vertical scale to apply to the random position.</param>
        /// <returns>A random valid NavMesh position within the specified radius.</returns>
        public static Vector3 GetRandomNavMeshPositionInBoxPredictable(this RoundManager roundManager, Vector3 pos, NavMeshQueryFilter queryFilter, float radius = 10f, NavMeshHit navHit = default(NavMeshHit), System.Random randomSeed = null!, float verticalScale = 1f)
        {
            RoundManager instanceRM = RoundManager.Instance;
            float y = pos.y;
            float x = instanceRM.RandomNumberInRadius(radius, randomSeed);
            float y2 = instanceRM.RandomNumberInRadius(radius * verticalScale, randomSeed);
            float z = instanceRM.RandomNumberInRadius(radius, randomSeed);
            Vector3 vector = new Vector3(x, y2, z) + pos;
            vector.y = y;
            roundManager.randomPositionInBoxRadius = vector;
            float num = Vector3.Distance(pos, vector);
            if (NavMesh.SamplePosition(vector, out navHit, num + 2f, queryFilter))
            {
                roundManager.GotNavMeshPositionResult = true;
                return navHit.position;
            }
            roundManager.GotNavMeshPositionResult = false;
            return pos;
        }

        /// <summary>
        /// Helper method that returns a random valid NavMesh position within a specified box defined by min and max coordinates.
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="RoundManager.GetRandomNavMeshPositionInBoxWithLimits(Vector3, float, float, float, float, float, float, float, NavMeshHit, System.Random, int, float)"/>, 
        /// but allows you to specify a <see cref="NavMeshQueryFilter"/> for custom agent types.
        /// </remarks>
        /// <param name="roundManager">The RoundManager instance.</param>
        /// <param name="pos">The position to sample.</param>
        /// <param name="queryFilter">The query filter to use for sampling.</param>
        /// <param name="minX">The minimum X coordinate of the box.</param>
        /// <param name="maxX">The maximum X coordinate of the box.</param>
        /// <param name="minZ">The minimum Z coordinate of the box.</param>
        /// <param name="maxZ">The maximum Z coordinate of the box.</param>
        /// <param name="minY">The minimum Y coordinate of the box.</param>
        /// <param name="maxY">The maximum Y coordinate of the box.</param>
        /// <param name="radius">The radius within which to sample the NavMesh.</param>
        /// <param name="navHit">Output parameter containing the NavMesh hit information.</param>
        /// <param name="randomSeed">The random seed to use for generating the random position.</param>
        /// <param name="verticalRange">The vertical range to apply to the random position.</param>
        /// <returns>A random valid NavMesh position within the specified box.</returns>
        public static Vector3 GetRandomNavMeshPositionInBoxWithLimits(this RoundManager roundManager, Vector3 pos, NavMeshQueryFilter queryFilter, float minX, float maxX, float minZ, float maxZ, float minY, float maxY, float radius = 10f, NavMeshHit navHit = default(NavMeshHit), System.Random randomSeed = null!, float verticalRange = 3f)
        {
            minX = Mathf.Max(minX, pos.x - radius);
            minY = Mathf.Max(minY, pos.y - radius);
            minZ = Mathf.Max(minZ, pos.z - radius);
            maxX = Mathf.Min(maxX, pos.x + radius);
            maxY = Mathf.Min(maxY, pos.y + radius);
            maxZ = Mathf.Min(maxZ, pos.z + radius);
            float x = math.remap(0f, 1f, minX, maxX, (float)randomSeed.NextDouble());
            float z = math.remap(0f, 1f, minZ, maxZ, (float)randomSeed.NextDouble());
            float value = math.remap(0f, 1f, minY, maxY, (float)randomSeed.NextDouble());
            value = Mathf.Clamp(value, pos.y - verticalRange, pos.y + verticalRange);
            Vector3 vector = new Vector3(x, value, z);
            if (Physics.Raycast(vector, Vector3.down, out var hitInfo, 5f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                vector = hitInfo.point;
            }
            float num = Vector3.Distance(pos, vector);
            if (NavMesh.SamplePosition(vector, out navHit, num + 2f, queryFilter))
            {
                return navHit.position;
            }
            return pos;
        }

        /// <summary>
        /// Helper method that returns a random valid NavMesh position within a specified box defined by a center point and radii along each axis.
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="RoundManager.GetRandomNavMeshPositionInBoxPredictable(Vector3, float, float, float, NavMeshHit, System.Random, int)"/>, 
        /// but allows you to specify a <see cref="NavMeshQueryFilter"/> for custom agent types.
        /// </remarks>
        /// <param name="roundManager">The RoundManager instance.</param>
        /// <param name="center">The center point of the box.</param>
        /// <param name="queryFilter">The query filter to use for sampling.</param>
        /// <param name="radiusX">The radius along the X axis.</param>
        /// <param name="radiusY">The radius along the Y axis.</param>
        /// <param name="radiusZ">The radius along the Z axis.</param>
        /// <param name="navHit">Output parameter containing the NavMesh hit information.</param>
        /// <param name="randomSeed">The random seed to use for generating the random position.</param>
        /// <returns>A random valid NavMesh position within the specified box.</returns>
        public static Vector3 GetRandomNavMeshPositionInBoxPredictable(this RoundManager roundManager, Vector3 center, NavMeshQueryFilter queryFilter, float radiusX = 10f, float radiusY = 10f, float radiusZ = 10f, NavMeshHit navHit = default(NavMeshHit), System.Random randomSeed = null!)
        {
            float y = center.y;
            float x = roundManager.RandomNumberInRadius(radiusX, randomSeed);
            float y2 = roundManager.RandomNumberInRadius(radiusY, randomSeed);
            float z = roundManager.RandomNumberInRadius(radiusZ, randomSeed);
            Vector3 sourcePosition = new Vector3(x, y2, z) + center;
            sourcePosition.y = y;
            const float maxDistance = 8f;
            if (NavMesh.SamplePosition(sourcePosition, out navHit, maxDistance, queryFilter))
            {
                return navHit.position;
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Helper method that returns a random valid NavMesh position within a specified spherical radius around a given position.
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="RoundManager.GetRandomNavMeshPositionInRadiusSpherical(Vector3, float, NavMeshHit)"/>, 
        /// but allows you to specify a <see cref="NavMeshQueryFilter"/> for custom agent types.
        /// </remarks>
        /// <param name="roundManager">The RoundManager instance.</param>
        /// <param name="pos">The center position of the spherical area.</param>
        /// <param name="queryFilter">The query filter to use for sampling.</param>
        /// <param name="radius">The radius of the spherical area.</param>
        /// <param name="navHit">Output parameter containing the NavMesh hit information.</param>
        /// <returns>A random valid NavMesh position within the specified spherical radius.</returns>
        public static Vector3 GetRandomNavMeshPositionInRadiusSpherical(this RoundManager roundManager, Vector3 pos, NavMeshQueryFilter queryFilter, float radius = 10f, NavMeshHit navHit = default(NavMeshHit))
        {
            pos = UnityEngine.Random.insideUnitSphere * radius + pos;
            if (NavMesh.SamplePosition(pos, out navHit, radius + 2f, queryFilter))
            {
                Debug.DrawRay(pos + Vector3.forward * 0.01f, Vector3.up * 2f, Color.blue);
                return navHit.position;
            }
            Debug.DrawRay(pos + Vector3.forward * 0.01f, Vector3.up * 2f, Color.yellow);
            return pos;
        }

        #endregion

        #region Lethal Company Specific

        /// <summary>
        /// Helper method that determines whether a complete and valid NavMesh path exists between two points for the given agent.
        /// </summary>
        /// <param name="enemyAI">The <see cref="EnemyAI"/> to test the path for</param>
        /// <inheritdoc cref="IsValidPathToTarget(Vector3, Vector3, NavMeshQueryFilter, ref NavMeshPath, out float, bool, float, float)"/>
        #pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidPathToTarget(this EnemyAI enemyAI, Vector3 startPosition, Vector3 endPosition, ref NavMeshPath path, out float pathDistance, bool calculatePathDistance = false, float nearestNavAreaRange = 2.7f, float maxRangeToEnd = 1.5f)
        #pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        {
            return enemyAI.agent.IsValidPathToTarget(startPosition, endPosition, ref path, out pathDistance, calculatePathDistance, nearestNavAreaRange, maxRangeToEnd);
        }

        /// <summary>
        /// A helper function that rebakes the entire exterior NavMesh
        /// </summary>
        /// <param name="environmentObject">When rebaking the exterior NavMesh, you can pass in a custom object. If null, we attempt to find OutsideLevelNavMesh ourself.</param>
        /// <param name="generateNewSurfaces">Should we create new <see cref="NavMeshSurface"/>s if they don't exist for custom agent types</param>
        public static void RebakeExteriorNavMesh(GameObject? environmentObject = null, bool generateNewSurfaces = false)
        {
            // Did the user give us the enviorment object
            if (environmentObject == null)
            {
                environmentObject = GameObject.FindGameObjectWithTag("OutsideLevelNavMesh");
            }

            // Lets go and regen the outside NavMesh
            if (environmentObject != null)
            {
                // Should we add missing surfaces
                HashSet<NavMeshSurface> navMeshSurfaces = new HashSet<NavMeshSurface>();
                if (generateNewSurfaces)
                {
                    // This exists to allow us to add custom NavMeshSurfaces for all custom agent
                    // types that were registered earlier
                    NavMeshSurface existingSurface = environmentObject.GetComponent<NavMeshSurface>();
                    int settingsCount = NavMesh.GetSettingsCount();
                    for (int i = 0; i < settingsCount; i++)
                    {
                        // Get the settings and the already existing surfaces
                        NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(i);
                        NavMeshSurface navMeshSurface = (from s in environmentObject.GetComponents<NavMeshSurface>()
                                                         where s.agentTypeID == settings.agentTypeID
                                                         select s).FirstOrDefault();
                        Plugin.LogDebug($"Checking NavMeshSurface for agent ID {settings.agentTypeID} at index {i}. Exterior surface null? {navMeshSurface == null}");
                        if (navMeshSurface == null)
                        {
                            // Copy what the other exterior NavmeshSurface had
                            navMeshSurface = environmentObject.AddComponent<NavMeshSurface>();
                            navMeshSurface.agentTypeID = settings.agentTypeID;
                            navMeshSurface.defaultArea = existingSurface.defaultArea;
                            navMeshSurface.useGeometry = existingSurface.useGeometry;
                            navMeshSurface.collectObjects = existingSurface.collectObjects;
                            if (existingSurface.collectObjects == CollectObjects.Volume)
                            {
                                navMeshSurface.center = existingSurface.center;
                                navMeshSurface.size = existingSurface.size;
                            }
                            navMeshSurface.layerMask = existingSurface.layerMask;
                            navMeshSurface.minRegionArea = existingSurface.minRegionArea;

                            // This is how Loadstone used to do it
                            NavMeshData navMeshData = navMeshSurface.navMeshData;
                            if (navMeshData == null)
                            {
                                // This is how BakeNavMesh creates the new data struct, we mimic that here 
                                navMeshData = new NavMeshData(navMeshSurface.GetBuildSettings().agentTypeID)
                                {
                                    position = navMeshSurface.transform.position,
                                    rotation = navMeshSurface.transform.rotation
                                };
                                navMeshSurface.navMeshData = navMeshData;
                            }
                            else
                            {
                                // Make sure we have the correct position and rotation
                                navMeshData.position = navMeshSurface.transform.position;
                                navMeshData.rotation = navMeshSurface.transform.rotation;
                            }
                        }

                        // Store the new surface
                        navMeshSurfaces.Add(navMeshSurface);
                    }
                }

                // Rebake the NavMeshes
                NavMeshSurface[] surfacesToUpdate = navMeshSurfaces.Count > 0 ? navMeshSurfaces.ToArray() : environmentObject.GetComponents<NavMeshSurface>();
                Plugin.LogInfo($"[RebakeExteriorNavMesh] Rebaking {surfacesToUpdate.Length} surface(s)");
                RoundManager.Instance.StartCoroutine(UpdateNavMeshDelayed(surfacesToUpdate));
            }
            else
            {
                Plugin.LogFatal($"[RebakeExteriorNavMesh] Failed to find environment object.......this should NEVER happen.");
            }
        }

        /// <summary>
        /// A helper function that rebakes the entire dungen NavMesh
        /// </summary>
        public static void RebakeDunGenNavMesh()
        {
            RoundManager instanceRM = RoundManager.Instance;
            if (instanceRM != null)
            {
                NavMeshSurface[] surfacesToUpdate = instanceRM.fullBakeSurfaces.ToArray();
                Plugin.LogInfo($"[RebakeDunGenNavMesh] Rebaking {surfacesToUpdate.Length} surface(s)");
                instanceRM.StartCoroutine(UpdateNavMeshDelayed(surfacesToUpdate));
            }
        }

        #endregion

        #region NavMesh Generation and Updates

        /// <summary>
        /// Asynchronously rebuilds the NavMesh for the current <see cref="NavMeshSurface"/>
        /// </summary>
        /// <param name="navMeshSurface">The surface to bake for.</param>
        /// <param name="onBuildCompleted">Called once the build is finished.</param>
        /// <param name="onSurfaceBuilt">Called once a <see cref="NavMeshSurface"/> is fully rebaked.</param>
        /// <returns>A coroutine allowing you to cancel the operation early.</returns>
        public static void BuildNavMeshAsync(this NavMeshSurface navMeshSurface, Action? onBuildCompleted = null, Action<NavMeshSurface>? onSurfaceBuilt = null)
        {
            // This is how Loadstone used to do it
            NavMeshData navMeshData = navMeshSurface.navMeshData;
            if (navMeshData == null)
            {
                // This is how BakeNavMesh creates the new data struct, we mimic that here 
                navMeshData = new NavMeshData(navMeshSurface.GetBuildSettings().agentTypeID)
                {
                    position = navMeshSurface.transform.position,
                    rotation = navMeshSurface.transform.rotation
                };
                navMeshSurface.navMeshData = navMeshData;
            }
            else
            {
                // Make sure we have the correct position and rotation
                navMeshData.position = navMeshSurface.transform.position;
                navMeshData.rotation = navMeshSurface.transform.rotation;
            }

            // Actually update the NavMesh
            navMeshSurface.StartCoroutine(UpdateNavMeshDelayed(navMeshSurface, onBuildCompleted, onSurfaceBuilt));
        }

        /// <summary>
        /// Asynchronously rebuilds the NavMesh for the array of <see cref="NavMeshSurface"/>s
        /// </summary>
        /// <param name="surfacesToUpdate">The surfaces to rebake.</param>
        /// <param name="onBuildCompleted">Called once the build is finished.</param>
        /// <param name="onSurfaceBuilt">Called once a <see cref="NavMeshSurface"/> is fully rebaked.</param>
        /// <returns></returns>
        public static IEnumerator UpdateNavMeshDelayed(NavMeshSurface[] surfacesToUpdate, Action? onBuildCompleted = null, Action<NavMeshSurface>? onSurfaceBuilt = null)
        {
            // Wait a frame to make sure everything else has loaded
            yield return null;

            // The game keeps a cache of all of the surfaces that were used for the full bake
            // of the dungeon, I can just loop through those and call UpdateNavMesh!
            AdjacentRoomCullingModified roomCullingModified = StartOfRound.Instance.occlusionCuller;
            bool wasEnabled = roomCullingModified.enabled;
            for (int i = 0; i < surfacesToUpdate.Length; i++)
            {
                NavMeshSurface? navMeshSurface = surfacesToUpdate[i];
                if (navMeshSurface != null)
                {
                    // Log about what we are updating!
                    Plugin.LogInfo($"Updating NavMesh for surface {navMeshSurface}.");

                    // NOTE: The vanilla game culling causes the NavMesh Generation to fail. Need to force everything to render
                    // before we can safely rebuild the mesh!
                    if (roomCullingModified != null && roomCullingModified.enabled)
                    {
                        wasEnabled = true;
                        roomCullingModified.enabled = false;
                    }

                    // Build our new mesh!
                    AsyncOperation asyncOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
                    while (asyncOperation != null && !asyncOperation.isDone)
                    {
                        Plugin.LogDebug($"{navMeshSurface} Rebuild Progress {asyncOperation.progress * 100}%");
                        yield return null;
                    }

                    // Update the NavMeshData!
                    Plugin.LogInfo($"{navMeshSurface} UpdateNavMesh finished, refreshing surface data.");
                    navMeshSurface.navMeshData.name = navMeshSurface.gameObject.name;
                    navMeshSurface.RemoveData();
                    Plugin.LogDebug("Removed existing data.");
                    if (navMeshSurface.isActiveAndEnabled)
                    {
                        navMeshSurface.AddData();
                        Plugin.LogDebug("Added updated data.");
                    }

                    // Call the related action
                    onSurfaceBuilt?.Invoke(navMeshSurface);
                }
            }

            // Turn the vanilla game culling back on!
            roomCullingModified ??= StartOfRound.Instance.occlusionCuller;
            if (roomCullingModified != null && roomCullingModified.enabled != wasEnabled)
            {
                roomCullingModified.enabled = wasEnabled;
            }

            // Let the user know the build has finished
            onBuildCompleted?.Invoke();
            Plugin.LogInfo($"Updated {surfacesToUpdate.Length} NavMeshe(s).");
        }

        private static IEnumerator UpdateNavMeshDelayed(NavMeshSurface surfaceToUpdate, Action? onBuildCompleted = null, Action<NavMeshSurface>? onSurfaceBuilt = null)
        {
            // Wait a frame to make sure everything else has loaded
            yield return null;

            // The game keeps a cache of all of the surfaces that were used for the full bake
            // of the dungeon, I can just loop through those and call UpdateNavMesh!
            AdjacentRoomCullingModified roomCullingModified = StartOfRound.Instance.occlusionCuller;
            bool wasEnabled = roomCullingModified.enabled;
            if (surfaceToUpdate != null)
            {
                // Log about what we are updating!
                Plugin.LogInfo($"Updating NavMesh for surface {surfaceToUpdate}.");

                // NOTE: The vanilla game culling causes the NavMesh Generation to fail. Need to force everything to render
                // before we can safely rebuild the mesh!
                if (roomCullingModified != null && roomCullingModified.enabled)
                {
                    wasEnabled = true;
                    roomCullingModified.enabled = false;
                }

                // Build our new mesh!
                AsyncOperation asyncOperation = surfaceToUpdate.UpdateNavMesh(surfaceToUpdate.navMeshData);
                while (asyncOperation != null && !asyncOperation.isDone)
                {
                    Plugin.LogDebug($"{surfaceToUpdate} Rebuild Progress {asyncOperation.progress * 100}%");
                    yield return null;
                }

                // Update the NavMeshData!
                Plugin.LogInfo($"{surfaceToUpdate} UpdateNavMesh finished, refreshing surface data.");
                surfaceToUpdate.navMeshData.name = surfaceToUpdate.gameObject.name;
                surfaceToUpdate.RemoveData();
                Plugin.LogDebug("Removed existing data.");
                if (surfaceToUpdate.isActiveAndEnabled)
                {
                    surfaceToUpdate.AddData();
                    Plugin.LogDebug("Added updated data.");
                }

                // Call the related action
                onSurfaceBuilt?.Invoke(surfaceToUpdate);
            }

            // Turn the vanilla game culling back on!
            roomCullingModified ??= StartOfRound.Instance.occlusionCuller;
            if (roomCullingModified != null && roomCullingModified.enabled != wasEnabled)
            {
                roomCullingModified.enabled = wasEnabled;
            }

            // Let the user know the build has finished
            onBuildCompleted?.Invoke();
            Plugin.LogInfo($"Finished updating {surfaceToUpdate}.");
        }

        #endregion
    }
}
