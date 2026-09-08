using NavMeshLib.Patches;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine.AI;

namespace NavMeshLib
{
    /// <summary>
    /// Helper manager that allows the registration of custom agent types
    /// </summary>
    public static class CustomAgentManager
    {
        /// <summary>
        /// The default agent ID as defined by Unity
        /// </summary>
        public const int DEFAULT_AGENT_ID = 0;

        /// <summary>
        /// The default agent name as defined by Unity
        /// </summary>
        public const string DEFAULT_AGENT_NAME = "Humanoid"; // NavMesh.GetSettingsNameFromID(DEFAULT_AGENT_ID)

        /// <summary>
        /// The invaild agent ID as defined by Unity
        /// </summary>
        public const int INVALID_AGENT_ID = -1;

        /// <summary>
        /// A public readonly collection of registered agent names
        /// </summary>
        /// <remarks>
        /// WARNING: This doesn't include the default agent name!
        /// </remarks>
        public static IReadOnlyCollection<string> RegisteredAgentNames => agentIDsByName.Keys;

        private static readonly Dictionary<int, NavMeshBuildSettings> registeredAgents = new Dictionary<int, NavMeshBuildSettings>();
        private static readonly Dictionary<int, string> agentNamesByID = new Dictionary<int, string>();
        private static readonly Dictionary<string, int> agentIDsByName = new Dictionary<string, int>();

        /// <summary>
        /// Registers a new <see cref="NavMeshAgent.agentTypeID"/> for use in the game.
        /// </summary>
        /// <remarks>
        /// WARNING: Make sure to cache the returned ID somewhere. This is how you can apply your custom agent type.<br/>
        /// WARNING: This should only be done in your <c>BaseUnityPlugin.Awake</c> as custom agent IDs will persist until the game is closed.
        /// </remarks>
        /// <param name="agentName">The name for this custom agent type</param>
        /// <param name="agentSettings">The settings for the agent</param>
        /// <returns>The <see cref="NavMeshAgent.agentTypeID"/> created by Unity</returns>
        public static int RegisterCustomAgent(string agentName, in NavMeshBuildSettings agentSettings)
        {
            // Sanity Check
            if (agentName == DEFAULT_AGENT_NAME)
            {
                throw new ArgumentException($"Cannot register custom NavMesh agent '{agentName}': that name is reserved for the default agent.", nameof(agentName));
            }

            // Don't register twice
            if (agentIDsByName.ContainsKey(agentName))
            {
                throw new ArgumentException($"A custom NavMesh agent named '{agentName}' is already registered.", nameof(agentName));
            }

            // Offically register our NavMesh agent with Unity
            var newAgent = NavMesh.CreateSettings();
            int agentIndex = newAgent.agentTypeID;

            // Replace the new agent settings with the one we were given
            newAgent.agentSlope = agentSettings.agentSlope;
            newAgent.agentClimb = agentSettings.agentClimb;
            newAgent.agentHeight = agentSettings.agentHeight;
            newAgent.agentRadius = agentSettings.agentRadius;
            newAgent.minRegionArea = agentSettings.minRegionArea;
            newAgent.overrideTileSize = agentSettings.overrideTileSize;
            newAgent.tileSize = agentSettings.tileSize;
            newAgent.overrideVoxelSize = agentSettings.overrideVoxelSize;
            newAgent.voxelSize = agentSettings.voxelSize;

            // Add to registered agent list
            registeredAgents.Add(agentIndex, newAgent);
            agentNamesByID.Add(agentIndex, agentName);
            agentIDsByName.Add(agentName, agentIndex);

            return agentIndex; // Give the user their new agent index
        }

        /// <summary>
        /// Updates the <see cref="NavMeshBuildSettings"/> with <paramref name="newSettings"/>
        /// </summary>
        /// <param name="agentTypeID">The agent ID to update.</param>
        /// <param name="newSettings">The new settings</param>
        public static void UpdateCustomAgentSettings(int agentTypeID, NavMeshBuildSettings newSettings)
        {
            // NEEDTOVALIDATE: Do we need this?
            //if (NavMesh.GetSettingsByID(agentTypeID).agentTypeID == INVALID_AGENT_ID)
            //{
            //    return;
            //}

            // Only allow registered agents
            if (!registeredAgents.ContainsKey(agentTypeID))
            {
                return;
            }

            // Sanity check, make sure this is set for the correct agent type
            newSettings.agentTypeID = agentTypeID;
            registeredAgents[agentTypeID] = newSettings;
        }

        /// <summary>
        /// Properly deregisteres and removes the custom agent type
        /// </summary>
        /// <remarks>
        /// This also deletes the custom agent from Unity so the <paramref name="agentTypeID"/> will be invalid. <br/>
        /// NOTE: You do not need to call this when the game closes, as Unity already handles this
        /// </remarks>
        /// <param name="agentTypeID"></param>
        public static void RemoveCustomAgent(int agentTypeID)
        {
            // Make sure this is a valid agent ID
            if (!registeredAgents.ContainsKey(agentTypeID) 
                && !agentNamesByID.ContainsKey(agentTypeID))
            {
                return;
            }

            // Remove our cache
            registeredAgents.Remove(agentTypeID);
            if (agentNamesByID.TryGetValue(agentTypeID, out var agentName))
            {
                agentIDsByName.Remove(agentName);
            }
            agentNamesByID.Remove(agentTypeID);

            // Delete the agent through Unity
            NavMesh.RemoveSettings(agentTypeID);
        }

        /// <summary>
        /// Gets an agent ID from the given settings name
        /// </summary>
        /// <param name="agentName">The name of the agent type</param>
        /// <returns>The found agent ID or <see cref="INVALID_AGENT_ID"/> on failure</returns>
        public static int GetAgentIDFromSettingsName(string agentName)
        {
            // Handle default
            if (agentName == DEFAULT_AGENT_NAME)
            {
                return DEFAULT_AGENT_ID;
            }

            // Make sure this is a vaild agent
            if (!agentIDsByName.TryGetValue(agentName, out var agentTypeID))
            {
                return INVALID_AGENT_ID;
            }

            return agentTypeID;
        }

        /// <summary>
        /// Backend hack to override the <paramref name="buildSettings"/> so the base game uses the one the User defines.
        /// </summary>
        /// <remarks>
        /// For some reason, Unity never exposed the method to update <see cref="NavMeshBuildSettings"/>, but thanks to Harmony,
        /// we can just manually override the settings right as Unity returns them. <br/> 
        /// Check out <see cref="NavMeshPatch"/> to see how this is done. <br/>
        /// For Modders: Use <see cref="NavMesh.GetSettingsByID(int)"/> or <seealso cref="NavMesh.GetSettingsByIndex(int)"/>, as NavMeshLib overrides them internally
        /// </remarks>
        /// <param name="buildSettings"></param>
        internal static void OverrideNavMeshSettings(ref NavMeshBuildSettings buildSettings)
        {
            // Replace the new agent settings with the one we were given
            int agentTypeID = buildSettings.agentTypeID;
            if (!registeredAgents.ContainsKey(agentTypeID))
            {
                return;
            }
            Plugin.LogDebug($"Overriding NavMeshBuildSettings for custom agent with ID: {agentTypeID}");

            // Go through and replace everything
            var agentSettings = registeredAgents[agentTypeID];
            buildSettings.agentSlope = agentSettings.agentSlope;
            buildSettings.agentClimb = agentSettings.agentClimb;
            buildSettings.agentHeight = agentSettings.agentHeight;
            buildSettings.agentRadius = agentSettings.agentRadius;
            buildSettings.minRegionArea = agentSettings.minRegionArea;
            buildSettings.overrideTileSize = agentSettings.overrideTileSize;
            buildSettings.tileSize = agentSettings.tileSize;
            buildSettings.overrideVoxelSize = agentSettings.overrideVoxelSize;
            buildSettings.voxelSize = agentSettings.voxelSize;
        }

        /// <summary>
        /// Backend hack to get the custom agent name so the base game uses the one the User defines.
        /// </summary>
        /// <remarks>
        /// For some reason, Unity never exposed a way to change the internal name, but thanks to Harmony,
        /// we can just manually override the name right as Unity returns them. <br/> 
        /// Check out <see cref="NavMeshPatch"/> to see how this is done. <br/>
        /// For Modders: Use <see cref="NavMesh.GetSettingsNameFromID(int)"/>, as NavMeshLib overrides it internally
        /// </remarks>
        /// <param name="agentTypeID"></param>
        /// <param name="agentName"></param>
        internal static void GetSettingsNameFromID(int agentTypeID, ref string agentName)
        {
            // Make sure this is a vaild agent
            if (!agentNamesByID.TryGetValue(agentTypeID, out var registeredName))
            {
                return; // Default agent ID or invaild
            }

            // Update the given refrence with the found name
            agentName = registeredName;
        }
    }
}
