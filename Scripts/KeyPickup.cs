using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles animating collecting a key using a parabola effect
/// </summary>
public class KeyPickup : MonoBehaviour
{
    /// <summary>
    /// A reference to the parent Key component since the key is on a layer
    /// that cannot be collected we have a child object on a different layer that
    /// can collide and communicates this to the parent key
    /// </summary>
    [SerializeField]
    Key m_parentKey;

    /// <summary>
    /// How high to arc
    /// </summary>
    [SerializeField]
    float m_arcHeight = 5f;

    /// <summary>
    /// How quickly to move to the destination
    /// </summary>
    [SerializeField]
    float m_speed = 1f;

    /// <summary>
    /// A reference to the parent transform to move it
    /// since keys are on a layer that cannot collide with the player
    /// we have to add this script as a child object so that it can collide with it
    /// </summary>
    Transform m_parent;

    /// <summary>
    /// Tracks the current animation time to create a parabola effect
    /// </summary>
    float m_animationTime = 0f;

    /// <summary>
    /// True once collected
    /// </summary>
    public bool IsPickedUp { get; set; }

    /// <summary>
    /// How close to target before snapping to it
    /// </summary>
    [SerializeField]
    float m_destinationProximity = 1f;

    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        m_parent = transform.parent;
        m_parentKey = GetComponentInParent<Key>();
    }

    /// <summary>
    /// Triggers being collected if not already collected
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !GameManager.instance.PlayerScript.IsDead && !IsPickedUp)
        {
            IsPickedUp = true;
            m_parentKey.IsCollected = true;
        }
    }

    /// <summary>
    /// Handles moving the key from is current position to the given destination
    /// using a parabola arc effect
    /// </summary>
    /// <param name="current"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public IEnumerator MoveToDestinationRoutine(Vector3 current, Transform destination)
    {
        while(Vector3.Distance(m_parent.position, destination.position) > m_destinationProximity && !GameManager.instance.PlayerScript.IsDead)
        {
            m_animationTime += Time.deltaTime;
            m_animationTime = m_animationTime % m_arcHeight;
            float time = m_animationTime / m_speed;

            Vector3 position = ParabolaEffect.Parabola(current, destination.position, m_arcHeight, time);
            m_parent.position = position;
            yield return new WaitForEndOfFrame();
        }

        m_animationTime = 0;
        m_parent.position = destination.position;
    }

    /// <summary>
    /// Overloads to allow a vector3 as the destination
    /// </summary>
    /// <param name="current"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public IEnumerator MoveToDestinationRoutine(Vector3 current, Vector3 destination)
    {
        while (Vector3.Distance(m_parent.position, destination) > m_destinationProximity)
        {
            m_animationTime += Time.deltaTime;
            m_animationTime = m_animationTime % m_arcHeight;
            float time = m_animationTime / m_speed;

            Vector3 position = ParabolaEffect.Parabola(current, destination, m_arcHeight, time);
            m_parent.position = position;
            yield return new WaitForEndOfFrame();
        }

        m_animationTime = 0;
        m_parent.position = destination;
    }
}
