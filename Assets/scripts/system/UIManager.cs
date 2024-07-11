using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
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
    public TMP_Text sectionTitle;
    [SerializeField] Button gameStart;
    [SerializeField] TMP_Text killDeath;
    float dCount = 0;

    public static UIManager Instance {get; private set;}
    void Awake() {
        Instance = this;
    }

    void Update() {
        if (Player.Local != null) {
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

            gameStart.gameObject.SetActive(PhotonNetwork.IsMasterClient && GameManager.Instance.state == "none");

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
