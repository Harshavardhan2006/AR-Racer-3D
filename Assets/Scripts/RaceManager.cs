using TMPro;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    [Header("HUD references")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text lapText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text statusText;

    [Header("Race settings")]
    [SerializeField] private int totalLaps = 3;

    private float elapsedTime;
    private int coinsCollected;
    private int completedLaps;
    private int expectedCheckpoint;
    private bool crossedStartLine;
    private bool raceRunning;
    private bool raceFinished;

    public bool IsRaceFinished => raceFinished;
    public int CoinsCollected => coinsCollected;
    public int CompletedLaps => completedLaps;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateHud();
    }

    public void BindHud(
        TMP_Text speed,
        TMP_Text timer,
        TMP_Text lap,
        TMP_Text coin,
        TMP_Text status)
    {
        speedText = speed;
        timerText = timer;
        lapText = lap;
        coinText = coin;
        statusText = status;
        UpdateHud();

        if (statusText != null)
            statusText.text = "Place the track to start.";
    }

    private void Update()
    {
        if (!raceRunning || raceFinished)
            return;

        elapsedTime += Time.deltaTime;

        if (timerText != null)
            timerText.text = "TIME  " + elapsedTime.ToString("F1") + " s";
    }

    public void BeginRace(CarController car)
    {
        elapsedTime = 0f;
        coinsCollected = 0;
        completedLaps = 0;
        expectedCheckpoint = 0;
        crossedStartLine = false;
        raceFinished = false;
        raceRunning = true;

        if (statusText != null)
            statusText.text = "GO! Follow the checkpoints.";

        UpdateHud();
    }

    public void UpdateSpeed(float speedKmh)
    {
        if (speedText != null)
            speedText.text = "SPEED  " + speedKmh.ToString("F0") + " km/h";
    }

    public void AddCoin()
    {
        if (raceFinished)
            return;

        coinsCollected++;
        if (coinText != null)
            coinText.text = "COINS  " + coinsCollected;
    }

    public void PassCheckpoint(int index)
    {
        if (!raceRunning || raceFinished)
            return;

        if (!crossedStartLine)
        {
            if (index == 0)
            {
                crossedStartLine = true;
                expectedCheckpoint = 1;
                if (statusText != null)
                    statusText.text = "Lap started!";
            }

            return;
        }

        if (index != expectedCheckpoint)
            return;

        if (index == 3)
        {
            expectedCheckpoint = 0;
            return;
        }

        if (index == 0)
        {
            completedLaps++;
            expectedCheckpoint = 1;

            if (lapText != null)
                lapText.text = "LAP  " + completedLaps + "/" + totalLaps;

            if (completedLaps >= totalLaps)
                FinishRace();

            return;
        }

        expectedCheckpoint++;
    }

    private void FinishRace()
    {
        raceFinished = true;
        raceRunning = false;

        if (statusText != null)
        {
            statusText.text =
                "RACE COMPLETE!\n" +
                "Time: " + elapsedTime.ToString("F1") + " s\n" +
                "Coins: " + coinsCollected;
        }
    }

    public void ResetRace()
    {
        elapsedTime = 0f;
        coinsCollected = 0;
        completedLaps = 0;
        expectedCheckpoint = 0;
        crossedStartLine = false;
        raceRunning = false;
        raceFinished = false;
        UpdateHud();

        if (statusText != null)
            statusText.text = "Place the track to start.";
    }

    private void UpdateHud()
    {
        if (speedText != null)
            speedText.text = "SPEED  0 km/h";

        if (timerText != null)
            timerText.text = "TIME  0.0 s";

        if (lapText != null)
            lapText.text = "LAP  0/" + totalLaps;

        if (coinText != null)
            coinText.text = "COINS  0";
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
