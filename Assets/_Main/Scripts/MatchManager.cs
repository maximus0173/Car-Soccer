using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;

public class MatchManager : MonoBehaviour
{

    public static MatchManager Instance { get; private set; }

    private GameInputControls inputControls;

    public enum Team
    {
        Yellow,
        Red
    }

    [System.Serializable]
    public struct MatchPlayersConfig
    {
        public string Code;
        public List<CarController> Cars;
    }

    [SerializeField] private List<CarController> yellowTeamCars = new List<CarController>();
    [SerializeField] private List<CarController> redTeamCars = new List<CarController>();
    [SerializeField] private List<Transform> yellowTeamCarSpawnPoints = new List<Transform>();
    [SerializeField] private List<Transform> redTeamCarSpawnPoints = new List<Transform>();
    [SerializeField] private GoalDetector yellowGoal;
    [SerializeField] private GoalDetector redGoal;
    [SerializeField] private List<MatchPlayersConfig> matchPlayersConfig = new();
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private Transform ball;
    [SerializeField] private MatchCamerasManager camerasManager;
    [SerializeField] private MatchResultUI matchResultUI;
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource music;

    private int yellowTeamScore = 0;
    private int redTeamScore = 0;
    private int totalMatchTime = 5 * 60;    // in seconds
    private float currentMatchTime = 0f;    // in seconds
    private bool freezeMatchTime = false;
    private bool matchFinished = false;
    private bool matchPaused = false;

    public int YellowTeamScore { get => this.yellowTeamScore; }
    public int RedTeamScore { get => this.redTeamScore; }

    public int TotalMatchTime { get => this.totalMatchTime; }
    public float CurrentMatchTime { get => this.currentMatchTime; }

    public event System.EventHandler OnScoreChanged;
    public event System.EventHandler OnMatchTimeChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ReadOptionsFromGameManager();
        this.OnMatchTimeChanged.Invoke(this, System.EventArgs.Empty);
        ResetPositions();
        this.inputControls = new GameInputControls();
        this.inputControls.Car.Enable();
        this.inputControls.Car.Menu.performed += OnMenuPressed;
    }

    private void OnDestroy()
    {
        this.inputControls.Car.Menu.performed -= OnMenuPressed;
        this.inputControls.Car.Disable();
    }

    private void Update()
    {
        this.UpdateMatchTime();
    }

    private void ReadOptionsFromGameManager()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        switch (GameManager.Instance.GameOptions.MatchType)
        {
            case GameManager.GameOptionsMatchType.SinglePlayer:
                this.totalMatchTime = GameManager.Instance.GameOptions.SinglePlayer.MatchTimeInMinutes * 60;
                foreach (CarController car in this.yellowTeamCars.Union(this.redTeamCars))
                {
                    car.gameObject.SetActive(false);
                }
                foreach (MatchPlayersConfig config in this.matchPlayersConfig)
                {
                    if (config.Code.Equals(GameManager.Instance.GameOptions.SinglePlayer.MatchPlayersCode))
                    {
                        foreach (CarController car in config.Cars)
                        {
                            car.gameObject.SetActive(true);
                        }
                    }
                }
                break;
        }
    }

    private void ResetPositions()
    {
        Rigidbody ballRigidbody = this.ball.GetComponent<Rigidbody>();
        ballRigidbody.position = this.ballSpawnPoint.position;
        ballRigidbody.rotation = this.ballSpawnPoint.rotation;
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        for (int i = 0; i < this.yellowTeamCars.Count; i++)
        {
            if (i < this.yellowTeamCarSpawnPoints.Count)
            {
                this.yellowTeamCars[i].MoveToPosition(this.yellowTeamCarSpawnPoints[i].position, this.yellowTeamCarSpawnPoints[i].rotation);
            }
        }

        for (int i = 0; i < this.redTeamCars.Count; i++)
        {
            if (i < this.redTeamCarSpawnPoints.Count)
            {
                this.redTeamCars[i].MoveToPosition(this.redTeamCarSpawnPoints[i].position, this.redTeamCarSpawnPoints[i].rotation);
            }
        }
    }

    public void AddYellowTeamPoint()
    {
        this.yellowTeamScore++;
        this.OnScoreChanged?.Invoke(this, System.EventArgs.Empty);
        ShowGoalScoredCelebration(Team.Yellow);
    }

    public void AddRedTeamPoint()
    {
        this.redTeamScore++;
        this.OnScoreChanged?.Invoke(this, System.EventArgs.Empty);
        ShowGoalScoredCelebration(Team.Red);
    }

    private void ShowGoalScoredCelebration(Team forTeam)
    {
        this.freezeMatchTime = true;
        this.yellowGoal.enabled = false;
        this.redGoal.enabled = false;
        this.camerasManager.DoGoalScoredCelebration(forTeam, FinishGoalScoredCelebration);
        foreach (CarController car in this.yellowTeamCars.Union(this.redTeamCars))
        {
            if (car.gameObject.activeSelf && car.GetComponent<CarAI>() != null)
            {
                car.Freeze();
            }
        }
    }

    private void FinishGoalScoredCelebration()
    {
        ResetPositions();
        StartCoroutine(BeginMatchAfterGoal());
    }

    IEnumerator BeginMatchAfterGoal()
    {
        foreach (CarController car in this.yellowTeamCars.Union(this.redTeamCars))
        {
            if (car.gameObject.activeSelf)
            {
                car.Freeze();
            }
        }
        yield return new WaitForSeconds(2f);
        this.freezeMatchTime = false;
        this.yellowGoal.enabled = true;
        this.redGoal.enabled = true;
        foreach (CarController car in this.yellowTeamCars.Union(this.redTeamCars))
        {
            if (car.gameObject.activeSelf)
            {
                car.UnFreeze();
            }
        }
    }

    private void UpdateMatchTime()
    {
        if (this.freezeMatchTime || this.matchFinished)
        {
            return;
        }
        this.currentMatchTime += Time.deltaTime;
        if (this.currentMatchTime > this.totalMatchTime)
        {
            this.currentMatchTime = this.totalMatchTime;
            FinishMatch();
        }
        this.OnMatchTimeChanged.Invoke(this, System.EventArgs.Empty);
    }

    private void FinishMatch()
    {
        this.matchFinished = true;
        this.yellowGoal.enabled = false;
        this.redGoal.enabled = false;
        foreach (CarController car in this.yellowTeamCars.Union(this.redTeamCars))
        {
            if (car.gameObject.activeSelf)
            {
                car.Freeze();
            }
        }
        if (this.yellowTeamScore > this.redTeamScore)
        {
            this.matchResultUI.ShowWinResult();
        } else if (this.yellowTeamScore < this.redTeamScore)
        {
            this.matchResultUI.ShowLoseResult();
        }
        else if (this.yellowTeamScore == this.redTeamScore)
        {
            this.matchResultUI.ShowTiedResult();
        }
    }

    public void GoToMainMenu()
    {
        this.HandleUnPause();
        GameManager.Instance.ShowMainMenu();
    }

    public void ReplayMatch()
    {
        this.HandleUnPause();
        GameManager.Instance.ReloadScene();
    }

    private void OnMenuPressed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        HandlePauseToggle();
    }

    public void UnpauseMatch()
    {
        HandlePauseToggle();
    }

    private void HandlePauseToggle()
    {
        if (!this.matchPaused)
        {
            HandlePause();
        }
        else
        {
            HandleUnPause();
        }
    }

    private void HandlePause()
    {
        if (this.matchPaused)
        {
            return;
        }
        this.matchPaused = true;
        Time.timeScale = 0f;
        this.music.Pause();
        this.audioMixer.SetFloat("MasterVolume", -80f);
        this.pauseMenuUI.Show();
    }

    private void HandleUnPause()
    {
        if (!this.matchPaused)
        {
            return;
        }
        this.matchPaused = false;
        this.pauseMenuUI.Hide();
        Time.timeScale = 1f;
        this.music.UnPause();
        this.audioMixer.SetFloat("MasterVolume", 0f);
    }

}
