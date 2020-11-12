using UnityEngine;
using System.Collections;

public class OperationBEJoystickPressed : BEInstruction
{

    BEJoystickButton beJoyButton;
    string result;

    public override string BEOperation(BETargetObject targetObject, BEBlock beBlock)
    {
        switch (beBlock.BeInputs.stringValues[0])
        {
            case "ArrowUp":
                beJoyButton = BeController.beJoystick.arrowUpButton;
                break;
            case "ArrowLeft":
                beJoyButton = BeController.beJoystick.arrowLeftButton;
                break;
            case "ArrowDown":
                beJoyButton = BeController.beJoystick.arrowDownButton;
                break;
            case "ArrowRight":
                beJoyButton = BeController.beJoystick.arrowRightButton;
                break;
            case "ButtonA":
                beJoyButton = BeController.beJoystick.buttonA;
                break;
            case "ButtonB":
                beJoyButton = BeController.beJoystick.buttonB;
                break;
            default:
                beJoyButton = null;
                break;
        }

        if (beJoyButton.isPressed)
        {
            result = "1";
        }
        else
        {
            result = "0";
        }

        return result;
    }

}
