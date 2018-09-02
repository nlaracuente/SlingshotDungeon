using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoilable : MonoBehaviour, IRecoilable
{
    /// <summary>
    /// A reference to the Recoil component
    /// </summary>
    Recoil m_recoil;

    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        m_recoil = GetComponent<Recoil>();
    }

    public void AddRecoilForce(Vector3 recoilForce, ForceMode mode = ForceMode.Impulse)
    {
        m_recoil.AddForce(recoilForce, mode);
    }

    public bool CanRecoil()
    {
        return m_recoil.IsGrounded;
    }
}
