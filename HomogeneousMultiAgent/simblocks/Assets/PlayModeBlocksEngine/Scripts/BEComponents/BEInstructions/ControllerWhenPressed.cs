using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ControllerWhenPressed : BEInstruction
{
    KeyCode key;

    public override void BEFunction(BETargetObject targetObject, BEBlock beBlock)
    {
        try
        {
            key = (KeyCode)System.Enum.Parse(typeof(KeyCode), beBlock.BeInputs.stringValues[0]);
        }
        catch(Exception e)
        {
            Debug.Log("probably still initializing");
            Debug.Log(e);
        }

        if (Input.GetKey(key))
        {
            beBlock.BeBlockGroup.isActive = true;
            BeController.PlayNextInside(beBlock);
        }
        else if (!Input.GetKey(key))
        {
            beBlock.BeBlockGroup.isActive = false;
            BeController.StopGroup(beBlock.BeBlockGroup);
        }
    }

}
