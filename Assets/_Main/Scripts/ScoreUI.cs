using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{

    [SerializeField] private TMP_Text greenTeamScoreText;
    [SerializeField] private TMP_Text redTeamScoreText;
    [SerializeField] private TMP_Text matchTimeText;

    private void Start()
    {
        MatchManager.Instance.OnScoreChanged += MatchManager_OnScoreChanged;
        MatchManager.Instance.OnMatchTimeChanged += MatchManager_OnMatchTimeChanged;
    }

    private void OnDestroy()
    {
        MatchManager.Instance.OnScoreChanged -= MatchManager_OnScoreChanged;
        MatchManager.Instance.OnMatchTimeChanged -= MatchManager_OnMatchTimeChanged;
    }

    private void MatchManager_OnScoreChanged(object sender, System.EventArgs e)
    {
        UpdateScore();
    }

    private void MatchManager_OnMatchTimeChanged(object sender, System.EventArgs e)
    {
        UpdateMatchTime();
    }

    private void UpdateScore()
    {
        this.greenTeamScoreText.text = MatchManager.Instance.YellowTeamScore.ToString();
        this.redTeamScoreText.text = MatchManager.Instance.RedTeamScore.ToString();
    }

    private void UpdateMatchTime()
    {
        int matchTimeLeft = MatchManager.Instance.TotalMatchTime - (int)MatchManager.Instance.CurrentMatchTime;
        if (matchTimeLeft < 0)
        {
            matchTimeLeft = 0;
        }
        System.TimeSpan t = System.TimeSpan.FromSeconds(matchTimeLeft);
        this.matchTimeText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

}
