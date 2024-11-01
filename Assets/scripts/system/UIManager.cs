using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    GameObject roomUI;
    [SerializeField]
    TMP_Text coin;
    [SerializeField]
    Slider hp;
    [SerializeField]
    Slider energy;
    [SerializeField]
    GameObject deathPanel;
    [SerializeField]
    TMP_Text deathTitle;
    [SerializeField]
    TMP_Text deathCount;
    public Image face;
    public Text sectionTitle;
    [SerializeField] Button gameStart;
    [SerializeField] TMP_Text killDeath;
    [SerializeField] Button spectorBtn;
    [SerializeField] TMP_Text spectortxt;
    [SerializeField] SpectatorIcon spectatorIcon;
    [SerializeField] Transform spectatorGroup;
    [SerializeField] GameObject state;
    [SerializeField] TMP_Text inAuto;
    public Dictionary<Player, SpectatorIcon> specs = new();
    float dCount = 0, specWait, specAuto, specAutoWait;
    List<Player> autoPlayers = new();
    Player RandPl;
    public Transform listSection;
    public ListPlayer listPl;
    public GameObject Gsection, Gsection_animate;
    public bool GsectionOpened;
    public GameObject beforeCOnn, COnnCover;
    public Text COnnText, mapText;
    public GameObject detail;
    public Slider timerRate;

    public static UIManager Instance {get; private set;}
    void Awake() {
        Instance = this;

        LoadingAnim();
    }

    public void ChangeSpectator() {
        Player.Local.SetSpectator(!Player.Local.isSpectator);
    }

    public void OpenGameSection() {
        GsectionOpened = true;

        Gsection.SetActive(true);
        Gsection_animate.transform.localScale = new Vector3(0.8f, 0.8f);
        Gsection_animate.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic);

        UpdatePlayerList();
    }

    public void CloseGameSection() {
        GsectionOpened = false;

        Gsection.SetActive(false);
    }

    public void LoadingAnim() {
        StartCoroutine(loadingAnim());
    }

    IEnumerator loadingAnim() {
        beforeCOnn.SetActive(true);
        COnnCover.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        COnnCover.SetActive(false);
        COnnText.text = "";

        yield return new WaitForSeconds(0.5f);

        string msg = "좀 더 기다려.";

        for (int i = 0; i < msg.Length; i++) {
            COnnText.text += msg[i];

            if (msg[i] == ' ') {
                yield return new WaitForSeconds(0.2f);
            } else {
                yield return new WaitForSeconds(0.1f);
            }
        }

        while (!GameManager.Instance.firstSpawn) {
            beforeCOnn.transform.localPosition = new Vector3(Random.Range(-8f, 8f), Random.Range(-8f, 8f));
            yield return null;
        }

        beforeCOnn.SetActive(false);
    }

    public void UpdatePlayerList(Transform list = null) {
        if (list == null) {
            list = listSection;
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

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (GsectionOpened) {
            UpdatePlayerList();
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (GsectionOpened) {
            UpdatePlayerList();
        }
    }

    void Update() {
        if (GsectionOpened) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                CloseGameSection();
            }

            if (MapManager.Instance.selectedMap != null) {
                mapText.text = MapManager.Instance.selectedMap.Name;
            }

            if (Player.Local.state != "room") {
                CloseGameSection();
            }
        }
    }

    void FixedUpdate() {
        if (Player.Local != null) {
            detail.SetActive(Player.Local.state == "room");
            spectatorGroup.gameObject.SetActive(Player.Local.isSpectator && Player.Local.state != "room");
            if (Player.Local.isSpectator) {
                spectortxt.text = "관전취소";
                state.SetActive(false);

                if (CamManager.main.autoSpector) {
                    inAuto.color = Color.cyan;

                    specAuto += Time.fixedDeltaTime;

                    if (specAuto > specAutoWait) {
                        specAutoWait = Random.Range(5, 8);
                        specAuto = 0;

                        autoPlayers.Clear();

                        autoPlayers = Player.players.FindAll((p)=>!p.isSpectator && p != RandPl);

                        CamManager.main.spectatorTarget = autoPlayers[Mathf.RoundToInt(Random.Range(0f, autoPlayers.Count-1))];
                    }
                } else {
                    inAuto.color = Color.white;
                }

                specWait += Time.fixedDeltaTime;

                if (specWait > 5) {
                    UpdateSpectator();

                    specWait = 0;
                }
            } else {
                spectortxt.text = "관전하기";
                state.SetActive(true);
            }

            roomUI.SetActive(Player.Local.state == "room");

            if (Player.Local.state == "room") {
                sectionTitle.text = "대기중 - " + PhotonNetwork.CurrentRoom.Name + "\n(" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";
            }

            if (GameManager.Instance.mode == GameMode.FreeAllKill) {
                if (Player.Local.state == "ingame") {
                    sectionTitle.text = "데스매치 - " + PhotonNetwork.CurrentRoom.Name + "\n(" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";
                }
            } else if (GameManager.Instance.mode == GameMode.BattleRoyal) {
                if (Player.Local.state == "ingame") {
                    sectionTitle.text = "파밍 라운드 - " + PhotonNetwork.CurrentRoom.Name + "\n(" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";
                }
            }

            hp.value = (float)Player.Local.health / Player.Local.maxHealth;
            energy.value = (float)Player.Local.energy / 100;
            coin.text = Player.Local.coin.ToString() + "G";

            if (Player.Local.isDeath) {
                if (Player.Local.state == "ingame") {
                    dCount += Time.deltaTime;

                    deathCount.text = "부활까지...\n" + (5 - (int)dCount).ToString();

                    if (dCount > 5) {
                        dCount = 0;

                        Player.Local.Revive();

                        Vector2 pos = GameManager.Instance.spawnPoses[Random.Range(0, GameManager.Instance.spawnPoses.Count - 1)];

                        Player.Local.SetPos(pos);

                        deathPanel.SetActive(false);
                    }
                } else {
                    Player.Local.Revive();
                    deathPanel.SetActive(false);
                }
            }

            killDeath.text = Player.Local.kill.ToString() + "/" + Player.Local.death.ToString();
        }
    }

    public void UpdateSpectator() {
        foreach (Player pl in Player.players) {
            if (!specs.ContainsKey(pl) && !pl.isSpectator) {
                var icn = Instantiate(spectatorIcon, spectatorGroup);

                icn.nameTag.text = pl.name_;
                icn.target = pl;
                
                if (pl.ch != null) {
                    icn.icon.sprite = pl.ch.icon;
                }

                specs[pl] = icn;
            }
        }

        foreach (KeyValuePair<Player, SpectatorIcon> v in specs) {
            if (!Player.players.Contains(v.Key) || v.Key.isSpectator) {
                specs.Remove(v.Key);
                Destroy(v.Value.gameObject);
            }
        }

        if (CamManager.main.spectatorTarget == null && specs.Count > 0) {
            CamManager.main.spectatorTarget = specs.First().Key;
        }
    }

    public void ShowDeathScreen() {
        deathPanel.SetActive(true);

        StartCoroutine(deathScreenAnim());
    }

    IEnumerator deathScreenAnim() {
        Color col = deathTitle.color;
        deathTitle.transform.localPosition = new Vector3(0, 352);
        deathTitle.transform.DOLocalMove(new Vector3(0, 422), 1f);

        for (int i = 0; i <= 20; i++) {
            col.a = i * 0.05f;

            deathTitle.color = col;

            yield return new WaitForSeconds(0.05f);
        }
    }
}
