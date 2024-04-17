using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class MainMenuManager : MonoBehaviour
{

    [System.Serializable]
    public struct MatchTimeDef
    {
        public int Minutes;
        public string Text;
    }

    [System.Serializable]
    public struct MatchPlayersDef
    {
        public string Code;
        public string Text;
    }

    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject singlePlayerPanel;
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private List<MatchTimeDef> singlePlayerMatchTimeDefs = new();
    [SerializeField] private List<MatchPlayersDef> singlePlayerMatchPlayersDefs = new();
    [SerializeField] private int singlePlayerSelectedMatchTimeIndex = -1;
    [SerializeField] private int singlePlayerSelectedMatchPlayersIndex = -1;

    [SerializeField] private TMP_Text singlePlayerMatchTimeText;
    [SerializeField] private TMP_Text singlePlayerMatchPlayersText;
    [SerializeField] private TMP_Text optionsMusicVolumeText;
    [SerializeField] private TMP_Text optionsSfxVolumeText;

    private int optionsMusicVolume = 80;
    private int optionsSfxVolume = 80;

    private void Start()
    {
        this.ShowMainPanel();
        this.UpdateMatchTimeText();
        this.UpdateMatchPlayersText();
        this.optionsMusicVolumeText.isRightToLeftText = false;
        this.optionsSfxVolumeText.isRightToLeftText = false;
        this.ReadAudioMixerVolumes();
        this.UpdateOptionsMusicVolumeText();
        this.UpdateOptionsSfxVolumeText();
    }

    public void ShowMainPanel()
    {
        this.mainPanel.SetActive(true);
        this.singlePlayerPanel.SetActive(false);
        this.multiplayerPanel.SetActive(false);
        this.optionsPanel.SetActive(false);
    }

    public void ShowSinglePlayerPanel()
    {
        this.mainPanel.SetActive(false);
        this.singlePlayerPanel.SetActive(true);
        this.multiplayerPanel.SetActive(false);
        this.optionsPanel.SetActive(false);
    }

    public void ShowMultiplayerPanel()
    {
        this.mainPanel.SetActive(false);
        this.singlePlayerPanel.SetActive(false);
        this.multiplayerPanel.SetActive(true);
        this.optionsPanel.SetActive(false);
    }

    public void ShowOptionsPanel()
    {
        this.mainPanel.SetActive(false);
        this.singlePlayerPanel.SetActive(false);
        this.multiplayerPanel.SetActive(false);
        this.optionsPanel.SetActive(true);
    }

    public void DoSinglePlayerMatchTimeIncrease()
    {
        this.singlePlayerSelectedMatchTimeIndex++;
        this.UpdateMatchTimeText();
    }

    public void DoSinglePlayerMatchTimeDecrease()
    {
        this.singlePlayerSelectedMatchTimeIndex--;
        this.UpdateMatchTimeText();
    }

    private void UpdateMatchTimeText()
    {
        this.singlePlayerSelectedMatchTimeIndex = Mathf.Clamp(this.singlePlayerSelectedMatchTimeIndex, 0, this.singlePlayerMatchTimeDefs.Count - 1);
        this.singlePlayerMatchTimeText.text = this.singlePlayerMatchTimeDefs[this.singlePlayerSelectedMatchTimeIndex].Text;
    }

    public void DoSinglePlayerMatchPlayersIncrease()
    {
        this.singlePlayerSelectedMatchPlayersIndex++;
        this.UpdateMatchPlayersText();
    }

    public void DoSinglePlayerMatchPlayersDecrease()
    {
        this.singlePlayerSelectedMatchPlayersIndex--;
        this.UpdateMatchPlayersText();
    }

    private void UpdateMatchPlayersText()
    {
        this.singlePlayerSelectedMatchPlayersIndex = Mathf.Clamp(this.singlePlayerSelectedMatchPlayersIndex, 0, this.singlePlayerMatchPlayersDefs.Count - 1);
        this.singlePlayerMatchPlayersText.text = this.singlePlayerMatchPlayersDefs[this.singlePlayerSelectedMatchPlayersIndex].Text;
    }

    public void DoSinglePlayerBack()
    {
        this.ShowMainPanel();
    }

    public void DoSinglePlayerPlay()
    {
        GameManager.Instance.GameOptions.MatchType = GameManager.GameOptionsMatchType.SinglePlayer;
        GameManager.Instance.GameOptions.SinglePlayer.MatchTimeInMinutes = this.singlePlayerMatchTimeDefs[this.singlePlayerSelectedMatchTimeIndex].Minutes;
        GameManager.Instance.GameOptions.SinglePlayer.MatchPlayersCode = this.singlePlayerMatchPlayersDefs[this.singlePlayerSelectedMatchPlayersIndex].Code;
        GameManager.Instance.PlaySinglePlayer();
    }

    public void DoMultiplayerBack()
    {
        this.ShowMainPanel();
    }

    private void ReadAudioMixerVolumes()
    {
        this.audioMixer.GetFloat("MasterMusicVolume", out float musicVolumeRaw);
        float musicVolume = Mathf.Pow(10.0f, musicVolumeRaw / 20.0f);
        this.optionsMusicVolume = (int)(musicVolume * 100f);

        this.audioMixer.GetFloat("MasterSfxVolume", out float soundVolumeRaw);
        float sfxVolume = Mathf.Pow(10.0f, soundVolumeRaw / 20.0f);
        this.optionsSfxVolume = (int)(sfxVolume * 100f);
    }

    public void DoOptionsMusicVolumeIncrease()
    {
        this.optionsMusicVolume += 5;
        this.UpdateOptionsMusicVolumeText();
    }

    public void DoOptionsMusicVolumeDecrease()
    {
        this.optionsMusicVolume -= 5;
        this.UpdateOptionsMusicVolumeText();
    }

    private void UpdateOptionsMusicVolumeText()
    {
        this.optionsMusicVolume = 5 * (this.optionsMusicVolume / 5);
        this.optionsMusicVolume = Mathf.Clamp(this.optionsMusicVolume, 0, 100);
        int activeCount = (this.optionsMusicVolume * 20) / 100;
        int disabledCount = 20 - activeCount;
        this.optionsMusicVolumeText.text = "<color=#FFF>" + new string('|', activeCount) + "</color>" + "<color=#555>" + new string('|', disabledCount) + "</color>";

        float newMusicVolume = Mathf.Clamp(Mathf.Log10(this.optionsMusicVolume / 100f) * 20f, -80f, 0f);
        this.audioMixer.SetFloat("MasterMusicVolume", newMusicVolume);
    }

    public void DoOptionsSfxVolumeIncrease()
    {
        this.optionsSfxVolume += 5;
        this.UpdateOptionsSfxVolumeText();
    }

    public void DoOptionsSfxVolumeDecrease()
    {
        this.optionsSfxVolume -= 5;
        this.UpdateOptionsSfxVolumeText();
    }

    private void UpdateOptionsSfxVolumeText()
    {
        this.optionsSfxVolume = 5 * (this.optionsSfxVolume / 5);
        this.optionsSfxVolume = Mathf.Clamp(this.optionsSfxVolume, 0, 100);
        int activeCount = (this.optionsSfxVolume * 20) / 100;
        int disabledCount = 20 - activeCount;
        this.optionsSfxVolumeText.text = "<color=#FFF>" + new string('|', activeCount) + "</color>" + "<color=#555>" + new string('|', disabledCount) + "</color>";

        float newSfxVolume = Mathf.Clamp(Mathf.Log10(this.optionsSfxVolume / 100f) * 20f, -80f, 0f);
        this.audioMixer.SetFloat("MasterSfxVolume", newSfxVolume);
    }

    public void DoOptionsBack()
    {
        this.ShowMainPanel();
    }

}
