using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogManager : MonoBehaviour
{
    public GSM.StateMachineProcessor machine;

    [Space]
    public Text message;
    public Button button1;
    public Text button1Text;
    public Button button2;
    public Text button2Text;
    public SpriteRenderer character;

    [Space]
    public Sprite spriteIdle;
    public Sprite spriteSurprised;
    public Sprite spriteEnjoyed;

    private void SetDialog(string message, string answer1, MyButtonDelegate answer1Event, string answer2, MyButtonDelegate answer2Event, Sprite sprite)
    {
        button1.gameObject.SetActive(answer1 != null);
        button2.gameObject.SetActive(answer2 != null);

        this.message.text = message;
        this.button1Text.text = answer1;
        this.button2Text.text = answer2;
        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();
        this.button1.onClick.AddListener(delegate { answer1Event(); });
        this.button2.onClick.AddListener(delegate { answer2Event(); });
        SetSprite(sprite);
    }

    private void SetDialog(string message, string answer1, MyButtonDelegate answer1Event, string answer2, MyButtonDelegate answer2Event)
    {
        SetDialog(message, answer1, answer1Event, answer2, answer2Event, character.sprite);
    }

    private void SetSprite(Sprite sprite)
    {
        character.sprite = sprite;
    }


    #region State Machine Callbacks

    #region Start State and outgoing edges
    public void InitDialog()
    {
        SetDialog("Welcome fellow programmer. I am Sir Ceesharp. But unfortunately I lost my glasses...",
            "Hello Sir Ceesharp!", () => machine.SendTrigger("I said hello"),
            "Hey. Can you see me?", () => machine.SendTrigger("Can you see me?"));
    }
    

    //Edge from Start State to State 1
    public void OnSaidHello()
    {
        SetDialog("Nice to meet you!\n\nWould you like to learn something about this AWESOME state machine?",
            "Yes please!", () => machine.SendTrigger("yes"),
            "No.", () => machine.SendTrigger("no"),
            spriteEnjoyed);
    }

    //Edge from Start State to State 1
    public void OnCanYouSeeMe()
    {
        SetDialog("Ha. Ha. Very funny...\nHaving some more great jokes for me, huh?",
            "Yes wait...", () => machine.SendTrigger("yes"),
            "No. I'm fine", () => machine.SendTrigger("no"), spriteSurprised);
    }
    #endregion

    #region Joke Said and outgoing edges
    public void OnNoFurtherJokes()
    {
        SetDialog("Good. So we can become serious, Sir programmer?\nDo you want to hear some more about this machine?",
            "Yes", () => machine.SendTrigger("yes"),
            "No", () => machine.SendTrigger("no"),
            spriteIdle);
    }


    public void OnFurtherJokes()
    {
        SetDialog("Okay mate, go on. How do you want to tell your joke... without... a textbox?",
            "Uhm...", () => machine.SendTrigger("uhm"),
            "TELEPATHY!!!", () => machine.SendTrigger("telepathy"),
            spriteIdle);
    }
    #endregion

    #region Input? and outgoing edges
    public void OnUhm()
    {
        SetDialog("I knew it...\nSo so you want to hear some more about this machine?",
            "Yes", () => machine.SendTrigger("yes"),
            "No", () => machine.SendTrigger("no"),
            spriteIdle);
    }

    public void OnTelepathy()
    {
        SetDialog("YOU CAN DO THAT? THAT IS REALLY COOL!!",
            "No", () => machine.SendTrigger("no"),
            "No", () => machine.SendTrigger("no"),
            spriteSurprised);
    }
    #endregion

    #region You can do that and outgoing edges
    public void OnNoTelepathy()
    {
        SetDialog("Oh man, you were kidding me right?\nDo you want to hear some more about this machine, now?",
            "Yes", () => machine.SendTrigger("yes"),
            null, null,
            spriteIdle);
    }
    #endregion

    #region No Learning and outgoing edges
    public void OnNoLearningEntered()
    {
        SetDialog("Alright, Sir programmer. You can exit the playmode now :)",
            null, null, null, null,
            spriteEnjoyed);
    }
    #endregion

    #region Learning
    public void Learning1()
    {
        SetDialog("This dialog was created by a game state machine. You can find it right in the folder where you opened this scene. Why don't you open it and see what happens while we talk?",
            "OK go on", () => machine.SendTrigger("understood"),
            null, () => machine.SendTrigger("back"),
            spriteEnjoyed);
    }


    public void Learning2()
    {
        SetDialog("The text and the buttons were set by script. But the script is called by the state machine.",
            "OK go on", () => machine.SendTrigger("understood"),
            "Go back!", () => machine.SendTrigger("back"),
            spriteEnjoyed);
    }

    public void Learning3()
    {
        message.text = "Transitions between states were made by just sending triggers.\n\nI have an idea!!!";
    }

    public void Learning4()
    {
        message.text = "In the machine you can see an unconnected state called \"FINISH\" right? Why don't you connect \"Learning 6\" with this unconnected state?";
    }

    public void Learning5()
    {
        message.text = "Just rightlick \"Learning 6\", click on \"Make Edge\" and click on the last state.";
    }

    public void Learning6()
    {
        SetDialog("If you now click on the new created edge and edit the trigger to \"finish\" (without \"\") there will be magic!!!",
            "I did!", () => { machine.SendTrigger("finish"); },
            "I did not yet", () => { machine.SendTrigger(null); });
    }

    public void OnFinish()
    {
        SetDialog("YOU DID IT!!!!!!!", null, null, null, null);
    }
    #endregion

    #endregion


    private delegate void MyButtonDelegate();

}
