using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchResultUI : MonoBehaviour
{

    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject winLabel;
    [SerializeField] private GameObject loseLabel;
    [SerializeField] private GameObject tiedLabel;

    private void ClearLabels()
    {
        this.winLabel.SetActive(false);
        this.loseLabel.SetActive(false);
        this.tiedLabel.SetActive(false);
    }

    public void ShowWinResult()
    {
        ClearLabels();
        this.panel.SetActive(true);
        this.winLabel.SetActive(true);
    }

    public void ShowLoseResult()
    {
        ClearLabels();
        this.panel.SetActive(true);
        this.loseLabel.SetActive(true);
    }

    public void ShowTiedResult()
    {
        ClearLabels();
        this.panel.SetActive(true);
        this.tiedLabel.SetActive(true);
    }

}
