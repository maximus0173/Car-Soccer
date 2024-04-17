using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public enum GameOptionsMatchType
    {
        SinglePlayer,
        Multiplayer
    }

    [System.Serializable]
    public struct GameOptionsSinglePlayerDef
    {
        public int MatchTimeInMinutes;
        public string MatchPlayersCode;
    }

    [System.Serializable]
    public struct GameOptionsDef
    {
        public GameOptionsMatchType MatchType;
        public GameOptionsSinglePlayerDef SinglePlayer;
    }

    public static GameManager Instance { get; private set; }

    public GameOptionsDef GameOptions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void PlaySinglePlayer()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
