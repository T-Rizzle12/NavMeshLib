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
    /// A helper mono behavior that allows moon makers to set custom agent IDs at runtime.
    /// </summary>
    /// <remarks>
    /// Since <see cref="NavMesh.CreateSettings"/> can return a different ids at runtime. 
    /// We use <see cref="CustomAgentManager.GetAgentIDFromSettingsName(string)"/> to resolve the custom agent ID at runtime.
    /// </remarks>
    public class CustomNavMeshAgentHelper : MonoBehaviour
    {
        [Tooltip("The custom agents' names to use")]
        public string[] agentNames = { CustomAgentManager.DEFAULT_AGENT_NAME };

        [Tooltip("Should this affect child objects as well?")]
        public bool includesChildren = false;

        [Tooltip("For NavMeshModifier and NavMeshModifierVolume only! Should this override the previously set agent types?")]
        public bool overridePreviousAgentTypes = false;

        [Tooltip("Should we affect attached NavMeshSurfaces?")]
        public bool updateNavMeshSurfaces = true;

        [Tooltip("Should we affect attached NavMeshModifiers?")]
        public bool updateNavMeshModifiers = true;

        [Tooltip("Should we affect attached NavMeshModifierVolumes?")]
        public bool updateNavMeshModifierVolumes = true;

        [Tooltip("Should we affect attached NavMeshLinks?")]
        public bool updateNavMeshLinks = true;

        private void Awake()
        {
            // Sanity check
            if (agentNames == null || agentNames.Length == 0)
            {
                Plugin.LogWarning($"No custom NavMesh agents were specified for {gameObject.name}.");
                return;
            }

            // Find the agent IDs associated
            List<int> agentIDs = new List<int>();
            for (int i = 0; i < agentNames.Length; i++)
            {
                int agentTypeID = CustomAgentManager.GetAgentIDFromSettingsName(agentNames[i]);
                if (agentTypeID == CustomAgentManager.INVALID_AGENT_ID)
                {
                    Plugin.LogWarning($"Unable to find custom NavMesh agent '{agentNames[i]}' for {gameObject.name}.");
                    continue;
                }
                agentIDs.Add(agentTypeID);
            }

            if (agentIDs.Count == 0)
            {
                Plugin.LogWarning($"Unable to find any custom NavMesh agent in '{nameof(agentNames)}' for {gameObject.name}.");
                return;
            }

            // Change all NavMesh Components to this agent
            UpdateIfSingleAgent(agentIDs);

            // These NavMesh objects accept multiple agent IDs
            NavMeshModifier[] modifiers = GetNavMeshComponents<NavMeshModifier>(updateNavMeshModifiers);
            NavMeshModifierVolume[] volumeModifiers = GetNavMeshComponents<NavMeshModifierVolume>(updateNavMeshModifierVolumes);

            // Standard NavMeshModifiers
            const int ALL_AGENTS_ID = -1; // Unity uses this to represnt the modifier affects all agents
            for (int i = 0; i < modifiers.Length; i++)
            {
                var modifiersSurface = modifiers[i];
                if (modifiersSurface != null)
                {
                    if (overridePreviousAgentTypes)
                    {
                        List<int> m_AffectedAgents = modifiersSurface.m_AffectedAgents;
                        m_AffectedAgents.Clear();
                        m_AffectedAgents.AddRange(agentIDs);
                    }
                    else if (modifiersSurface.m_AffectedAgents[0] != ALL_AGENTS_ID)
                    {
                        HashSet<int> oldAffectedAgents = modifiersSurface.m_AffectedAgents.ToHashSet();
                        oldAffectedAgents.UnionWith(agentIDs);
                        modifiersSurface.m_AffectedAgents = oldAffectedAgents.ToList();
                    }
                }
            }

            // NavMeshModiferVolumes
            for (int i = 0; i < volumeModifiers.Length; i++)
            {
                var volumeModifier = volumeModifiers[i];
                if (volumeModifier != null)
                {
                    if (overridePreviousAgentTypes)
                    {
                        List<int> m_AffectedAgents = volumeModifier.m_AffectedAgents;
                        m_AffectedAgents.Clear();
                        m_AffectedAgents.AddRange(agentIDs);
                    }
                    else if (volumeModifier.m_AffectedAgents[0] != ALL_AGENTS_ID)
                    {
                        HashSet<int> oldAffectedAgents = volumeModifier.m_AffectedAgents.ToHashSet();
                        oldAffectedAgents.UnionWith(agentIDs);
                        volumeModifier.m_AffectedAgents = oldAffectedAgents.ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Helper function for NavMesh stuff that only works with 1 agent ID
        /// </summary>
        /// <param name="agentIDs"></param>
        private void UpdateIfSingleAgent(List<int> agentIDs)
        {
            // Only do this if the size is 1
            if (agentIDs.Count != 1)
            {
                if (agentIDs.Count > 1 && (updateNavMeshSurfaces || updateNavMeshLinks))
                {
                    Plugin.LogWarning($"Multiple custom agents were specified for {gameObject.name}. \n NavMeshSurface and NavMeshLink components require a single agent type and will not be modified.");
                }
                return;
            }

            // Get the ID
            int agentTypeID = agentIDs[0];

            // Update the Surfaces
            NavMeshSurface[] navMeshSurfaces = GetNavMeshComponents<NavMeshSurface>(updateNavMeshSurfaces);
            NavMeshLink[] navMeshLinks = GetNavMeshComponents<NavMeshLink>(updateNavMeshLinks);
            for (int i = 0; i < navMeshSurfaces.Length; i++)
            {
                var navMeshSurface = navMeshSurfaces[i];
                if (navMeshSurface != null)
                {
                    navMeshSurface.agentTypeID = agentTypeID;
                }
            }

            // Update the links
            for (int i = 0; i < navMeshLinks.Length; i++)
            {
                var navLink = navMeshLinks[i];
                if (navLink != null)
                {
                    navLink.agentTypeID = agentTypeID;
                }
            }
        }

        /// <summary>
        /// Helper function for getting the correct components based on <see cref="includesChildren"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enabled"></param>
        /// <returns></returns>
        private T[] GetNavMeshComponents<T>(bool enabled)
            where T : Component
        {
            if (!enabled)
            {
                return Array.Empty<T>();
            }

            return includesChildren ? GetComponentsInChildren<T>() : GetComponents<T>();
        }
    }
}
