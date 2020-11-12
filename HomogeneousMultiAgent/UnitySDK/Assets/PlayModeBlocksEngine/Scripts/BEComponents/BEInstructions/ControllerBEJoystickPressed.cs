using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControllerBEJoystickPressed : BEInstruction
{
    BEJoystickButton beJoyButton;

    public override void BEFunction(BETargetObject targetObject, BEBlock beBlock)
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
            beBlock.BeBlockGroup.isActive = true;
            BeController.PlayNextInside(beBlock);
        }
        else
        {
            beBlock.BeBlockGroup.isActive = false;
            BeController.StopGroup(beBlock.BeBlockGroup);
        }
    }
}
