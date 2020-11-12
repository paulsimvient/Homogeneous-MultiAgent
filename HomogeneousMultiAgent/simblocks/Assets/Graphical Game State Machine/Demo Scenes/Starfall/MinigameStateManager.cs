using GSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameStateManager : MonoBehaviour
{
    
    public GSM.StateMachineProcessor machineBehaviour;
    private GraphicalStateMachine machine;

    [Space]
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject ui;
    public GameObject game;
    public Text countdownText;

    public Text scoreText;

    [Space]
    public GameObject playerPrefab;
    public GameObject starPrefab;
    public float movementSpeed = 1;
    public float border = 7;
    public float playerHeight = -2.85f;
    public float starFallSpeed = 12;
    public Vector2 starFallSpawnTime;

    private int score = 0;

    private GameObject player;
    public List<GameObject> stars = new List<GameObject>();

    void Start()
    {
        machine = machineBehaviour.Machine;
    }

    public void SetMainMenuVisibility(bool visible)
    {
        mainMenu.SetActive(visible);
    }

    public void SetPauseMenuVisibility(bool visible)
    {
        pauseMenu.SetActive(visible);
    }

    public void SetGameUIVisibility(bool visible)
    {
        ui.SetActive(visible);
    }

    public void SetCountdownVisibility(bool visible)
    {
        countdownText.gameObject.SetActive(visible);
    }

    public void OnGameExit()
    {
        SetPauseMenuVisibility(false);
        foreach (var star in stars)
        {
            Destroy(star);
        }
        stars = new List<GameObject>();
        Destroy(player);
    }


    public void OnStartGame()
    {
        player = Instantiate(playerPrefab, game.transform);
        player.transform.position = new Vector2(0, playerHeight);
        player.GetComponent<MinigamePlayer>().manager = this;
    }


    public void AddScore(int score)
    {
        this.score += score;
        scoreText.text = this.score + "";
    }

    public void OnGameStateLeft()
    {
        SetGameUIVisibility(false);
        StopCoroutine(spawnCoroutine);
    }

    private Coroutine spawnCoroutine;

    public void OnGameStateEntered()
    {
        SetGameUIVisibility(true);
        spawnCoroutine = StartCoroutine(SpawnStars());
    }

    private IEnumerator SpawnStars()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(starFallSpawnTime.x, starFallSpawnTime.y));
        var star = Instantiate(starPrefab);
        star.transform.position = new Vector2(UnityEngine.Random.Range(-border, border), 7);
        stars.Add(star);
        yield return SpawnStars();
    }


    public void OnGameUpdate()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            machine.SendTrigger("pause");
            return;
        }

        float move = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move -= 1;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            move += 1;
        }

        player.transform.position = new Vector2(
            Mathf.Clamp(player.transform.position.x + move * Time.deltaTime * movementSpeed, -border, border), 
            playerHeight);


        foreach (var star in stars)
        {
            star.transform.position += Vector3.down * Time.deltaTime * starFallSpeed;
        }
    }


    float countdown;
    public void OnStartCountdown()
    {
        countdown = 3;
    }

    //Acts like a simple Update()-method while Countdown state is active
    public void OnStayCountdown()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            machine.SendTrigger("pause");
            return;
        }

        countdown -= Time.deltaTime;
        if(countdown <= 0)
        {
            machine.SendTrigger("countdown done");
            return;
        }
        countdownText.text = Mathf.Ceil(countdown) + "";
    }


    //Acts like a Update()-method while Countdown Paused or paused state is active
    public void OnStayPaused()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            machine.SendTrigger("continue");
            return;
        }
    }

}
