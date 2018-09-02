using UnityEngine;
using System.Collections;

/// <summary>
/// Keys are collectable items that the player needs to open doors to access the next section of the level
/// Each key is bound to a specific starting position but any keys can be used to open exactly ONE door
/// Collected Keys get added to the player's inventory and removed when the player dies before consuming it
/// </summary>
public class Key : MonoBehaviour
{
    /// <summary>
    /// A reference to the rigidbody
    /// </summary>
    Rigidbody m_rigidbody;

    /// <summary>
    /// A reference to the collider
    /// </summary>
    Collider m_collider;

    /// <summary>
    /// A reference to the renderer
    /// </summary>
    Renderer m_renderer;

    /// <summary>
    /// A reference to the KeyPickup component to animate the key being picked up
    /// </summary>
    KeyPickup m_pickup;    

    /// <summary>
    /// Where the key was before being collected
    /// </summary>
    [SerializeField]
    Vector3 m_origin;

    /// <summary>
    /// Returns the player's current position
    /// </summary>
    Vector3 PlayerPosition { get { return GameManager.instance.PlayerScript.transform.position; } }

    /// <summary>
    /// True while the routine is running
    /// </summary>
    bool m_isRoutineRunning = false;

    /// <summary>
    /// True when the key has been picked up
    /// </summary>
    bool m_isCollected = false;
    public bool IsCollected
    {
        get { return m_isCollected; }
        set {
            m_isCollected = value;
            if (value)
            {
                ActiveRigidbody = false;
                StartCoroutine( PickedUpRoutine(transform.position, GameManager.instance.PlayerScript.transform) );
                GameManager.instance.PlayerScript.KeyCollected(this);
            }

            // Notify the GameManager that a key was collected to update the icon display
            GameManager.instance.UpdateKeyIconStatus();
        }
    }

    /// <summary>
    /// Enables/Disables the rigidbody controls
    /// </summary>
    bool ActiveRigidbody
    {
        set {
            m_rigidbody.useGravity = value;
            if(!value)
            {
                m_rigidbody.velocity = Vector3.zero;
                m_rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// Enables/Disables the keys renderer and colliders
    /// </summary>
    bool EnableKey
    {
        set {
            m_renderer.enabled = value;
            foreach (Collider collider in GetComponents<Collider>())
            {
                collider.enabled = value;
            }
        }
    }

    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        m_origin = transform.position;
        m_pickup = GetComponentInChildren<KeyPickup>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_renderer = GetComponent<Renderer>();
        m_collider = GetComponent<Collider>();

        Recoil recoil = GetComponent<Recoil>();
        recoil.OnLand += OnKeyLand;
    }

    /// <summary>
    /// Triggers the sound of the key landing
    /// </summary>
    public void OnKeyLand()
    {
        if (RecoilCamera.IsVisible(m_collider))
        {
            AudioManager.instance.PlaySound(AudioName.KeyLands);
        }
    }
    
    /// <summary>
    /// Leaves the player's current position and goes back into its starting position
    /// Resets the key to not be collected and reactivates the rigidbody
    /// </summary>
    public void TriggerRespawn()
    {
        // Place it at the player's current position
        transform.position = PlayerPosition;

        // Display it
        EnableKey = true;

        StartCoroutine(RespawnRoutine());
    }

    /// <summary>
    /// Handles respawing the key and re-enabling it
    /// </summary>
    /// <returns></returns>
    IEnumerator RespawnRoutine()
    {
        // Move it
        // The TRUE here keeps it enabled
        yield return StartCoroutine(PickedUpRoutine(transform.position, m_origin, true));

        // Allow it to be re-collected
        IsCollected = false;
        m_pickup.IsPickedUp = false;
        ActiveRigidbody = true;
    }

    /// <summary>
    /// Handles animating the key leaving the player's current position
    /// and moving back to where it started and re-activating its rigidbody
    /// </summary>
    /// <returns></returns>
    IEnumerator PickedUpRoutine(Vector3 from, Transform destination, bool enableKey = false)
    {
        // Only trigger when the object has not been picked up
        // Note that the Key triggers this routine
        // therefore we use the KeyPickup component to determine if the routine was triggered
        if (!m_isRoutineRunning)
        {
            m_isRoutineRunning = true;
            AudioManager.instance.PlaySound(AudioName.KeyLands);
            yield return StartCoroutine(m_pickup.MoveToDestinationRoutine(from, destination));
            AudioManager.instance.PlaySound(AudioName.KeyPickup);
            EnableKey = enableKey;
            m_isRoutineRunning = false;
        }
    }

    /// <summary>
    /// Overloads the pickup routine to allow for a vector3 to be passed as the destination rather than a transform
    /// </summary>
    /// <param name="from"></param>
    /// <param name="destination"></param>
    /// <param name="enableKey"></param>
    /// <returns></returns>
    IEnumerator PickedUpRoutine(Vector3 from, Vector3 destination, bool enableKey = false)
    {
        // Only trigger when the object has not been picked up
        // Note that the Key triggers this routine
        // therefore we use the KeyPickup component to determine if the routine was triggered
        if (!m_isRoutineRunning)
        {
            m_isRoutineRunning = true;
            yield return StartCoroutine(m_pickup.MoveToDestinationRoutine(from, destination));
            AudioManager.instance.PlaySound(AudioName.KeyLands);
            EnableKey = enableKey;
            m_isRoutineRunning = false;
        }
    }
}
