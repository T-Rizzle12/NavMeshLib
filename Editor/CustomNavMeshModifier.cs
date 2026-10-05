using DunGen;
using NavMeshLib.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;

namespace NavMeshLib.Editor
{
    /// <summary>
    /// A helper mono behavior that allows for dynamic creation of <seealso cref="NavMeshModifierVolume"/>s
    /// for custom interiors, traps, and so much more.
    /// </summary>
    /// <remarks>
    /// Vanilla moons and Interior Navmesh only grab child objects. This causes objects such as traps with <see cref="NavMeshModifier"/>s to
    /// not affect the NavMesh. <br/>
    /// This works around the issue by creating a proxy object to parent itself to the dugeon or exterior NavMesh 
    /// so it will be consider for NavMesh generation purposes. <br/>
    /// </remarks>
    [RequireComponent(typeof(NavMeshUpdater))]
    public class CustomNavMeshModifier : MonoBehaviour
    {
        /// <summary>
        /// What object should this be parented to after initialization.
        /// </summary>
        [Tooltip("What object should this be parented to after initialization.")]
        public ModifierParent modifierLocation = ModifierParent.Interior;

        /// <summary>
        /// The object to parent our NavMeshModifierVolume(s) to when using Custom.
        /// </summary>
        [Tooltip("The object to parent our NavMeshModifierVolume(s) to when using Custom.")]
        public GameObject? parentObject = null;

        /// <summary>
        /// The GameObject with the modifier(s) and modifier volume(s) to reparent.
        /// </summary>
        /// <remarks>
        /// NOTE: This will be reparented to whatever <see cref="modifierLocation"/> is set to after initialization
        /// </remarks>
        [Tooltip("The GameObject with the modifier(s) and modifier volume(s) to reparent. \n NOTE: This will be reparented to whatever \"modifierLocation\" is set to after initialization")]
        public GameObject rootModifierObject = null!;

        /// <summary>
        /// Should the NavMesh be rebuilt on initialization?
        /// </summary>
        /// <remarks>
        /// NOTE: If you are creating this at runtime and plan to rebake yourself, you should disable this.
        /// </remarks>
        [Tooltip("Should the NavMesh be rebuilt on initialization? \n NOTE: If you are creating this at runtime and plan to rebake yourself, you should disable this.")]
        public bool autoRebuildNavMesh = true;

        /// <summary>
        /// Should the NavMesh be rebuilt if the object is moved?
        /// </summary>
        [Tooltip("Should the NavMesh be rebuilt if the object is moved?")]
        public bool autoRebuildNavMeshOnMovement = false;

        /// <summary>
        /// How far the object must move from it's last position before it's considered changed.
        /// </summary>
        [Tooltip("How far the object must move from it's last position before it's considered changed.")]
        public float autoRebuildMoveThreshold = 0.1f;

        /// <summary>
        /// Should we only rebuild if we are stationary?
        /// </summary>
        [Tooltip("Should we only rebuild if we are stationary?")]
        public bool autoRebuildWhenStationary = false;

        /// <summary>
        /// How much time should pass, in seconds, without movement before we consider ourself stationary?
        /// </summary>
        [Tooltip("How much time should pass, in seconds, without movement before we consider ourself stationary?")]
        public float autoRebuildTimeToStationary = 0.5f;

        /// <summary>
        /// Should we initialize on start?
        /// </summary>
        /// <remarks>
        /// NOTE: If you have a custom script, you can manually call InitializeCustomModifier yourself.
        /// </remarks>
        [Tooltip("Should we initialize on start? \n NOTE: If you have a custom script, you can manually call InitializeCustomModifier yourself.")]
        public bool initOnStart = true;

        /// <summary>
        /// The <see cref="NavMeshUpdater"/> used for rebaking the NavMesh.
        /// </summary>
        /// <remarks>
        /// If your object can be inside and outside, it's recommend to set rebaking to all active surfaces.
        /// </remarks>
        [Tooltip("The NavMeshUpdater used for rebaking the NavMesh. \n NOTE: If your object can be inside and outside, it's recommend to set rebaking to all active surfaces.")]
        public NavMeshUpdater navMeshUpdater = null!;

        private NavMeshModifierVolume[] navMeshModifierVolumes = null!;

        private NavMeshModifier[] navMeshModifiers = null!;

        private bool initialized = false;

        private Vector3 lastPosition;

        private Quaternion lastRotation;

        private bool shouldUpdate = false;

        private float timeStationary = 0.0f;

        private bool isDestroying = false;

        private void Awake()
        {
            // Only do this if we were not provided one already
            if (navMeshUpdater == null)
            {
                navMeshUpdater = GetComponent<NavMeshUpdater>(); // Check if the user forgot to set it.
                if (navMeshUpdater == null)
                {
                    navMeshUpdater = gameObject.AddComponent<NavMeshUpdater>(); // Create one with default settings
                }
            }
        }

        private void Start()
        {
            if (initOnStart)
            {
                InitializeCustomModifier();
            }
        }

        private void OnEnable()
        {
            // Don't do this if we are not initialized
            if (!initialized) return;

            // Enable our proxy object and all of it's modifiers
            if (rootModifierObject != null)
            {
                // Set the root as active
                rootModifierObject.SetActive(true);

                // Update the NavMesh we are attached to.
                if (navMeshUpdater != null)
                {
                    navMeshUpdater.UpdateNavMesh();
                }
            }
        }

        private void OnDisable()
        {
            // Don't do this if we are not initialized or
            // if we are being destroyed (OnDestroy will handle this)
            if (!initialized || isDestroying) return;

            // Disable our proxy object and all of it's modifiers
            if (rootModifierObject != null)
            {
                // Set the root as inactive
                rootModifierObject.SetActive(false);

                // Update the NavMesh we are attached to.
                if (navMeshUpdater != null)
                {
                    navMeshUpdater.UpdateNavMesh();
                }
            }
        }

        /// <summary>
        /// Helper function for runtime creation of <see cref="CustomNavMeshModifier"/>s.
        /// </summary>
        /// <remarks>
        /// NOTE: This sets <see cref="initOnStart"/> and <see cref="autoRebuildNavMesh"/> to false on the newly created object.
        /// </remarks>
        /// <param name="rootObject">The root GameObject that will be used.</param>
        /// <param name="collider">The collider to use for runtime creation.</param>
        /// <param name="area">The NavMeshLayer for this modifier to affect.</param>
        /// <param name="affectedAgents">Agent ids to affect with this modifier.</param>
        /// <param name="buffer">A buffer to add to the current size of the collider.</param>
        /// <param name="modifierLocation">What object should this be parented to after initialization.</param>
        /// <param name="parentObject">The object to parent our <see cref="NavMeshModifierVolume"/> to when using Custom.</param>
        /// <returns>The newly created <see cref="CustomNavMeshModifier"/>.</returns>
        public static CustomNavMeshModifier CreateFromCollider(GameObject rootObject, Collider collider, int area, List<int> affectedAgents, Vector3? buffer = null, ModifierParent modifierLocation = ModifierParent.Custom, GameObject? parentObject = null)
        {
            // Make the "root" object
            buffer ??= Vector3.zero;
            CustomNavMeshModifier modifier = CreateBaseModifier(rootObject, modifierLocation, parentObject);
            GameObject proxyObject = modifier.rootModifierObject;

            // Create a child object and set it to where the collider is.
            CreateModifierVolume(proxyObject, collider, area, affectedAgents, buffer.Value);

            return modifier;
        }

        /// <summary>
        /// Helper function for runtime creation of <see cref="CustomNavMeshModifier"/>s.
        /// </summary>
        /// <remarks>
        /// NOTE: This sets <see cref="initOnStart"/> and <see cref="autoRebuildNavMesh"/> to <see langword="false"/> on the newly created object(s).
        /// </remarks>
        /// <param name="rootObject">The root GameObject that will be used.</param>
        /// <param name="colliders">The colliders to use for runtime creation.</param>
        /// <param name="area">The NavMeshLayer for this modifier to affect.</param>
        /// <param name="affectedAgents">Agent ids to affect with this modifier.</param>
        /// <param name="buffer">Additional world-space size added to the generated modifier volume.</param>
        /// <param name="modifierLocation">What object should this be parented to after initialization.</param>
        /// <param name="parentObject">The object to parent our <see cref="NavMeshModifierVolume"/>(s) to when using Custom.</param>
        /// <returns>The newly created <see cref="CustomNavMeshModifier"/></returns>
        public static CustomNavMeshModifier CreateFromColliders(GameObject rootObject, Collider[] colliders, int area, List<int> affectedAgents, Vector3? buffer = null, ModifierParent modifierLocation = ModifierParent.Custom, GameObject? parentObject = null)
        {
            // Make the "root" object
            buffer ??= Vector3.zero;
            CustomNavMeshModifier modifier = CreateBaseModifier(rootObject, modifierLocation, parentObject);
            GameObject proxyObject = modifier.rootModifierObject;

            // Loop through and create each custom modifier
            for (int i = 0; i < colliders.Length; i++)
            {
                // Sanity check
                var collider = colliders[i];
                if (collider != null)
                {
                    // Create a child object and set it to where the collider is.
                    CreateModifierVolume(proxyObject, collider, area, affectedAgents, buffer.Value);
                }
            }

            return modifier;
        }

        /// <summary>
        /// Helper for creating a empty <see cref="CustomNavMeshModifier"/>.
        /// </summary>
        /// <param name="rootObject"></param>
        /// <param name="modifierLocation"></param>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        private static CustomNavMeshModifier CreateBaseModifier(GameObject rootObject, ModifierParent modifierLocation, GameObject? parentObject)
        {
            // Create proxy object
            GameObject proxyObject = new GameObject($"{rootObject.name}-NavMeshProxy");
            proxyObject.transform.SetPositionAndRotation(rootObject.transform.position, rootObject.transform.rotation);
            //proxyObject.transform.localScale = rootObject.transform.lossyScale;
            proxyObject.layer = LayerMask.NameToLayer("NavigationSurface");

            // Make our custom modifier and add it to the root object
            CustomNavMeshModifier modifier = rootObject.AddComponent<CustomNavMeshModifier>();
            modifier.rootModifierObject = proxyObject;
            modifier.parentObject = parentObject;
            modifier.initOnStart = false;
            modifier.modifierLocation = modifierLocation;
            modifier.autoRebuildNavMesh = false;

            // Make sure we have a NavMeshUpdater
            modifier.navMeshUpdater ??= modifier.GetComponent<NavMeshUpdater>(); // See if the NavMeshUpdater already exists on the root object
            if (modifier.navMeshUpdater == null)
            {
                modifier.navMeshUpdater = modifier.gameObject.AddComponent<NavMeshUpdater>(); // Create one with default settings
            }

            return modifier;
        }

        /// <summary>
        /// Helper for creating custom <see cref="NavMeshModifierVolume"/>(s) for the given <paramref name="collider"/>.
        /// </summary>
        /// <param name="proxyObject"></param>
        /// <param name="collider"></param>
        /// <param name="area"></param>
        /// <param name="affectedAgents"></param>
        /// <param name="buffer"></param>
        private static void CreateModifierVolume(GameObject proxyObject, Collider collider, int area, List<int> affectedAgents, Vector3 buffer)
        {
            // Create a child object and set it to where the collider is.
            GameObject childTemp = new GameObject($"{collider.name}-NavMeshModifierVolume");
            childTemp.layer = proxyObject.layer; // NOTE: layers are not inherited from parents.

            // Create a custom modifier volume
            NavMeshModifierVolume sourceVolume = childTemp.AddComponent<NavMeshModifierVolume>();
            sourceVolume.area = area;
            sourceVolume.m_AffectedAgents = new List<int>(affectedAgents);

            // Set the volume to the center and size of the collider
            if (collider is BoxCollider box)
            {
                // Child is currently unparented.
                // Therefore these establish its WORLD transform directly.
                childTemp.transform.SetPositionAndRotation(collider.transform.position, collider.transform.rotation);

                // Scale the buffer with the object's lossy scale
                Vector3 scale = collider.transform.lossyScale;
                Vector3 localBuffer = new Vector3(
                    Mathf.Abs(scale.x) > Mathf.Epsilon
                        ? buffer.x / Mathf.Abs(scale.x)
                        : 0f,

                    Mathf.Abs(scale.y) > Mathf.Epsilon
                        ? buffer.y / Mathf.Abs(scale.y)
                        : 0f,

                    Mathf.Abs(scale.z) > Mathf.Epsilon
                        ? buffer.z / Mathf.Abs(scale.z)
                        : 0f
                );

                // Make sure we are scaled correctly
                childTemp.transform.localScale = scale;

                // Preserve the world transform while moving it beneath the proxy.
                childTemp.transform.SetParent(proxyObject.transform, worldPositionStays: true);

                // Make us have the same center and size as the collider
                sourceVolume.center = box.center;
                sourceVolume.size = box.size + localBuffer;
            }
            else
            {
                // All other colliders are in the world space and should be reflected as such
                Bounds bounds = collider.bounds;
                childTemp.transform.SetPositionAndRotation(bounds.center, Quaternion.identity);

                // Default scale
                childTemp.transform.localScale = Vector3.one;

                // Preserve the world transform while moving it beneath the proxy.
                childTemp.transform.SetParent(proxyObject.transform, worldPositionStays: true);

                // Make us have the same center and size as the collider
                sourceVolume.center = Vector3.zero;
                sourceVolume.size = bounds.size + buffer;
            }

            Plugin.LogInfo(
                $"Collider lossyScale: {collider.transform.lossyScale} | " +
                $"Proxy lossyScale: {proxyObject.transform.lossyScale} | " +
                $"Volume localScale: {childTemp.transform.localScale} | " +
                $"Volume lossyScale: {childTemp.transform.lossyScale}"
            );
        }

        /// <summary>
        /// Initalizes this custom NavMesh Modifier
        /// </summary>
        /// <returns><see langword="true"/> if we were or was successfully initialized; otherwise <see langword="false"/></returns>
        public bool InitializeCustomModifier()
        {
            // Don't do this again
            if (initialized) return true;

            // Make sure our Modifier volume is valid
            navMeshModifiers = rootModifierObject.GetComponentsInChildren<NavMeshModifier>();
            navMeshModifierVolumes = rootModifierObject.GetComponentsInChildren<NavMeshModifierVolume>();
            if (navMeshModifierVolumes.Length == 0 
                && navMeshModifiers.Length == 0)
            {
                Plugin.LogError($"[CustomNavMeshModifier] No NavMeshModifier(s) and no NavMeshModifierVolume(s) were found!");
                return false;
            }

            // Find the object to be parented to.
            Transform? parentTransform = null;
            switch (modifierLocation)
            {
                case ModifierParent.Interior:
                    Dungeon dungeon = RoundManager.Instance.dungeonGenerator.Generator.CurrentDungeon;
                    if (dungeon != null)
                    {
                        parentTransform = dungeon.gameObject.transform;
                    }
                    break;
                case ModifierParent.MoonEnvironment:
                    GameObject outsideNavMesh = GameObject.FindGameObjectWithTag("OutsideLevelNavMesh");
                    if (outsideNavMesh == null)
                    {
                        outsideNavMesh = GameObject.Find("CompanyBuildingNavMesh"); // NavMeshInCompanyRedux support!
                    }
                    parentTransform = outsideNavMesh.transform;
                    break;
                case ModifierParent.Custom:
                    parentTransform = parentObject != null ? parentObject.transform : null;
                    break;
                default:
                    break;
            }

            // Make sure we actually found it
            if (parentTransform == null)
            {
                Plugin.LogError($"[CustomNavMeshModifier] Failed to find transform to parent to. ModiferParent Value: {modifierLocation}");
                return false;
            }

            // Update our proxy object
            rootModifierObject.transform.SetParent(parentTransform, worldPositionStays: true);
            rootModifierObject.layer = LayerMask.NameToLayer("NavigationSurface");

            // Log what we did            
            Plugin.LogInfo($"[CustomNavMeshModifier] Added NavMeshModifierVolume to {parentTransform.gameObject} with {navMeshModifierVolumes.Length} modifier(s).");

            // Mark ourself as initialized
            initialized = true;

            // Mark our last position and last rotation
            lastPosition = transform.position;
            lastRotation = transform.rotation;

            // Update the NavMesh we are attached to.
            if (autoRebuildNavMesh)
            {
                navMeshUpdater.UpdateNavMesh();
            }

            return true;
        }

        private void Update()
        {
            // Don't do this until we are initialized
            if (!initialized)
            {
                return;
            }

            // Check if we need to update
            if (autoRebuildNavMeshOnMovement)
            {
                bool hasMoved = HasTransformChanged();
                if (hasMoved || shouldUpdate)
                {
                    if (hasMoved)
                    {
                        // Reset our stationary timer
                        timeStationary = 0.0f;
                        shouldUpdate = true; // Flag for update
                    }

                    // Update the proxy object's current position and rotation
                    rootModifierObject.transform.position = transform.position;
                    rootModifierObject.transform.rotation = transform.rotation;

                    if (!autoRebuildWhenStationary 
                        || timeStationary > autoRebuildTimeToStationary)
                    {
                        // Update the NavMesh
                        shouldUpdate = false;
                        navMeshUpdater.UpdateNavMesh();
                    }

                    // Update our last position and last rotation
                    lastPosition = transform.position;
                    lastRotation = transform.rotation;
                }

                // Only increment if we were not moving
                if (!hasMoved)
                {
                    timeStationary += Time.deltaTime;
                }
            }
        }

        private bool HasTransformChanged()
        {
            // No need to do this if our proxy object doesn't exist
            if (rootModifierObject == null)
            {
                return false;
            }

            // Check if we moved past the threshold
            if ((lastPosition - transform.position).sqrMagnitude > autoRebuildMoveThreshold * autoRebuildMoveThreshold)
            {
                return true;
            }
            if (lastRotation != transform.rotation)
            {
                return true;
            }
            return false;
        }

        private void OnDestroy()
        {
            // Update our flag
            isDestroying = true;

            // Destroy our proxy game object
            // We have to do this since we reparented rootModifierObject
            // and we need to guarantee the root object is destroyed as well.
            if (rootModifierObject != null)
            {
                // Disable our proxy NavMeshModifierVolume(s)
                if (navMeshModifierVolumes != null)
                {
                    // Loop through them all
                    for (int i = 0; i < navMeshModifierVolumes.Length; i++)
                    {
                        // Sanity check
                        var modifier = navMeshModifierVolumes[i];
                        if (modifier != null)
                        {
                            modifier.enabled = false; // Disable it so it doesn't affect the NavMeshRegen
                        }
                    }
                }

                // Disable our proxy NavMeshModifier(s)
                if (navMeshModifiers != null)
                {
                    // Loop through them all
                    for (int i = 0; i < navMeshModifiers.Length; i++)
                    {
                        // Sanity check
                        var modifier = navMeshModifiers[i];
                        if (modifier != null)
                        {
                            modifier.enabled = false; // Disable it so it doesn't affect the NavMeshRegen
                        }
                    }
                }

                // Rebake the NavMesh
                navMeshUpdater.UpdateNavMesh();

                // Destroy ourself!
                UnityEngine.Object.Destroy(rootModifierObject);
            }
        }
    }
}
