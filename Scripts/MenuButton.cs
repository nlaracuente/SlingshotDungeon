using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles playing sound on over and button click
/// </summary>
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    /// <summary>
    /// A reference to the button component
    /// </summary>
    Button m_button;

    /// <summary>
    /// Initialize
    /// </summary>
    void Start()
    {
        m_button = GetComponent <Button>();    
    }

    public void OnPointerEnter(PointerEventData ped)
    {
        if (m_button.interactable)
        {
            AudioManager.instance.PlaySound(AudioName.ButtonHover);
        }        
    }

    public void OnPointerDown(PointerEventData ped)
    {
        if (m_button.interactable)
        {
            AudioManager.instance.PlaySound(AudioName.ButtonClick);
        }
    }
}
