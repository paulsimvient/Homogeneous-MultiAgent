using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Game settings and rules
/// </summary>
public class GameController : MonoBehaviour
{
    PathCreator pathCreator;
    CameraFraming cameraFramer;
    public GameObject mainPanel;
    public GameObject sandboxScene;
    BEController beController;
    BETargetObject beTargetObject;
    Rigidbody beTargetObjectRigdbody;
    bool gameIsPlaying;
    int status;
    Renderer[] sandBoxBounds;
    Renderer[] mapBounds;
    public Transform particlesWinGameGO;
    ParticleSystem particlesWinGame;
    public Transform particlesLoseGameGO;
    ParticleSystem particlesLoseGame;
    public Text menuMessage;
    string defaultMessage = "Make your robot reach the Goal!";
    string loseMessage = "Your robot fell ლ(╥_╥ლ)\nTry again!";
    string winMessage = "Good job! (☞ﾟヮﾟ)☞\nTry a different mode!";

    void Start()
    {
        menuMessage.text = defaultMessage;
        particlesWinGame = particlesWinGameGO.GetChild(0).GetComponent<ParticleSystem>();
        particlesLoseGame = particlesLoseGameGO.GetChild(0).GetComponent<ParticleSystem>();

        pathCreator = GetComponent<PathCreator>();
        cameraFramer = GetComponent<CameraFraming>();
        beController = pathCreator.beController;
        beTargetObject = BEController.beTargetObjectList[0];
        beTargetObjectRigdbody = beTargetObject.GetComponent<Rigidbody>();
        gameIsPlaying = false;
        sandBoxBounds = new Renderer[sandboxScene.transform.GetComponentsInChildren<BoxCollider>().Length];
        var i = 0;
        foreach (BoxCollider collider in sandboxScene.transform.GetComponentsInChildren<BoxCollider>())
        {
            sandBoxBounds[i] = collider.GetComponent<Renderer>();
            i++;
        }
    }

    public void StartGame()
    {
        menuMessage.text = defaultMessage;
        mainPanel.SetActive(false);
        particlesWinGame.Stop();
        particlesWinGameGO.position = new Vector3(0, 0, -200);
        Dropdown mode = mainPanel.GetComponentInChildren<Dropdown>();
        SetMode(mode.options[mode.value].text);
        cameraFramer.CentralizeCamera(mapBounds);
        beTargetObjectRigdbody.isKinematic = false;
        status = 0;
        gameIsPlaying = true;
    }

    public void EndGame()
    {
        gameIsPlaying = false;
        beTargetObjectRigdbody.isKinematic = true;
        mainPanel.SetActive(true);
    }

    void Update()
    {
        if (gameIsPlaying && status == 0)
        {
            VerifyRules();
        }
    }

    void VerifyRules()
    {
        if (beTargetObject.transform.position.y < -3)
        {
            menuMessage.text = loseMessage;
            particlesLoseGameGO.position = beTargetObject.transform.position;
            particlesLoseGame.Play();
            status = 2;
            EndGame();
        }

        if (Vector3.Distance(beTargetObject.transform.position, pathCreator.pathGoal.transform.position) < 1.1f)
        {
            menuMessage.text = winMessage;
            particlesWinGameGO.position = pathCreator.pathGoal.transform.position;
            particlesWinGame.Play();
            status = 1;
            EndGame();
        }
    }

    public void Cancel()
    {
        beTargetObjectRigdbody.isKinematic = false;
        mainPanel.SetActive(false);
        if (status != 0)
        {
            gameIsPlaying = true;
        }
    }

    /// <summary>
    /// Set scene parameters to match scene mode
    /// </summary>
    /// <param name="mode"></param>
    void SetMode(string mode)
    {
        switch (mode)
        {
            case "Easy":
                sandboxScene.SetActive(false);
                pathCreator.numberOfPathTiles = 5;
                pathCreator.minLineSize = 2;
                pathCreator.maxLineSize = 3;
                pathCreator.maxConsecutiveTurns = 1;
                mapBounds = pathCreator.CreatePath();
                break;
            case "Medium":
                sandboxScene.SetActive(false);
                pathCreator.numberOfPathTiles = 10;
                pathCreator.minLineSize = 3;
                pathCreator.maxLineSize = 4;
                pathCreator.maxConsecutiveTurns = 2;
                mapBounds = pathCreator.CreatePath();
                break;
            case "Hard":
                sandboxScene.SetActive(false);
                pathCreator.numberOfPathTiles = 20;
                pathCreator.minLineSize = 3;
                pathCreator.maxLineSize = 4;
                pathCreator.maxConsecutiveTurns = 2;
                mapBounds = pathCreator.CreatePath();
                break;
            case "Sandbox":
                mapBounds = sandBoxBounds;
                sandboxScene.SetActive(true);
                beTargetObject.transform.position = Vector3.zero;
                beTargetObject.transform.rotation = Quaternion.identity;
                break;
            default:
                break;
        }
    }

}
