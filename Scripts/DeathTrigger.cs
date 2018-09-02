using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A death trigger causes the player to instantly die upon touching it
/// </summary>
public class DeathTrigger : MonoBehaviour
{
    /// <summary>
    /// Chose an OnStay rather than an OnEnter because it tends to be more realiable
    /// as some frame updates could miss the enter state. Since we only need this to 
    /// happen once we can achieve this by ensuring that the player is not dead already
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player") && !GameManager.instance.PlayerScript.IsDead)
        {         
            GameManager.instance.TriggerPlayerDeath();
        }
    }
}
