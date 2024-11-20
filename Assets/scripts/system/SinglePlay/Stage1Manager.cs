using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Stage1Manager : MonoBehaviour
{
    [SerializeField] GameObject cover1, cover2, cover_all;
    bool started;
    [SerializeField]
    Text info, title;
    [SerializeField]
    GameObject npc, P1, P2;
    Cooldown changeCool = new(0.5f);

    string currentCharacter = "samurai";
    int p = 0;
    void Start()
    {
        cover_all.SetActive(true);
        cover1.SetActive(true);
        cover2.SetActive(true);

        StartCoroutine(starting());
    }

    IEnumerator starting() {
        Player.Local.preventInput = 100000000;
        yield return new WaitForSeconds(2);

        cover_all.SetActive(false);
        SoundManager.Instance.Play("explosion_1");
        yield return new WaitForSeconds(0.3f);

        cover1.SetActive(false);
        SoundManager.Instance.Play("select");
        yield return new WaitForSeconds(0.2f);

        cover2.SetActive(false);
        SoundManager.Instance.Play("select");

        CamManager.main.Shake(2, 0.2f);

        // var fighter = CharacterManager.instance.GetIcon("fighter");
        // var samurai = CharacterManager.instance.GetIcon("samurai");

        // DialogueController.Instance.SetChColor(Color.white);
        // DialogueController.Init("fighter", "너도 왔구나.", fighter, DialogueDirection.Left).
        //     Add("samurai", "여기는 뭔가요? 아까 그 사람들은요?", samurai, DialogueDirection.Right).
        //     Add("fighter", "이 공간 자체야.", fighter, DialogueDirection.Left).
        //     Add("samurai", "네..?", samurai, DialogueDirection.Right).
        //     Add("fighter", "그러니까 아까 그 사람들이 이 공간으로 변한거라고", fighter, DialogueDirection.Left).
        //     Add("samurai", "어째서요?", samurai, DialogueDirection.Right).
        //     Add("fighter", "너도 여기 왔다는거는 그 거지X끼가 한 말을 들었다는거잖아?", fighter, DialogueDirection.Left).
        //     Add("fighter", "여기는 늙지도 죽지도 않아. 하지만 싸우는 것을 멈추고 전의를 상실하면...", fighter, DialogueDirection.Left).
        //     Add("fighter", "시현에 빨려들어가서 공간의 일부가 되는거지.", fighter, DialogueDirection.Left).
        //     Add("fighter", "갑자기 땅이나 하늘이 보이는 것도 이 공간이 된 그 녀석들의 기억에 의한거야.", fighter, DialogueDirection.Left).
        //     Add("samurai", "왜 이런 규칙이 있는거죠?", samurai, DialogueDirection.Right).
        //     Add("fighter", "그야 나도 모르지. 아마 신꺠서는 우리를 그저 장난감 정도로 생각하고 있을지도 몰라.", fighter, DialogueDirection.Left).
        //     Add("samurai", "저는 앞으로 여기서 뭘 하면 되는거죠?", samurai, DialogueDirection.Right).
        //     Add("fighter", "싸워", fighter, DialogueDirection.Left, 0.5f).
        //     Add("fighter", "이런 공간은 한번 생기면 사라지지 않아. 몇번이고 다시 나타나지", fighter, DialogueDirection.Left).
        //     Add("fighter", "나타나면 가서 싸우고. 안 나타나면 시현에 있는 사람들끼리 싸우면 되니까", fighter, DialogueDirection.Left).
        //     Show();

        // while (DialogueController.Instance.opened) {
        //     yield return null;
        // }

        yield return new WaitForSeconds(1);
        Player.Local.preventInput = 0;

        started = true;
    }

    IEnumerator p1() {
        Player.Local.preventInput = 100000;
        started = false;

        var fighter = CharacterManager.instance.GetIcon("fighter");
        var samurai = CharacterManager.instance.GetIcon("samurai");
        DialogueController.Init("fighter", "너도 왔구나.", fighter, DialogueDirection.Left).
            Add("samurai", "여기는 뭔가요? 아까 그 사람들은요?", samurai, DialogueDirection.Right).
            Add("fighter", "이 공간 자체야.", fighter, DialogueDirection.Left).
            Add("samurai", "네..?", samurai, DialogueDirection.Right).
            Add("fighter", "그러니까 아까 그 사람들이 이 공간으로 변한거라고", fighter, DialogueDirection.Left).
            Add("samurai", "어째서요?", samurai, DialogueDirection.Right).
            Add("fighter", "너도 여기 왔다는거는 그 거지X끼가 한 말을 들었다는거잖아?", fighter, DialogueDirection.Left).
            Show();

        while (DialogueController.Instance.opened) {
            yield return null;
        }

        Player.Local.preventInput = 0;

        npc.SetActive(false);

        Vector2 titleScale = title.transform.localScale;
        title.text = "fighter가 전투에 합류했습니다!\n<color=\"grey\">[F]를 눌러서 스위치</color>";
        title.transform.localScale = titleScale * 0.8f;
        title.transform.DOScale(titleScale, 0.5f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(1.5f);

        title.text = "";

        P1.SetActive(true);

        started = true;
    }

    IEnumerator p2() {
        Player.Local.preventInput = 100000;
        started = false;

        yield return new WaitForSeconds(1f);

        var fighter = CharacterManager.instance.GetIcon("fighter");
        var samurai = CharacterManager.instance.GetIcon("samurai");
        DialogueController.Init("fighter", "여기는 늙지도 죽지도 않아. 하지만 싸우는 것을 멈추고 전의를 상실하면...", fighter, DialogueDirection.Left).
            Add("fighter", "시현에 빨려들어가서 공간의 일부가 되는거지.", fighter, DialogueDirection.Left).
            Add("fighter", "갑자기 땅이나 하늘이 보이는 것도 이 공간이 된 그 녀석들의 기억에 의한거야.", fighter, DialogueDirection.Left).
            Add("samurai", "왜 이런 규칙이 있는거죠?", samurai, DialogueDirection.Right).
            Add("fighter", "그야 나도 모르지. 아마 신꺠서는 우리를 그저 장난감 정도로 생각하고 있을지도 몰라.", fighter, DialogueDirection.Left).
            Add("samurai", "저는 앞으로 여기서 뭘 하면 되는거죠?", samurai, DialogueDirection.Right).
            Add("fighter", "싸워", fighter, DialogueDirection.Left).
            Show();

        while (DialogueController.Instance.opened) {
            yield return null;
        }

        Player.Local.preventInput = 0;

        P2.SetActive(true);

        started = true;
    }

    IEnumerator p3() {
        Player.Local.preventInput = 100000;

        yield return new WaitForSeconds(1f);

        var fighter = CharacterManager.instance.GetIcon("fighter");
        DialogueController.Init("fighter", "이런 공간은 한번 생기면 사라지지 않아. 몇번이고 다시 나타나지", fighter, DialogueDirection.Left).
            Add("fighter", "나타나면 가서 싸우고. 안 나타나면 시현에 있는 사람들끼리 싸우면 되니까", fighter, DialogueDirection.Left).
            Show();

        while (DialogueController.Instance.opened) {
            yield return null;
        }

        yield return new WaitForSeconds(1);

        Player.Local.preventInput = 0;

        PlayFabManager.SetTutorialEnded();
        LobbyTutorial.afterTuto = true;

        LoadingController.LoadScene("Lobby");
    }

    void Update() {
        if (started) {
            info.text = "처치해야할 적 : " + Monster.monsters.Count;
            
            if (p == 0) {
                npc.SetActive(Monster.monsters.Count <= 0);
                if (Monster.monsters.Count <= 0 && Mathf.Abs(npc.transform.position.x - Player.Local.transform.position.x) <= 2 && Player.Local.preventInput <= 0) {
                    npc.SetActive(true);

                    p = 1;

                    Debug.Log("p1");

                    StartCoroutine(p1());
                }
                
            }
            else if (p == 1) {
                if (Monster.monsters.Count <= 0 && Player.Local.preventInput <= 0) {
                    p = 2;

                    StartCoroutine(p2());
                }
                
            }
            else if (p == 2) {
                if (Monster.monsters.Count <= 0 && Player.Local.preventInput <= 0) {
                    p = 3;

                    StartCoroutine(p3());
                }
                
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && !changeCool.IsIn() && p >= 1) {
            changeCool.Start();
            if (currentCharacter == "samurai") {
                currentCharacter = "fighter";
            } else {
                currentCharacter = "samurai";
            }

            Player.Local.SetCharacter(currentCharacter);
            Player.Local.ch.OnSwitch();
        }
    }
}
