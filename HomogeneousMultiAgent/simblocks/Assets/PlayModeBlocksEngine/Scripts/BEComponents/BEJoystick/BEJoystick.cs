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
using UnityEngine.UI;

//v1.2 -Added BEJoystick (virtual joystick), BEJoystickButton and BEJoystick Trigger/Event Blocks
public class BEJoystick : MonoBehaviour
{
    public BEJoystickButton arrowUpButton;
    public BEJoystickButton arrowLeftButton;
    public BEJoystickButton arrowDownButton;
    public BEJoystickButton arrowRightButton;
    public BEJoystickButton buttonA;
    public BEJoystickButton buttonB;

    void Start()
    {
        arrowUpButton = GetButtonRef("ArrowUp");
        arrowLeftButton = GetButtonRef("ArrowLeft");
        arrowDownButton = GetButtonRef("ArrowDown");
        arrowRightButton = GetButtonRef("ArrowRight");
        buttonA = GetButtonRef("ButtonA");
        buttonB = GetButtonRef("ButtonB");
    }

    BEJoystickButton GetButtonRef(string name)
    {
        BEJoystickButton button = null;
        foreach (Transform child in transform)
        {
            if (child.name == name)
            {
                button = child.GetComponent<BEJoystickButton>();
                break;
            }
        }
        return button;
    }

    void Update()
    {

    }
}
