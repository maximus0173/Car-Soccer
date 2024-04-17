using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{

    [SerializeField] private GameObject panel;

    public void Show()
    {
        this.panel.SetActive(true);
    }

    public void Hide()
    {
        this.panel.SetActive(false);
    }

}
