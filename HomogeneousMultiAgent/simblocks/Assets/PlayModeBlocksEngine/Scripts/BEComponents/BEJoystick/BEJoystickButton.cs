/*
 * Play Mode Blocks Engine - Version 1.3
 * 
 * Daniel C Menezes
 * http://danielcmcg.github.io
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BEJoystickButton : MonoBehaviour
{
    public bool isPressed = false;

    void Start()
    {
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((e) => isPressed = true);
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((e) => isPressed = false);
        trigger.triggers.Add(pointerUp);
    }

}
