using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    bool m_triggerd = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !m_triggerd)
        {
            m_triggerd = true;
            GameManager.instance.TriggerPlayerVictory();
        }
    }
}
