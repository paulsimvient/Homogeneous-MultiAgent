using GSM;
using UnityEngine;
using UnityEngine.UI;

public class QuestStateMachineManager : MonoBehaviour
{
    public static GraphicalStateMachine machine;

    public GameObject timmy;
    public GameObject billy;
    public GameObject liz;
    public GameObject greg;
    public GameObject lilly;
    public GameObject emily;
    [Space]
    public GameObject highlighter;
    [Space]
    public Text message;
    public Button button1;
    public Button button2;
    public Button button3;

    void Start()
    {
        machine = GetComponent<StateMachineProcessor>().Machine;
    }

    public void HighlightCharacter(string name)
    {
        switch (name)
        {
            case "timmy":
                HighlightCharacter(timmy);
                return;
            case "billy":
                HighlightCharacter(billy);
                return;
            case "liz":
                HighlightCharacter(liz);
                return;
            case "greg":
                HighlightCharacter(greg);
                return;
            case "lilly":
                HighlightCharacter(lilly);
                return;
            case "emily":
                HighlightCharacter(emily);
                return;
            default:
                break;
        }
    }

    public void HighlightCharacter(GameObject g)
    {
        highlighter.transform.position = g.transform.position;
        highlighter.SetActive(true);
    }

    public void SetMessage(string text)
    {
        message.text = text;
    }

    public void SetButtonVisibility(bool b)
    {
        button1.gameObject.SetActive(b);
        button2.gameObject.SetActive(b);
        button3.gameObject.SetActive(b);
    }

}
