using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public enum GameMode {
    BattleRoyal,
    FreeAllKill,
    Dual,
}
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    PhotonView pv;
    public GameObject CharacterPanel;

    public string state = "none";
    public GameMode mode = GameMode.FreeAllKill;
    public bool applyPlayerInput;
    public List<Vector2> spawnPoses;
    public TMP_Text title;
    public readonly Vector3 spawnPos = new(-55.73f, -19.55f);
    [SerializeField] DamageInfo damInfo;
    public float timer;
    public int timeCash;
    public GameObject rankPanel;
    public TMP_Text rankData;
    [SerializeField] TMP_Text timerText;
    void Start()
    {
        pv = GetComponent<PhotonView>();
        CharacterPanel.SetActive(true);

        Instance = this;

        applyPlayerInput = true;

        if (PhotonNetwork.InLobby) {
            PhotonNetwork.JoinRoom(NetworkManager.instance.savedRoom);
        }
    }

    public void StartGame() {
        StartCoroutine(stgm());
    }
    public void SetTitle(string str) {
        object[] obj = {
            str,
        };
        pv.RPC("setTitle", RpcTarget.All, obj);
    }
    [PunRPC]
    void setTitle(string str) {
        title.text = str;
    }

    public void SetState(string str) {
        object[] obj = {
            str,
        };
        pv.RPC("setState", RpcTarget.All, obj);
    }
    [PunRPC]
    void setState(string st) {
        state = st;
    }
    public void SetTimer(float time) {
        object[] obj = {
            time,
        };
        pv.RPC("setTimer", RpcTarget.All, obj);
    }
    [PunRPC]
    void setTimer(float time) {
        timer = time;
        timeCash = (int)time - 60;
    }

    IEnumerator stgm() {
        SetState("starting");
        foreach (Player pl in Player.players) {
            pl.SetState("ready");
            pl.SetEnergy(0);

            pl.Heal(pl.maxHealth);
            pl.SetDeath(0);
            pl.SetKill(0);
            pl.SetCoin(0);
        }

        List<Vector2> poses = spawnPoses;
        var shuffled = poses.OrderBy( x => UnityEngine.Random.value ).ToList( );

        for (int i = 0; i < shuffled.Count; i++) {
            if (i < Player.players.Count) {
                Player.players[i].SetPos(shuffled[i]);
            }
        }

        yield return new WaitForSeconds(2f);

        for (int i = 5; i > 0; i--) {
            SetTitle(i.ToString());

            yield return new WaitForSeconds(1f);
        }

        SetTitle("<color=\"green\">GameStart!</color>");

        foreach (Player pl in Player.players) {
            pl.SetState("ingame");
        }

        switch (mode) {
            case GameMode.BattleRoyal:
                StartRoyale();
                break;
            case GameMode.FreeAllKill:
                StartFAK();
                break;
            case GameMode.Dual:
                break;
            default:
                break;
        }

        SetState("started");

        yield return new WaitForSeconds(1.5f);

        SetTitle("");

        yield break;
    }

    void StartRoyale() {
        ChatManager.Instance.SendComment("<color=\"orange\">준비 단계가 시작되었습니다. 다른 참가자들을 처치하면 금화를 획득합니다.</color>");
    }

    void StartFAK() {
        SetTimer(8 * 60);
        ChatManager.Instance.SendComment("<color=\"orange\">제한 시간 내에 가장 많은 플레이어를 처치하세요!</color>");
    }

    public void DisplayDamage(Vector2 pos, int damage, Color col = default) {
        var obj = Instantiate(damInfo, pos, quaternion.identity);

        obj.txt = damage.ToString();
        obj.color = col;
    }

    public void Spawn() {
        PhotonNetwork.Instantiate("Player", spawnPos, quaternion.identity);
    }

    public void ForceEnd() {
        if (PhotonNetwork.IsMasterClient) {
            GameEnd();
        }
    }

    public void GameEnd() {
        foreach (Player pl in Player.players) {
            pl.SetPos(spawnPos);

            pl.SetState("room");

            pl.Heal(pl.maxHealth);
            pl.SetDeath(0);
            pl.SetKill(0);
            pl.SetCoin(0);
        }

        SetState("none");
    }

    void phaseEnd() {
        switch (mode) {
            case GameMode.BattleRoyal:
                break;
            case GameMode.FreeAllKill:
                pv.RpcSecure("FAKEnd", RpcTarget.All, true);
                break;
            case GameMode.Dual:
                break;
            default:
                break;
        }
    }
    [PunRPC]
    void FAKEnd() {
        StartCoroutine(fakEnd());
    }

    IEnumerator fakEnd() {
        state = "ending";
        applyPlayerInput = false;

        title.text = "GAME OVER!";

        yield return new WaitForSeconds(1);

        title.text = "";

        rankPanel.SetActive(true);
        rankData.text = "";

        var players = Player.players;
        players.Sort((a, b)=>a.death - b.death);
        players.Sort((a, b)=>b.kill - a.kill);

        for (int i = 0; i < players.Count; i++) {
            var pl = players[i];

            rankData.text += (i+1).ToString() + ". " + pl.GetName() + "    " + pl.kill + "킬  " + pl.death + "데스\n";
        }

        yield return new WaitForSeconds(4f);

        applyPlayerInput = true;

        rankPanel.SetActive(false);

        if (PhotonNetwork.IsMasterClient) GameEnd();
    }

    void Update()
    {
        if (state == "none") {
            if (Input.GetKeyDown(KeyCode.F) && !SkillInfo.Instance.panel.activeSelf) {
                CharacterPanel.SetActive(!CharacterPanel.activeSelf);
            }
            if (Input.GetKeyDown(KeyCode.U) && !CharacterPanel.activeSelf) {
                if (SkillInfo.Instance.panel.activeSelf) {
                    SkillInfo.Instance.Close();
                } else {
                    SkillInfo.Instance.Open(Player.Local.ch);
                }
            }
        } else if (state == "started") {
            if (timer > 0) {
                timer -= Time.deltaTime;
            }

            if (timer < timeCash) {
                SetTimer(timer);
            }

            if (PhotonNetwork.IsMasterClient) {
                if (timer <= 0) {
                    phaseEnd();
                }


            }
        }

        timerText.text = ((timer / 60 < 10) ? "0" + ((int)(timer / 60)) : ((int)(timer / 60)).ToString()) + ":" + ((timer % 60 < 10) ? "0" + ((int)timer % 60) : ((int)timer % 60).ToString());
    }
}
