using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A recoilable object can be applied recoil force
/// </summary>
public interface IRecoilable
{
    /// <summary>
    /// True when the object is in state where recoil force can be applied
    /// </summary>
    /// <returns></returns>
    bool CanRecoil();

    /// <summary>
    /// Applies the given recoil force to the object
    /// </summary>
    /// <param name="recoilForce"></param>
    void AddRecoilForce(Vector3 recoilForce, ForceMode mode);
}
