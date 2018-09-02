using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBlock : MonoBehaviour
{
    Collider m_collider;

    void Start()
    {
        m_collider = GetComponent<Collider>();
        Recoil recoil = GetComponent<Recoil>();
        recoil.OnLand += OnBlockLands;
    }

    /// <summary>
    /// Triggers the sound of the key landing
    /// </summary>
    public void OnBlockLands()
    {
        // Only if they are visible on screen then play the sound
        if (RecoilCamera.IsVisible(m_collider))
        {
            AudioManager.instance.PlaySound(AudioName.SpikeBlockLands);
        }        
    }
}
