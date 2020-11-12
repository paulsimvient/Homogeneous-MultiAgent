using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class FunctionRotateAxis : BEInstruction
{
    public override void BEFunction(BETargetObject targetObject, BEBlock beBlock)
    {
        Vector3 axis;

        switch (beBlock.BeInputs.stringValues[0])
        {
            case "X axis":
                axis = Vector3.right;
                break;
            case "Y axis":
                axis = Vector3.up;
                break;
            case "Z axis":
                axis = Vector3.forward;
                break;
            default:
                axis = Vector3.up;
                break;
        }

        targetObject.transform.Rotate(axis, beBlock.BeInputs.numberValues[1]);

        BeController.PlayNextOutside(beBlock);
    }
}
