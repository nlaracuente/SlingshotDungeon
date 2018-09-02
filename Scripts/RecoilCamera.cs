using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera that handles tracking the player and the recoiling action
/// </summary>
public class RecoilCamera : MonoBehaviour
{
    /// <summary>
    /// The transform to track
    /// </summary>
    [SerializeField]
    Transform m_target;
    public Transform Target
    {
        get {
            if(m_target == null)
            {
                m_target = PlayerScript.transform;
            }
            return m_target;
        }
        set { m_target = value; }
    }

    /// <summary>
    /// How fast to track the target
    /// </summary>
    [SerializeField]
    float m_trackingSpeed = 5f;
    public float TrackingSpeed { get { return m_trackingSpeed; } set { m_trackingSpeed = value; } }

    /// <summary>
    /// The offset from the target
    /// </summary>
    [SerializeField]
    Vector3 m_trackingDistance = new Vector3(0f, 10f, 0f);

    /// <summary>
    /// The axis of the targt for the camera to track
    /// </summary>
    [SerializeField]
    Vector3 m_axisToTrack = Vector3.zero;

    /// <summary>
    /// How close to the target position before snapping in place
    /// </summary>
    [SerializeField]
    float m_destinationProximity = .01f;

    /// <summary>
    /// Multiplier for when recoiling by moving the mouse
    /// </summary>
    [SerializeField]
    float m_mouseSensitivity = 5f;

    /// <summary>
    /// Multiplier for when recoiling by pressing a button
    /// </summary>
    [SerializeField]
    float m_buttonSensitivity = 20f;

    /// <summary>
    /// How fast to recoil
    /// </summary>
    [SerializeField]
    float m_recoilingSpeed = 1f;

    /// <summary>
    /// The maximum distance the camera can be recoiled when doing a "spring action" basd on current position
    /// </summary>
    [SerializeField]
    float m_maxRecoilDistance = 50f;

    /// <summary>
    /// Minimum amount of distance to recoil before considering it can be used
    /// </summary>
    [SerializeField]
    float m_minRecoilDistance = 5f;

    /// <summary>
    /// How long it takes to spring back to the player
    /// </summary>
    [SerializeField]
    float m_springTime = 2f;

    /// <summary>
    /// The amount of force the camera will apply during the spring action
    /// </summary>
    [SerializeField]
    float m_springForce = 200f;

    /// <summary>
    /// True when the camera can track the target
    /// </summary>
    [SerializeField]
    bool m_trackTarget = false;
    public bool TrackTarget { get { return m_trackTarget; } set { m_trackTarget = value; } }

    /// <summary>
    /// Keeps track of where the camera was when the spring action was engaged
    /// </summary>
    [SerializeField]
    Vector3 m_startingPosition;

    /// <summary>
    /// Determines how much the camera can be moved
    /// </summary>
    [SerializeField]
    Vector3 m_maxPosition;

    /// <summary>
    /// True while the recoil routine is running
    /// </summary>
    bool m_recoilRoutineStarted = false;

    /// <summary>
    /// Holds a reference to the player
    /// </summary>
    Player m_player;
    Player PlayerScript
    {
        get {
            if(m_player == null)
            {
                m_player = FindObjectOfType<Player>();
            }
            return m_player;
        }
    }

    /// <summary>
    /// Returns true when the player is activating the request to recoil the camera
    /// </summary>
    bool IsEnganged
    {
        get {
            return Input.GetButton("Recoil");
        }
    }

    /// <summary>
    /// True when the recoil/spring action can be engaged
    /// </summary>
    bool CanEngage
    {
        get {
            bool canEngage = false;

            if (PlayerScript != null)
            {
                canEngage = PlayerScript.IsGrounded && GameManager.instance.IsGamePlay;
            }
            return canEngage;
        }
    }
    
    /// <summary>
    /// Triggers the dragging of the camera when the mouse is enganged
    /// </summary>
    void Update()
    {
        if(CanEngage && IsEnganged && !m_recoilRoutineStarted)
        {
            m_startingPosition = transform.position;
            m_maxPosition = new Vector3(m_startingPosition.x, m_startingPosition.y - m_maxRecoilDistance, m_startingPosition.z);
            StartCoroutine(RecoilRoutine());
        }
    }

    /// <summary>
    /// While the camera's recoil is engaged the camera will stop tracking its target
    /// and move in the direction being recoiled until it is released
    /// </summary>
    /// <returns></returns>
    IEnumerator RecoilRoutine()
    {
        TrackTarget = false;
        m_recoilRoutineStarted = true;
        bool cancelRecoil = false;

        SoundClip clip = AudioManager.instance.PlaySound(AudioName.PullLevel);
        while (IsEnganged)
        {
            cancelRecoil = !GameManager.instance.IsGamePlay || 
                            PlayerScript.IsDead ||
                            PlayerScript.IsFalling ||
                            !PlayerScript.IsGrounded;

            if(cancelRecoil)
            {
                break;
            }

            float speed = GetRecoilSpeed();

            Vector3 targetPosition = new Vector3(
                transform.position.x,
                Mathf.Clamp(transform.position.y - speed, m_maxPosition.y, m_startingPosition.y),
                transform.position.z
            );

            transform.position = Vector3.Lerp(transform.position, targetPosition, m_recoilingSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        // Make sure the sound is stopped
        clip.Stop();

        // Play animation
        Vector3 recoilPosition = transform.position;
        float difference = m_startingPosition.y - recoilPosition.y;

        // Meets minimum threshold
        // Also, we may have cancelled it
        if (!cancelRecoil && difference >= m_minRecoilDistance)
        {
            AudioManager.instance.PlaySound(AudioName.ReleaseLevel);
            yield return StartCoroutine(SpringRoutine(transform.position, m_startingPosition));

            // Find all recoilable objects on the screen to apply the recoil force           
            Vector3 force = Vector3.up * (m_springForce * difference);
            AddForceToRecoilables(force);

            // Apply to the player too
            GameManager.instance.PlayerScript.AddRecoilForce(force);
            GameManager.instance.PlayerScript.IsJumping = true;
        }        

        TrackTarget = true;

        // Wait for a bit to ensure the objects take-off before we allow this action to be re-enabled
        yield return new WaitForSeconds(.25f);

        m_recoilRoutineStarted = false;
    }

    /// <summary>
    /// Finds all recoilable objects currently in view and attempts to apply recoil force
    /// </summary>
    /// <param name="force"></param>
    void AddForceToRecoilables(Vector3 force)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        foreach (GameObject recoilObject in GameObject.FindGameObjectsWithTag("Recoilable"))
        {
            Collider collider = recoilObject.GetComponent<Collider>();

            if(GeometryUtility.TestPlanesAABB(planes, collider.bounds))
            {
                IRecoilable recoilable = collider.GetComponent<IRecoilable>();
                if (recoilable != null && recoilable.CanRecoil())
                {
                    recoilable.AddRecoilForce(force, ForceMode.Impulse);
                }
            }
        }
    }

    /// <summary>
    /// Returns true if the given collider is visible to the camera
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    public static bool IsVisible(Collider collider)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, collider.bounds);
    }

    /// <summary>
    /// Returns the speed to use when recoiling the camera base on either mouse position
    /// or alternative button being held
    /// </summary>
    /// <returns></returns>
    float GetRecoilSpeed()
    {
        float speed = 0f;

        if (IsEnganged)
        {
            if (Input.GetButton("Fire1"))
            {
                speed = -Input.GetAxisRaw("Mouse Y") * m_mouseSensitivity;
            } else
            {
                speed = m_buttonSensitivity * Time.deltaTime;
            }
        }

        return speed;
    }

    /// <summary>
    /// Moves towards the given destination in a spring like effect
    /// </summary>
    /// <param name="fromPos"></param>
    /// <param name="toPos"></param>
    /// <returns></returns>
    IEnumerator SpringRoutine(Vector3 fromPos, Vector3 toPos)
    {
        float time = 0f;

        while (Vector3.Distance(transform.position, toPos) > m_destinationProximity)
        {
            time += Time.deltaTime / m_springTime;
            transform.position = Vector3.Lerp(fromPos, toPos, time);
            yield return new WaitForFixedUpdate();
        }

        transform.position = toPos;
    }

    /// <summary>
    /// Smoothly tracks the target
    /// </summary>
    void LateUpdate()
    {
        Track();
    }

    /// <summary>
    /// Either smoothly or instantly tracks the known target's position
    /// </summary>
    /// <param name="smoothTrack"></param>
    public void Track(bool smoothTrack = true)
    {
        // Can't track unless we know what to track or with that to track it with
        if (Target == null || !TrackTarget)
        {
            return;
        }

        // Calculate the new axis position
        Vector3 targetAxis = new Vector3(
            m_axisToTrack.x * Target.position.x,
            m_axisToTrack.y * Target.position.y,
            m_axisToTrack.z * Target.position.z

        );

        Vector3 targetPosition = targetAxis + m_trackingDistance;

        if (smoothTrack)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, m_trackingSpeed * Time.deltaTime);
        } else
        {
            transform.position = targetPosition;
        }        
    }

    /// <summary>
    /// Changes the camera target to track
    /// Waits until the camera is close enough to the target before finishing
    /// This is triggered when an event to have the camera focus on something
    /// is triggered and we need to wait for the camera to finish tracking
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator ChangeTargetRoutine(Transform target)
    {
        if (target != null)
        {
            Target = target;
            while (Vector3.Distance(transform.position, target.position) > m_destinationProximity)
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }
}