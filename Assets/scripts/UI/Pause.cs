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

        pause_main.SetActive(true);
        if (SceneManager.GetActiveScene().name == "GameScene") {
            pause_game.SetActive(true);
            pause_setting.SetActive(false);

            UpdatePlayerList();
        } else {
            pause_setting.SetActive(true);
        }

        pause_game.transform.localScale = new Vector3(0.8f, 0.8f);
        pause_game.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic);
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

        pause_setting.transform.localScale = new Vector3(0.8f, 0.8f);
        pause_setting.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic);

        float masterVol = PlayerPrefs.GetFloat("MasterVolume");
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume");
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume");

        masterSlider.value = masterVol;
        bgmSlider.value = bgmVol;
        sfxSlider.value = sfxVol;
    }

    public void ClosePause() {
        inPause = false;

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
        PhotonNetwork.LeaveRoom();
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
