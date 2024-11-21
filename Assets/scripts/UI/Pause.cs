using System.Collections;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public static Pause Instance {get; private set;}
    public ListPlayer listPl;
    [SerializeField] GameObject pause_main;
    [SerializeField] Transform pause_playerlist;
    [SerializeField] GameObject pause_game;
    [SerializeField] GameObject pause_single;
    [SerializeField] GameObject pause_setting;
    [Header("settings")]
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;
    public bool inPause;
    void Start()
    {
        Instance = this;

        UpdateAudio();
    }

    public void UpdateAudio() {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume");
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume");
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume");

        if (masterVol == -40f) mixer.SetFloat("Master", -80);
        else mixer.SetFloat("Master", masterVol);

        if (bgmVol == -40f) mixer.SetFloat("SFX", -80);
        else mixer.SetFloat("SFX", bgmVol);

        if (sfxVol == -40f) mixer.SetFloat("BGM", -80);
        else mixer.SetFloat("BGM", sfxVol);
    }

    public void OpenPause() {
        inPause = true;

        GameObject targetPanel = pause_game;

        pause_main.SetActive(true);
        if (SceneManager.GetActiveScene().name == "GameScene") {
            pause_game.SetActive(true);
            pause_setting.SetActive(false);
            pause_single.SetActive(false);

            UpdatePlayerList();
        } else {
            if (Player.Local != null && Player.Local.state == "singlePlay") {
                targetPanel = pause_single;

                pause_single.SetActive(true);
                pause_setting.SetActive(false);
                pause_game.SetActive(false);
            } else {
                pause_setting.SetActive(true);
            }
        }

        targetPanel.transform.localScale = new Vector3(0.8f, 0.8f);
        targetPanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);

        Time.timeScale = 0;
    }

    public void UpdatePlayerList(Transform list = null) {
        if (list == null) {
            list = pause_playerlist;
        }
        foreach (Transform child in list) {
            Destroy(child.gameObject);
        }

        foreach (Player p in Player.players) {
            ListPlayer pl = Instantiate(listPl, list);
            pl.nametag.text = p.nametag.text;

            pl.profile.sprite = p.ch.icon;
            pl.userId = p.pv.Owner.UserId;
            pl.actorNumber = p.pv.Owner.ActorNumber;

            pl.crown.gameObject.SetActive(p.pv.Owner.IsMasterClient);

            if (p.isSpectator) {
                pl.nametag.text += " <color=\"grey\">[관전]</color>";
                pl.profile.color = Color.black;
            }

            if (p.blocked) {
                pl.blockTx.text = "차단해제";
            } else {
                pl.blockTx.text = "차단";
            }

            if (p != Player.Local) {
                pl.block.gameObject.SetActive(true);
                
                if (PhotonNetwork.IsMasterClient) {
                    pl.kick.gameObject.SetActive(true);
                    pl.giveMaster.gameObject.SetActive(true);
                } else {
                    pl.kick.gameObject.SetActive(false);
                    pl.giveMaster.gameObject.SetActive(false);
                }
            } else {
                pl.block.gameObject.SetActive(false);
                pl.kick.gameObject.SetActive(false);
                pl.giveMaster.gameObject.SetActive(false);
            }
        }
    }

    public void OpenSetting() {
        inPause = true;

        pause_main.SetActive(true);
        pause_game.SetActive(false);
        pause_setting.SetActive(true);
        pause_single.SetActive(false);

        pause_setting.transform.localScale = new Vector3(0.8f, 0.8f);
        pause_setting.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic).SetUpdate(true);

        float masterVol = PlayerPrefs.GetFloat("MasterVolume");
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume");
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume");

        masterSlider.value = masterVol;
        bgmSlider.value = bgmVol;
        sfxSlider.value = sfxVol;
    }

    public void ClosePause() {
        inPause = false;

        Time.timeScale = 1;

        pause_main.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (inPause) {
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    ClosePause();
                }
        } else {
            if (SceneManager.GetActiveScene().name == "GameScene") {
                if (UIManager.Instance.GsectionOpened == true
                    || ChatManager.Instance.inChat == true
                ) {
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                    OpenPause();
                }
        }
    }

    public void GameEnd() {
        ClosePause();

        GameManager.Instance.ForceEnd();
    }

    public void ExitRoom() {
        Time.timeScale = 1;
        PhotonNetwork.LeaveRoom();
    }

    public void GoToLobby() {
        Time.timeScale = 1;
        LoadingController.LoadScene("Lobby");
    }

    public void Replay() {
        Time.timeScale = 1;
        LoadingController.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame() {
        PhotonNetwork.Disconnect();
        Application.Quit();
    }

    public void SaveSetting() {
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);

        UpdateAudio();
    }
}
