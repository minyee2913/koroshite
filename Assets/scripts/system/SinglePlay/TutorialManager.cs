using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] List<SpriteRenderer> maps;
    [SerializeField] Player player;
    [SerializeField] Text info;
    [SerializeField] GameObject battle1;
    [SerializeField] GameObject whiteBack;
    [SerializeField] SpriteRenderer master;
    [SerializeField] GameObject skip;

    public string action;
    Vector2 savePoint;
    void Start()
    {
        StartCoroutine(starting());
    }

    public void Skip() {
        LobbyTutorial.afterTuto = false;
        LoadingController.LoadScene("Lobby");
    }

    IEnumerator starting() {
        foreach (SpriteRenderer render in maps) {
            render.color = Color.black;
        }

        player.preventInput = 100000;
        player.ForceWhite = true;

        skip.SetActive(true);

        player.hprate.gameObject.SetActive(false);

        yield return new WaitForSeconds(3);

        player._setBalloon("여긴 어디지..?");

        yield return new WaitForSeconds(3.5f);

        player._setBalloon("아무것도 안 보여");

        yield return new WaitForSeconds(3);

        foreach (SpriteRenderer render in maps) {
            yield return new WaitForSeconds(0.5f);

            SoundManager.Instance.Play("explosion_1");
            render.color = Color.white;
            CamManager.main.Shake(10, 0.2f);
        }

        yield return new WaitForSeconds(1);

        player._setBalloon("어..?");

        yield return new WaitForSeconds(3f);

        SoundManager.Instance.Play("select");

        info.text = "[A] [D]를 눌러 좌우로 이동할 수 있습니다.";

        player.preventInput = 0;

        action = "watingJumpPlace";
    }

    IEnumerator explainConcept() {
        player.preventInput = 100000;
        player.ch.render.flipX = true;

        yield return new WaitForSeconds(2);

        SoundManager.Instance.Play("explosion_1");
        
        whiteBack.SetActive(true);

        player.ForceWhite = false;
        player.ch.render.color = Color.black;

        UIManager.Instance.state.SetActive(false);
        player.hprate.gameObject.SetActive(false);
        info.text = "";

        yield return new WaitForSeconds(2f);

        master.gameObject.SetActive(true);
        master.transform.position = new Vector3(player.transform.position.x - 2, player.transform.position.y + 2);

        yield return new WaitForSeconds(2f);

        DialogueController.Instance.SetChColor(Color.black);
        DialogueController.Init("#$X#", "환영합니다. 안타깝게 끝나버린 영웅님.", master.sprite, DialogueDirection.Left).
            Add("#$X#", "저는 이곳 '시현'의 관리자 #$X#입니다.", master.sprite, DialogueDirection.Left).
            Add("samurai", "'시현'..?", player.ch.render.sprite, DialogueDirection.Right, 0.5f).
            Add("#$X#", "당신은 한 시대의 영웅으로써 사명을 실현하기 위해 힘썼지만", master.sprite, DialogueDirection.Left).
            Add("#$X#", "결국 뜻을 이루지 못하고 처참하게 생을 마감하였고", master.sprite, DialogueDirection.Left).
            Add("#$X#", "운 좋게 선택받아 이곳 '시현'에 오게 되었습니다.", master.sprite, DialogueDirection.Left).
            Add("samurai", "...", player.ch.render.sprite, DialogueDirection.Right).
            Add("#$X#", "이곳은 당신에게 다시 사명을 이룰 기회를 드릴겁니다.", master.sprite, DialogueDirection.Left).
            Add("#$X#", "선택은 본인의 몫이니...", master.sprite, DialogueDirection.Left).
            Add("#$X#", "쥐고 싶다면 <color=\"red\">끝없이 싸우세요</color>", master.sprite, DialogueDirection.Left, 0).
            Add("samurai", "...", player.ch.render.sprite, DialogueDirection.Right).
            Add("samurai", "왜 싸워야하죠?", player.ch.render.sprite, DialogueDirection.Right, 0.5f).
            Add("#$X#", "...", master.sprite, DialogueDirection.Left).
            Show();

        while (DialogueController.Instance.opened) {
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        whiteBack.SetActive(false);

        foreach (SpriteRenderer render in maps) {
            render.color = Color.black;
        }

        yield return new WaitForSeconds(1f);

        LobbyTutorial.afterTuto = false;
        LoadingController.LoadScene("Lobby");
    }
    
    void Update()
    {
        Vector2 detectPos = new Vector2(player.transform.position.x, 0);
        if (action == "watingJumpPlace") {
            if (Vector2.Distance(detectPos, new Vector2(15, 0)) <= 1.5f) {
                SoundManager.Instance.Play("select");
                info.text = "[SPACE]를 2번 눌러 높이 점프하세요!";

                CamManager.main.Shake();
                player.preventInput = 0.5f;

                action = "watingJump";
            }
        } else if (action == "watingJump") {
            if (Vector2.Distance(detectPos, new Vector2(23, 0)) <= 5f && player.OnGround()) {
                SoundManager.Instance.Play("select");
                info.text = "[SHIFT]를 눌러 이동 방향으로 대쉬할 수 있습니다.";

                CamManager.main.Shake();
                player.preventInput = 0.5f;

                action = "training_dash";
            }
        } else if (action == "training_dash") {
            if (Vector2.Distance(detectPos, new Vector2(80, 0)) <= 3f) {
                battle1.SetActive(true);

                CamManager.main.Shake(5, 0.2f);
                player.preventInput = 0.5f;

                player.hprate.gameObject.SetActive(true);
                UIManager.Instance.state.SetActive(true);
                UIManager.Instance.face.gameObject.SetActive(false);

                UIManager.Instance.state.transform.localScale = Vector2.one;
                UIManager.Instance.state.transform.DOScale(new Vector3(2, 2), 0.2f).SetEase(Ease.OutCubic);

                action = "waiting_kill";
            }
        } else if (action == "waiting_kill") {
            SoundManager.Instance.Play("select");
            info.text = "[J] 로 공격하여 모든 적들을 처치하세요!\n(" + (3 - Monster.monsters.Count).ToString() + "/3)";

            if (Monster.monsters.Count <= 0) {
                action = "";
                StartCoroutine(explainConcept());
            }
        }

        if (player.OnGround()) {
            savePoint = player.transform.position;
        } else {
            if (player.transform.position.y < savePoint.y - 10) {
                player.transform.position = savePoint;
            }
        }
    }
}
