using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject ballPrefab;
    public Vector3 ballSpawnPosition;

    public Text runsText;
    public Text oversText;
    public Text overMessageText;
    public Text gameOverText;
    public Text wicketsText;
    public Text outMessageText;

    public int totalRuns = 0;
    private int ballsBowled = 0;
    private int totalBalls = 12;
    private int wicketsDown = 0;

    private FielderController[] fielders;
    private bool isBallActive = false;
    private bool gameOver = false;

    public Transform batsman;
    public Vector3 batsmanPosition { get { return batsman.position; } }

    private void Awake()
    {
        instance = this;
        UpdateRuns(0);
        UpdateOversText();
        overMessageText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        outMessageText.gameObject.SetActive(false);
        wicketsText.gameObject.SetActive(true);
    }

    void Start()
    {
        fielders = FindObjectsOfType<FielderController>();
        UpdateWicketsText();
    }

    private void Update()
    {
        if (!gameOver && Input.GetKeyDown(KeyCode.Space))
        {
            if (!isBallActive)
            {
                ResetFielders();
                ThrowBall();
            }
        }
    }

    private void ResetFielders()
    {
        foreach (FielderController fielder in fielders)
        {
            fielder.ResetPosition();
        }
    }

    private void ThrowBall()
    {
        isBallActive = true;
        Instantiate(ballPrefab, ballSpawnPosition, Quaternion.identity);
    }

    public void OnBallResolved(bool isValidBall)
    {
        if (gameOver) return;

        if (isValidBall)
        {
            ballsBowled++;
        }

        UpdateOversText();

        if (ballsBowled >= totalBalls || wicketsDown >= 3)
        {
            GameOver();
        }
        else if (ballsBowled % 6 == 0)
        {
            StartCoroutine(ShowOverMessage());
        }

        isBallActive = false;
    }

    public void UpdateRuns(int runs)
    {
        totalRuns += runs;
        runsText.text = "Runs: " + totalRuns;
    }

    private void UpdateOversText()
    {
        int currentOver = ballsBowled / 6;
        int currentBall = ballsBowled % 6;
        oversText.text = "Overs: " + currentOver + "." + currentBall + "/2";
    }

    public void IncrementWicketCount()
    {
        wicketsDown++;
        UpdateWicketsText();

        if (wicketsDown >= 3)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(ShowOutMessage());
        }
    }

    private void UpdateWicketsText()
    {
        wicketsText.text = "Wickets: " + wicketsDown + "/3";
    }

    private void GameOver()
    {
        gameOver = true;
        gameOverText.gameObject.SetActive(true);
        gameOverText.text = "Game Over!";
        isBallActive = false;
    }

    public IEnumerator ShowOverMessage()
    {
        overMessageText.gameObject.SetActive(true);
        overMessageText.text = "Over up!";
        yield return new WaitForSeconds(2f);
        overMessageText.gameObject.SetActive(false);
    }

    public IEnumerator ShowOutMessage()
    {
        outMessageText.gameObject.SetActive(true);
        outMessageText.text = "Out!";
        yield return new WaitForSeconds(2f);
        outMessageText.gameObject.SetActive(false);
    }

    public IEnumerator ShowWideMessage()
    {
        overMessageText.gameObject.SetActive(true);
        overMessageText.text = "Wide!";
        yield return new WaitForSeconds(2f);
        overMessageText.gameObject.SetActive(false);
    }

    public void OnNoBall()
    {
        UpdateRuns(1);  // Add 1 run for no-ball
        StartCoroutine(ShowNoBallMessage());
    }

    public IEnumerator ShowNoBallMessage()
    {
        overMessageText.gameObject.SetActive(true);
        overMessageText.text = "No Ball!";
        yield return new WaitForSeconds(2f);
        overMessageText.gameObject.SetActive(false);
    }
}
