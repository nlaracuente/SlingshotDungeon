using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Informs the parent checkpoint if when this trigger was hit
/// </summary>
[RequireComponent(typeof(Collider))]
public class CheckpointTriggers : MonoBehaviour
{
    /// <summary>
    /// A reference to the collider component
    /// </summary>
    Collider m_colider;

    /// <summary>
    /// A reference to the parent checkpoint
    /// </summary>
    Checkpoint m_checkpoint;

    /// <summary>
    /// A collections of all the objects that are triggering this 
    /// </summary>
    List<Collider> m_others = new List<Collider>();

	/// <summary>
    /// Initialization
    /// </summary>
	void Start ()
    {
        m_colider = GetComponent<Collider>();
        m_checkpoint = GetComponentInParent<Checkpoint>();
	}

    /// <summary>
    /// Notifies the parent checkpoint of any new collision triggers
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        if(!m_others.Contains(other))
        {
            m_others.Add(other);
            m_checkpoint.OnTriggerCollision(m_colider, other);
        }
    }

    /// <summary>
    /// Notifies the parent checkpoint of any new collision that left
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        if (m_others.Contains(other))
        {
            m_others.Remove(other);
            m_checkpoint.OnTriggerLeave(m_colider, other);
        }
    }
}
