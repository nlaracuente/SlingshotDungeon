using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicChanger : MonoBehaviour
{

    [SerializeField]
    int m_level = 0;

    bool m_isChanged = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !m_isChanged)
        {
            m_isChanged = true;
            GameManager.instance.NewLevelReached(m_level);
        }
    }
}
