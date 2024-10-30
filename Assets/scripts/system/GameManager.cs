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
    Defense,
    Raid
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
    List<Player> removedPlayer = new();
    public float timer, updateTick;
    public int timeCash;
    public GameObject rankPanel;
    public TMP_Text rankData;
    [SerializeField] TMP_Text timerText;
    bool inSudden;
    float suddenTime;
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
    public void SetMode(GameMode m) {
        object[] obj = {
            (int)m,
        };
        pv.RPC("setMode", RpcTarget.All, obj);
    }
    [PunRPC]
    void setMode(int mod) {
        mode = (GameMode)mod;
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
        SetSudden(false);
        SetMode(mode);
        SoundManager.Instance.StopToAll(4);

        MapManager.Instance.SelectByMod(mode);

        int count = 0;
        foreach (Player pl in Player.players) {
            pl.SetState("ready");
            if (pl.isSpectator) continue;

            if (mode == GameMode.Dual) {
                if (count > 2) {
                    pl.SetSpectator(true);

                    continue;
                }

                count++;
            }
            
            pl.SetEnergy(0);
            pl.SetShield(0);

            pl.Heal(pl.maxHealth);
            pl.SetDeath(0);
            pl.SetKill(0);
            pl.SetCoin(0);

            pl.CallCancelF();
            pl.CallRevive();
        }

        SetTitle("<color=\"grey\">맵</color>\n<color=\"green\">" + MapManager.Instance.selectedMap.Name + "</color>");

        List<Vector2> poses = MapManager.Instance.selectedMap.spawnPos;
        var shuffled = poses.OrderBy( x => UnityEngine.Random.value ).ToList( );

        for (int i = 0; i < shuffled.Count; i++) {
            if (i < Player.players.Count) {
                if (Player.players[i].isSpectator) continue;

                Player.players[i].SetPos(shuffled[i]);
            }
        }

        yield return new WaitForSeconds(2f);

        for (int i = 5; i > 0; i--) {
            SetTitle(i.ToString());

            yield return new WaitForSeconds(1f);
        }

        SetTitle("<color=\"yellow\">GameStart!</color>");

        foreach (Player pl in Player.players) {
            pl.SetState("ingame");
        }

        inSudden = false;

        switch (mode) {
            case GameMode.BattleRoyal:
                StartRoyale();
                break;
            case GameMode.FreeAllKill:
                StartFAK();
                break;
            case GameMode.Dual:
                StartDual();
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

    void endDual() {
        StartCoroutine(edu());
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        var p = Player.players.Find((v)=>v.pv.Owner.UserId == otherPlayer.UserId);

        if (p != null) {
            Player.players.Remove(p);
        }
    }

    public override void OnLeftRoom()
    {
       ChatManager.Instance.SendComment("<color=\"yellow\">" + PhotonNetwork.NickName + "님이 게임을 떠났습니다.</color>");
    }

    IEnumerator edu(){
        var players = Player.players;
        players.Sort((a, b)=>b.kill - a.kill);

        SetTitle(players[0].name_ + " WIN!");

        yield return new WaitForSeconds(1.5f);

        SetTitle("");

        GameEnd();
    }

    void StartDual() {
        SetTimer(2 * 60);

        ChatManager.Instance.SendComment("<color=\"orange\">상대 플레이어를 처치하면 승리합니다! 최대체력이 50%% 증가합니다.</color>");

        foreach (Player pl in Player.players) {
            if (pl.isSpectator) {
                continue;
            }

            pl.SetMaxHp((int)(pl.maxHealth * 1.5f));
        }

        float soundCount = 3;

        int rd = UnityEngine.Random.Range(0, 100);

        if (rd <= 100 / soundCount) {
            SoundManager.Instance.PlayToAll("crashingOut");
        } else if (rd <= 200 / soundCount) {
            SoundManager.Instance.PlayToAll("meltdown");
        } else {
            SoundManager.Instance.PlayToAll("cavern");
        }
    }

    void StartFAK() {
        SetTimer(5 * 60);
        ChatManager.Instance.SendComment("<color=\"orange\">제한 시간 내에 가장 많은 플레이어를 처치하세요!</color>");
        ChatManager.Instance.SendComment("<color=\"white\">데스매치에서는 [F]플레이어 변경 사용 가능</color>");

        float soundCount = 3;

        int rd = UnityEngine.Random.Range(0, 100);

        if (rd <= 100 / soundCount) {
            SoundManager.Instance.PlayToAll("crashingOut");
        } else if (rd <= 200 / soundCount) {
            SoundManager.Instance.PlayToAll("meltdown");
        } else {
            SoundManager.Instance.PlayToAll("cavern");
        }
    }

    public void DisplayDamage(Vector2 pos, int damage, Color col = default) {
        var obj = Instantiate(damInfo, pos, quaternion.identity);

        obj.txt = damage.ToString();
        obj.color = col;
    }

    public void Spawn() {
        PhotonNetwork.Instantiate("Player", spawnPos, quaternion.identity);

        SoundManager.Instance.Play("super_spiffy");
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

        SoundManager.Instance.PlayToAll("super_spiffy");

        SetState("none");
    }

    public void ForcePhaseEnd() {
        pv.RpcSecure("phaseEnd", RpcTarget.All, true);
    }

    [PunRPC]
    void phaseEnd() {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }
        switch (mode) {
            case GameMode.BattleRoyal:
                break;
            case GameMode.FreeAllKill:
                pv.RpcSecure("FAKEnd", RpcTarget.All, true);
                break;
            case GameMode.Dual:
                endDual();
                break;
            default:
                break;
        }
    }
    [PunRPC]
    void FAKEnd() {
        StartCoroutine(fakEnd());
    }
    void SetSudden(bool bol) {
        pv.RpcSecure("setSudden", RpcTarget.All, true, new object[] { bol });
    }
    [PunRPC]
    void setSudden(bool bol) {
        inSudden = bol;
        suddenTime = 0;
    }

    public void ShowRank() {
        rankPanel.SetActive(true);
        rankData.text = "";

        var players = Player.players;
        players.Sort((a, b)=>a.death - b.death);
        players.Sort((a, b)=>b.kill - a.kill);

        int ri = 0;
        for (int i = 0; i < players.Count; i++) {
            var pl = players[i];

            if (pl.isSpectator) {
                continue;
            }

            rankData.text += (ri+1).ToString() + ". " + pl.GetName() + "    " + pl.kill + "킬  " + pl.death + "데스\n";

            ri++;
        }
    }

    public void HideRank() {
        rankPanel.SetActive(false);
    }

    IEnumerator fakEnd() {
        state = "ending";
        applyPlayerInput = false;

        title.text = "GAME OVER!";

        yield return new WaitForSeconds(1);

        title.text = "";

        ShowRank();

        yield return new WaitForSeconds(4f);

        applyPlayerInput = true;

        HideRank();

        if (PhotonNetwork.IsMasterClient) GameEnd();
    }

    void Update()
    {
        updateTick += Time.deltaTime;

        if (updateTick > 2) {
            foreach (Player player in Player.players) {
                if (player == null) {
                    removedPlayer.Add(player);
                }
            }

            foreach (Player player in removedPlayer) {
                Player.players.Remove(player);
            }
            removedPlayer.Clear();

            updateTick = 0;
        }

        if (Input.GetKeyDown(KeyCode.F5)) {
            CamManager.main.autoSpector = !CamManager.main.autoSpector;
        }

        if (Input.GetKeyDown(KeyCode.U) && !CharacterPanel.activeSelf) {
            if (SkillInfo.Instance.panel.activeSelf) {
                SkillInfo.Instance.Close();
            } else {
                SkillInfo.Instance.Open(Player.Local.ch);
            }
        }

        if (mode == GameMode.FreeAllKill) {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                ShowRank();
            }
            if (Input.GetKeyUp(KeyCode.Tab)) {
                HideRank();
            }

            if (state == "started") {
                if (inSudden) {
                    suddenTime += Time.deltaTime;
                    if (suddenTime > 1) {
                        Player.Local.SetEnergy(Player.Local.energy + 2);

                        suddenTime = 0;

                        Debug.Log("add");
                    }
                }

                if (PhotonNetwork.IsMasterClient) {
                    if (!inSudden && timer <= 150) {
                        inSudden = true;

                        StartCoroutine(GoSudden());
                    }
                }
            }
        }

        if (state == "none" || mode == GameMode.FreeAllKill) {
            if (Input.GetKeyDown(KeyCode.F) && !SkillInfo.Instance.panel.activeSelf) {
                CharacterPanel.SetActive(!CharacterPanel.activeSelf);
            }
        }
        if (state == "started") {
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

    IEnumerator GoSudden() {
        SetSudden(true);
        SetTitle("<color=\"red\">남은 시간이 얼마 없어!!</color>\n이제 에너지가 자동으로 채워집니다.");
        SoundManager.Instance.PlayToAll("sudden");

        yield return new WaitForSeconds(6f);

        SetTitle("");
        
    }
}
