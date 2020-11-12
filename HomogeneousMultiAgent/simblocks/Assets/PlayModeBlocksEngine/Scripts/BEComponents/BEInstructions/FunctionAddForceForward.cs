using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class FunctionAddForceForward : BEInstruction
{
    public override void BEFunction(BETargetObject targetObject, BEBlock beBlock)
    {
        if (targetObject.GetComponent<Rigidbody2D>())
        {
            targetObject.GetComponent<Rigidbody2D>().AddForce(targetObject.transform.right * beBlock.BeInputs.numberValues[0]);
        }
        else if(targetObject.GetComponent<Rigidbody>())
        {
            targetObject.GetComponent<Rigidbody>().AddForce(targetObject.transform.forward * beBlock.BeInputs.numberValues[0]);
        }

        BeController.PlayNextOutside(beBlock);
    }
}



