using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checkpoints are locked behind doors that separats sections of the level and 
/// require keys to open. When the player is close enough to the door with a key
/// the door opens closing when they leave. When the player touches the checkpoint
/// trigger, the key is consumed, the door closed, and their spawn point updated
/// </summary>
public class Checkpoint : MonoBehaviour
{
    /// <summary>
    /// Where the player will re-spawn after dying/reaching this checkpoint
    /// </summary>
    [SerializeField]
    GameObject m_spawnPointGO;

    /// <summary>
    /// A reference to the Animator controller
    /// </summary>
    [SerializeField]
    Animator m_animator;

    /// <summary>
    /// Total keys required to open this door
    /// </summary>
    [SerializeField]
    int m_requiredKeys = 1;

    /// <summary>
    /// True once the chekpoint has been activated, meaning it should any new request to opened the door
    /// </summary>
    bool m_activated = false;

    /// <summary>
    /// Opens/Closes the door
    /// </summary>
    bool OpenDoor
    {
        set {
            m_animator.SetBool("Opened", value);
        }
    }

    /// <summary>
    /// Returns true while the player has enoughs keys to open the door
    /// </summary>
    bool CanOpenDoor
    {
        get {
            return GameManager.instance.PlayerScript.TotalKeys >= m_requiredKeys;
        }
    }

    /// <summary>
    /// Handles what happens when an objects enters/stays in a trigger
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="other"></param>
    public void OnTriggerCollision(Collider trigger, Collider other)
    {
        // Ignore anything that's not the player
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // As long as the player has keys 
        if(trigger.name == "OpenDoorTrigger" && !m_activated)
        {
            OpenDoor = CanOpenDoor;
        }

        // Update the players spawn point and disable the trigger
        if(trigger.name == "CheckpointTrigger")
        {
            trigger.enabled = false;
            m_activated = true;
            GameManager.instance.PlayerSpawnPoint = m_spawnPointGO.transform.InverseTransformVector(m_spawnPointGO.transform.position);
            GameManager.instance.PlayerScript.ConsumeTotalsKey(m_requiredKeys);
        }
    }

    /// <summary>
    /// Handles what happens when an objects leaves a trigger
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="other"></param>
    public void OnTriggerLeave(Collider trigger, Collider other)
    {
        // Ignore anything that's not the player
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (trigger.name == "OpenDoorTrigger")
        {
            OpenDoor = false;
        }
    }
}
