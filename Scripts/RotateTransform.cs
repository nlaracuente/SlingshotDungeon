using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates the attached transform indefinitely
/// </summary>
public class RotateTransform : MonoBehaviour
{
    /// <summary>
    /// How fast to rotate
    /// </summary>
    [SerializeField]
    float m_rotationSpeed = 90f;

    /// <summary>
    /// Which axis to rotate
    /// </summary>
    [SerializeField]
    Vector3 m_rotationAxis = Vector3.up;

    /// <summary>
    /// Triggers the rotation routine
    /// </summary>
    void Start()
    {
        StartCoroutine("RotateRoutine");
    }

    /// <summary>
    /// Continuously rotate this object at the given rate
    /// </summary>
    /// <returns></returns>
    IEnumerator RotateRoutine()
    {
        while (true)
        {
            transform.Rotate(m_rotationAxis * m_rotationSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
}
