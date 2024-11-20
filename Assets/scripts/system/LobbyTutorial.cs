using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LobbyTutorial : MonoBehaviour
{
    [SerializeField] GameObject afterTutorial, tuto;
    [SerializeField] List<GameObject> mobu;
    [SerializeField] GameObject samurai, fighter;
    [SerializeField] GlobalBalloon balloon;
    [SerializeField] GameObject curse1, curse2, curse3;
    [SerializeField] GameObject skip;
    Samurai samuch;
    Fighter fightch;
    public static bool afterTuto, storyTuto;
    public bool force;
    void Awake()
    {
        if (force == true) {
            afterTuto = true;
            force = false;
        }
        if (storyTuto) {
            afterTuto = false;
            storyTuto = false;
        }
        if (PlayFabManager.tutorialEnded || afterTuto) {
            afterTutorial.SetActive(true);
            tuto.SetActive(false);
        } else {
            afterTutorial.SetActive(false);
            tuto.SetActive(true);

            StartCoroutine(starting());
        }
    }

    public void Skip() {
        if (Input.GetKey(KeyCode.LeftShift)) {
            afterTuto = true;
            LoadingController.LoadScene("Lobby");
        } else {
            LoadingController.LoadScene("StageTuto-1");
        }
    }

    IEnumerator starting() {
        tuto.SetActive(true);
        samurai.transform.position = new Vector3(-10, -3.23f);
        samuch = samurai.GetComponentInChildren<Samurai>();
        fightch = fighter.GetComponentInChildren<Fighter>();
        yield return new WaitForSeconds(1);

        skip.SetActive(true);

        samurai.transform.DOMoveX(-7, 1.5f);
        samuch.animator.SetBool("isMoving", true);

        yield return new WaitForSeconds(1.5f);
        samuch.animator.SetBool("isMoving", false);

        yield return new WaitForSeconds(1f);

        balloon.ShowBalloon("사람인가?", samurai.transform.position + new Vector3(0, 2.5f), 1.5f);

        yield return new WaitForSeconds(2f);

        balloon.ShowBalloon("못해... 못하겠어", mobu[0].transform.position + new Vector3(0, 1f), 1.5f);

        yield return new WaitForSeconds(1.5f);

        balloon.ShowBalloon("사무직인 나한테 왜 전투를 시키는거야..!", mobu[2].transform.position + new Vector3(0, 1f), 1.5f);

        yield return new WaitForSeconds(1.5f);

        balloon.ShowBalloon("그만하고 싶어...", mobu[1].transform.position + new Vector3(0, 1f), 1.5f);

        yield return new WaitForSeconds(2f);

        fighter.transform.DOMoveX(5, 4f);
        fightch.animator.SetBool("isMoving", true);

        yield return new WaitForSeconds(4f);
        fightch.animator.SetBool("isMoving", false);

        balloon.ShowBalloon("너희들", fighter.transform.position + new Vector3(0, 2.5f), 2f);
        yield return new WaitForSeconds(2f);
        balloon.ShowBalloon("마지막으로 한번만 충고해주지.", fighter.transform.position + new Vector3(0, 2.5f), 2f);
        yield return new WaitForSeconds(2f);
        balloon.ShowBalloon("이곳은 아무 규칙도 속박도 없지만", fighter.transform.position + new Vector3(0, 2.5f), 2f);
        yield return new WaitForSeconds(2f);
        balloon.ShowBalloon("싸우지 않는 것 만큼은 크게 후회하게 될거야", fighter.transform.position + new Vector3(0, 2.5f), 2f);

        yield return new WaitForSeconds(2f);

        balloon.ShowBalloon("...", mobu[0].transform.position + new Vector3(0, 1f), 1.5f);

        yield return new WaitForSeconds(2f);

        balloon.ShowBalloon("말이 안통하는군", fighter.transform.position + new Vector3(0, 2.5f), 1.5f);

        yield return new WaitForSeconds(1.5f);

        fighter.transform.DOMoveX(-5, 6f);
        fightch.animator.SetBool("isMoving", true);

        yield return new WaitForSeconds(6f);
        fightch.animator.SetBool("isMoving", false);

        yield return new WaitForSeconds(1.5f);

        balloon.ShowBalloon("너는...", fighter.transform.position + new Vector3(0, 2.5f), 2f);
        yield return new WaitForSeconds(2f);
        balloon.ShowBalloon("신입인가?", fighter.transform.position + new Vector3(0, 2.5f), 2f);
        yield return new WaitForSeconds(2.5f);
        balloon.ShowBalloon("네.. 뭐 그런 샘이죠", samurai.transform.position + new Vector3(0, 2.5f), 2f);
        yield return new WaitForSeconds(2.5f);
        balloon.ShowBalloon("그럼 지금부터 저들을 잘 봐둬", fighter.transform.position + new Vector3(0, 2.5f), 2f);
        yield return new WaitForSeconds(2f);
        balloon.ShowBalloon("그리고...", fighter.transform.position + new Vector3(0, 2.5f), 3f);
        yield return new WaitForSeconds(3f);

        fightch.render.flipX = false;
        CamManager.main.Shake(2, 0.1f);
        var bal = balloon.ShowBalloon("<color=\"red\">전투 준비.</color>", fighter.transform.position + new Vector3(0, 2.7f), 2f);
        bal.transform.localScale = bal.transform.localScale * 1.2f;

        yield return new WaitForSeconds(2f);

        CamManager.main.Offset(new Vector2(2, -2), 1f);
        CamManager.main.CloseUp(3, 0, 1f);

        yield return new WaitForSeconds(1.5f);

        StartCoroutine(shakeBody(mobu[2].transform, 10));

        yield return new WaitForSeconds(2f);

        balloon.ShowBalloon("?", mobu[0].transform.position + new Vector3(0, 1f), 2f);

        yield return new WaitForSeconds(2f);

        balloon.ShowBalloon("너 몸이 왜 그래?", mobu[0].transform.position + new Vector3(0, 1f), 2f);

        yield return new WaitForSeconds(2f);

        balloon.ShowBalloon("어..?", mobu[1].transform.position + new Vector3(0, 1f), 2f);

        StartCoroutine(shakeBody(mobu[1].transform, 8));

        yield return new WaitForSeconds(2f);

        StartCoroutine(shakeBody(mobu[0].transform, 8));

        yield return new WaitForSeconds(2f);
        curse1.SetActive(true);
        mobu[2].SetActive(false);
        CamManager.main.Shake(5, 0.2f);
        yield return new WaitForSeconds(1.5f);
        curse2.SetActive(true);
        mobu[1].SetActive(false);
        CamManager.main.Shake(5, 0.2f);
        yield return new WaitForSeconds(1.5f);
        curse3.SetActive(true);
        mobu[0].SetActive(false);
        CamManager.main.Shake(5, 0.2f);

        yield return new WaitForSeconds(2f);

        CamManager.main.Offset(Vector2.zero, 1.5f);
        CamManager.main.CloseOut(1.5f);

        yield return new WaitForSeconds(2.5f);
        CamManager.main.Shake(2, 0.1f);
        var bal2 = balloon.ShowBalloon("상실자들이 시현에 먹혔다!", fighter.transform.position + new Vector3(0, 2.7f), 2f);
        bal2.transform.localScale = bal2.transform.localScale * 1.2f;

        yield return new WaitForSeconds(2f);

        CamManager.main.Shake(2, 0.1f);
        var bal3 = balloon.ShowBalloon("괴물들을 찢으러 가자!", fighter.transform.position + new Vector3(0, 2.7f), 2f);
        bal3.transform.localScale = bal3.transform.localScale * 1.2f;

        yield return new WaitForSeconds(2f);

        fighter.transform.DOMoveX(3, 2f);
        fightch.animator.SetBool("isMoving", true);

        yield return new WaitForSeconds(1.8f);
        fightch.animator.SetBool("isMoving", false);
        fighter.SetActive(false);

        yield return new WaitForSeconds(2.5f);

        balloon.ShowBalloon("음..", samurai.transform.position + new Vector3(0, 2.5f), 2f);

        yield return new WaitForSeconds(2f);

        samurai.transform.DOMoveX(3, 2.5f);
        samuch.animator.SetBool("isMoving", true);

        yield return new WaitForSeconds(2.3f);
        samuch.animator.SetBool("isMoving", false);
        samurai.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        LoadingController.LoadScene("StageTuto-1");
    }

    IEnumerator shakeBody(Transform transform, float time) {
        Vector2 pos = transform.position;

        for (int i = 0; i < time / 0.05f; i++) {
            transform.position = pos + new Vector2(Random.Range(-0.15f, 0.15f), 0);
            yield return new WaitForSeconds(0.05f);
        }

        transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
