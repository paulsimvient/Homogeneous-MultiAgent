using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class FunctionWait : BEInstruction
{
    float counter = 0;
    
    public override void BEFunction(BETargetObject targetObject, BEBlock beBlock)
    {
        if (beBlock.beBlockFirstPlay)
        {
            counter = beBlock.BeInputs.numberValues[0];
            
            beBlock.beBlockFirstPlay = false;
        }
        if (counter > 0)
        {
            counter -= Time.deltaTime;
        }
        else
        {
            beBlock.beBlockFirstPlay = true;
            counter = 0;
            BeController.PlayNextOutside(beBlock);
        }
    }
}
