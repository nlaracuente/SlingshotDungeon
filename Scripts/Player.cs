using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Controls player actions
/// </summary>
public class Player : MonoBehaviour, IRecoilable
{
    /// <summary>
    /// A reference to the rigidbody component
    /// </summary>
    [SerializeField]
    Rigidbody m_rigidbody;

    /// <summary>
    /// A reference to the animator component
    /// </summary>
    [SerializeField]
    Animator m_animator;
    Animator AnimController
    {
        get {
            if(m_animator == null)
            {
                m_animator = GetComponentInChildren<Animator>();
            }
            return m_animator;
        }
    }

    /// <summary>
    /// A reference to the camera to use when translating player movement
    /// </summary>
    [SerializeField]
    Camera m_camera;

    /// <summary>
    /// How fast the rigidbody moves left/right
    /// </summary>
    [SerializeField]
    float m_moveSpeed = 40f;

    /// <summary>
    /// How quickly the rigidbody turns left/right
    /// </summary>
    [SerializeField]
    float m_rotationSpeed = 25f;

    /// <summary>
    /// How close to the target rotation before snapping to it
    /// </summary>
    [SerializeField]
    float m_rotationDistance = 0.01f;

    /// <summary>
    /// Holds the current rotation the player needs to face
    /// </summary>
    Quaternion m_targetRotation = Quaternion.identity;

    /// <summary>
    /// A reference to the recoil component
    /// </summary>
    Recoil m_recoil;

    /// <summary>
    /// Holds the players movement input vector
    /// </summary>
    Vector3 m_input = Vector3.zero;

    /// <summary>
    /// Collections of keys the player has picked up
    /// </summary>
    List<Key> m_keys = new List<Key>();

    /// <summary>
    /// True while the player is jumping
    /// </summary>
    bool m_isJumping = false;
    public bool IsJumping
    {
        get { return m_isJumping; }
        set {
            m_isJumping = value;
            AnimController.SetBool("Jumping", value);
        }
    }

    /// <summary>
    /// Checks if the player has collided with the ground
    /// </summary>
    public bool IsGrounded { get { return m_recoil.IsGrounded; } }

    /// <summary>
    /// True when object is falling
    /// </summary>
    public bool IsFalling { get { return m_recoil.IsFalling; } }

    /// <summary>
    /// True: ignores player's input
    /// </summary>
    public bool ControlsEnabled { get; set; }

    /// <summary>
    /// True when the player has died
    /// </summary>
    bool m_isDead = false;
    public bool IsDead
    {
        get { return m_isDead; }
        set {
            m_isDead = value;
            ControlsEnabled = !value; // opposite as "true death" == "no controls"
            AnimController.SetBool("Dead", value);

            // Freeze the rigidbody
            if (value)
            {
                m_rigidbody.velocity = Vector3.zero;
                m_rigidbody.angularVelocity = Vector3.zero;
                m_rigidbody.useGravity = false;
                AudioManager.instance.PlaySound(AudioName.Die);
            } else {
                m_rigidbody.useGravity = true;
            }
        }
    }

    /// <summary>
    /// True while the player has at least one key
    /// </summary>
    public bool HasKeys { get { return m_keys.Count > 0; } }

    /// <summary>
    /// Returns the total keys current held by the player
    /// </summary>
    public int TotalKeys { get { return m_keys.Count; } }

    /// <summary>
    /// Keeps track of the clip used for walking
    /// </summary>
    SoundClip m_walkingClip;

    /// <summary>
    /// Initialization
    /// </summary>
    void Start()
    {
        if (m_rigidbody == null)
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }

        if (m_camera == null)
        {
            m_camera = Camera.main;
        }

        m_recoil = GetComponent<Recoil>();
    }	
	
    /// <summary>
    /// Handles player input
    /// </summary>
	void Update ()
    {
        m_input = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            0f
        );

        AnimController.SetBool("Falling", !IsGrounded && IsFalling);

        // No longer jumping
        if(IsFalling)
        {
            IsJumping = false;
        }
	}

    /// <summary>
    /// Handles rotation/movement
    /// </summary>
    void FixedUpdate()
    {
        if (!ControlsEnabled)
        {
            return;
        }

        Rotate();
        Move();
    }

    /// <summary>
    /// Handles moving the player based on player input
    /// </summary>
    void Move()
    {
        if (m_input == Vector3.zero)
        {
            AnimController.SetFloat("Speed", 0f);
            if (m_walkingClip != null)
            {
                m_walkingClip.Stop();
            }
            return;

        } else {
            AnimController.SetFloat("Speed", 1f);

            if (!IsFalling && IsGrounded)
            {
                bool isAlreadyPlaying = m_walkingClip != null && m_walkingClip.IsPlaying;
                if (!isAlreadyPlaying)
                {
                    m_walkingClip = AudioManager.instance.PlaySound(AudioName.Walk);
                }
                
            } else
            {
                if (m_walkingClip != null)
                {
                    m_walkingClip.Stop();
                }
            }
        }

        // m_rigidbody.AddForce(m_input * m_moveSpeed, ForceMode.VelocityChange);
        Vector3 targetPosition = m_rigidbody.position + m_input * m_moveSpeed * Time.deltaTime;
        m_rigidbody.MovePosition(targetPosition);
    }
    
    /// <summary>
    /// Handles rotating the player
    /// </summary>
    void Rotate()
    {
        if (m_input == Vector3.zero)
        {         
            return;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(m_input, Vector3.up);

        if (m_targetRotation != desiredRotation)
        {
            // Always stop to be safe
            StopCoroutine("RotateRoutine");

            // Snap to the previous desired rotation
            // to avoid any weird half rotations
            if (m_targetRotation != Quaternion.identity)
            {
                m_rigidbody.rotation = m_targetRotation;
            }

            m_targetRotation = desiredRotation;
            StartCoroutine(RotateRoutine());
        }
    }

    /// <summary>
    /// Smoothly rotates the player to face the target rotation
    /// </summary>
    /// <returns></returns>
    IEnumerator RotateRoutine()
    {
        while (Quaternion.Angle(m_rigidbody.rotation, m_targetRotation) > m_rotationDistance)
        {
            Quaternion newRotation = Quaternion.Lerp(
                m_rigidbody.rotation,
                m_targetRotation,
                m_rotationSpeed * Time.fixedDeltaTime
            );

            m_rigidbody.rotation = newRotation;
            yield return new WaitForFixedUpdate();
        }

        m_rigidbody.rotation = m_targetRotation;
    }

    /// <summary>
    /// Adds the given force to the rigidbody
    /// </summary>
    /// <param name="recoilForce"></param>
    public void AddRecoilForce(Vector3 recoilForce, ForceMode mode = ForceMode.Impulse)
    {
        m_recoil.AddForce(recoilForce, mode);
    }

    /// <summary>
    /// As long as the player is grounded then they can be recoiled
    /// </summary>
    /// <returns></returns>
    public bool CanRecoil()
    {
        return ControlsEnabled && IsGrounded;
    }

    /// <summary>
    /// Triggered after the player has re-spawned to handles anything needed
    /// </summary>
    public void Respanwed()
    {
        // Cause any and all keys the player is holding to respawn
        m_keys.ForEach(x => x.TriggerRespawn());
        m_keys.Clear();

        GameManager.instance.UpdateKeyIconStatus();
        AudioManager.instance.PlaySound(AudioName.Respawn);
    }

    /// <summary>
    /// Triggered when the player collects the key
    /// </summary>
    /// <param name="key"></param>
    public void KeyCollected(Key key)
    {
        if(!m_keys.Contains(key))
        {
            m_keys.Add(key);
        }
    }

    /// <summary>
    /// Removes from the keys inventory the total given 
    /// Notifies the game manger to update keys icon
    /// </summary>
    /// <param name="total"></param>
    public void ConsumeTotalsKey(int total)
    {
        if(m_keys.Count >= total)
        {
            m_keys.RemoveRange(0, total);
            AudioManager.instance.PlaySound(AudioName.KeyUsed);
        }
        
        GameManager.instance.UpdateKeyIconStatus();
    }

    public void PlayVictory()
    {
        if (m_walkingClip != null)
        {
            m_walkingClip.Stop();
        }
        m_animator.SetTrigger("Victory");
    }
}