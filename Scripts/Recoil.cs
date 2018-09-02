using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Recoil : MonoBehaviour
{
    /// <summary>
    /// A reference to the rigidbody component
    /// </summary>
    [SerializeField]
    Rigidbody m_rigidbody;

    /// <summary>
    /// The layer where the ground is
    /// </summary>
    [SerializeField]
    LayerMask m_groundLayer;

    /// <summary>
    /// How fast the object dalls
    /// </summary>
    [SerializeField]
    float m_fallSpeed = 7f;

    /// <summary>
    /// How far to shoot the ray when testing collision
    /// with the ground based on its collider
    /// </summary>
    float m_distanceToGround = 0f;

    /// <summary>
    /// How many units delta must there be to consider the object falling
    /// </summary>
    [SerializeField]
    float m_minDelta = .25f;

    /// <summary>
    /// Keeps track of the y axis to know when the object is falling
    /// </summary>
    float m_previousY = 0f;

    /// <summary>
    /// Delegate for handling what happens when the object lands
    /// </summary>
    public delegate void LandAction();

    /// <summary>
    /// Event triggered when the objet lands after falling
    /// </summary>
    public event LandAction OnLand;

    /// <summary>
    /// Checks if the object is colliding with the ground
    /// </summary>
    public bool IsGrounded
    {
        get {
            float distance = m_distanceToGround + 0.1f;
            Debug.DrawRay(m_rigidbody.position, Vector3.down * distance, Color.red);
            return Physics.Raycast(m_rigidbody.position, Vector3.down, distance, m_groundLayer);
        }
    }

    /// <summary>
    /// True when object is falling
    /// </summary>
    public bool IsFalling { get; set; }

    /// <summary>
    /// True when object is falling
    /// </summary>
    public bool IsJumping { get; set; }

    /// <summary>
    /// Flag to prevent running the couroutine too often
    /// </summary>
    bool m_isCheckingForLanding = false;

    /// <summary>
    /// Initialization
    /// </summary>
    void Start()
    {
        if (m_rigidbody == null)
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }

        m_distanceToGround = GetComponent<Collider>().bounds.extents.y;
        m_previousY = transform.position.y;
    }

    /// <summary>
    /// Handles gravity
    /// </summary>
    void FixedUpdate()
    {
        // Only if the rigidbody has gravity enabled
        if (m_rigidbody.useGravity)
        {
            if (IsFalling && !m_isCheckingForLanding)
            {
                StartCoroutine(WaitUntilLandingRoutine());
            }
            AddFallingGravity();
        }
    }

    /// <summary>
    /// Updates the IsFalling variable based on current y position and the minum delta requirements
    /// </summary>
    void LateUpdate()
    {
        float currentY = transform.position.y;
        float deltaY = Mathf.Abs(currentY - m_previousY);

        IsFalling = (currentY < m_previousY && deltaY >= m_minDelta);
        IsJumping = (currentY > m_previousY && deltaY >= m_minDelta);
        m_previousY = currentY;
    }

    /// <summary>
    /// Triggered when the object is falling 
    /// Dispatches OnLand once the object is touching the ground
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitUntilLandingRoutine()
    {
        m_isCheckingForLanding = true;

        while (IsFalling)
        {
            yield return new WaitForFixedUpdate();
        }

        // Landed 
        if (OnLand != null)
        {
            OnLand();
        }

        m_isCheckingForLanding = false;
    }

    /// <summary>
    /// Increases the falling velocity to achieve a more "video game" like falling
    /// </summary>
    void AddFallingGravity()
    {
        if (IsGrounded)
        {
            return;
        }

        Vector3 fallForce = Vector3.down * m_fallSpeed;
        AddForce(fallForce);
    }

    /// <summary>
    /// Adds the given force to the rigidbody
    /// </summary>
    /// <param name="force"></param>
    public void AddForce(Vector3 force, ForceMode mode = ForceMode.Impulse)
    {
        m_rigidbody.AddForce(force, mode);
    }
}
