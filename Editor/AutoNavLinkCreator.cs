using System;
using System.Collections.Generic;
using System.Text;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace NavMeshLib.Editor
{
    /// <summary>
    /// A helper mono behavior made to allow moon makers to automatically have <see cref="NavMeshLink"/>s created for
    /// custom agent types at runtime. Since custom agent IDs are registered at runtime, custom <see cref="NavMeshLink"/>s
    /// must be created for them at runtime. This allows moon makers and other modders to have the links auto created
    /// on object awake.
    /// </summary>
    /// <remarks>
    /// This object is not needed for Interior and OutsideNavMesh game object as NavMeshLib already handles those cases. <br/>
    /// WARNING: NavMeshLib's build-in NavMeshLink auto creation only happens on <see cref="SceneManager.sceneLoaded"/>. If your object can be created after that,
    /// you should include this in your object. <br/>
    /// NOTE: In some cases, you are better off using <see cref="CustomNavMeshAgentHelper"/> if you want more control over NavMeshLinks for certain custom agent IDs! <br/>
    /// </remarks>
    [RequireComponent(typeof(OffMeshLink))]
    public class AutoNavLinkCreator : MonoBehaviour
    {
        /// <summary>
        /// Represents the <see cref="OffMeshLink"/> to copy and mimic.
        /// </summary>
        [Tooltip("The OffMeshLink to copy and mimic.")]
        public OffMeshLink linkToCopy = null!;

        /// <summary>
        /// Indicates whether the existing <see cref="OffMeshLink"/> should be destroyed.
        /// </summary>
        public bool destroyOldOffMeshLink = false;

        /// <summary>
        /// The agents to create copy the OffMeshLink for. <br/>
        /// Leave blank for all.
        /// </summary>
        /// <remarks>
        /// WARNING: <see cref="destroyOldOffMeshLink"/> will be forced true if the default agent ID isn't included. <br/>
        /// This is a limitation on my side.
        /// </remarks>
        [Tooltip("The agents to create copy the OffMeshLink for. \n Leave blank for all. \n WARNING: destroyOldOffMeshLink will be forced true if the default agent ID isn't included. \n This is a limitation on my side.")]
        public string[] agentNames = { };

        private void Start()
        {
            // Make sure we have a valid link to copy
            if (linkToCopy == null)
            {
                Plugin.LogWarning($"[AutoNavLinkCreator] ({name}) linkToCopy was not set, attempting to find OffMeshLink component.");
                linkToCopy = GetComponent<OffMeshLink>();
                if (linkToCopy == null)
                {
                    Plugin.LogError($"[AutoNavLinkCreator] ({name}) failed to find OffMeshLink to copy! Skipping!");
                    return;
                }
            }

            // Log what we are doing
            Plugin.LogInfo($"[AutoNavLinkCreator] ({name}) starting copying the given OffMeshLink.");

            // So, did the user specify which agents links should be created for
            if (agentNames.Length <= 0)
            {
                // Call our helper to do our dirty work
                NavMeshUtil.CopyOffMeshLink(linkToCopy, destroyOldOffMeshLink);
            }
            // Skip the loop overhead and directly grab the agent settings ID
            else if (agentNames.Length == 1)
            {
                // Find the agent ID
                int agentTypeID = CustomAgentManager.GetAgentIDFromSettingsName(agentNames[0]);
                if (!destroyOldOffMeshLink && agentTypeID == CustomAgentManager.DEFAULT_AGENT_ID)
                {
                    Plugin.LogWarning($"[AutoNavLinkCreator] ({name}) Default agent name was given, but destroyOldOffMeshLink was false.....this is redundant as OffMeshLinks for for it by default. Was this a mistake?");
                }

                // Call our helper to do our dirty work
                NavMeshUtil.CopyOffMeshLink(linkToCopy, agentTypeID, destroyOldOffMeshLink);
            }
            // Loop and copy them all
            else
            {
                // Loop through the given agent names
                for (int i = 0; i < agentNames.Length; i++)
                {
                    // Find the agent ID
                    int agentTypeID = CustomAgentManager.GetAgentIDFromSettingsName(agentNames[i]);
                    if (!destroyOldOffMeshLink && agentTypeID == CustomAgentManager.DEFAULT_AGENT_ID)
                    {
                        Plugin.LogWarning($"[AutoNavLinkCreator] ({name}) Default agent name was given, but destroyOldOffMeshLink was false.....this is redundant as OffMeshLinks for for it by default. Was this a mistake?");
                    }

                    // Call our helper to do our dirty work
                    NavMeshUtil.CopyOffMeshLink(linkToCopy, agentTypeID, destroyOldOffMeshLink);
                }
            }
        }
    }
}
