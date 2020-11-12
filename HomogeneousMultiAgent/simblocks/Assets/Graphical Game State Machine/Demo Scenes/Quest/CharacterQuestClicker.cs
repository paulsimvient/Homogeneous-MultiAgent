using GSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterQuestClicker : MonoBehaviour
{

    public string triggerToSendOnClick = "talked";

    public void OnMouseDown()
    {
        QuestStateMachineManager.machine.SendTrigger(triggerToSendOnClick);
    }
}
