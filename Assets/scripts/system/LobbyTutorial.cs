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
    Samurai samuch;
    Fighter fightch;
    void Start()
    {
        if (PlayFabManager.tutorialEnded) {
            afterTutorial.SetActive(true);
        } else {
            afterTutorial.SetActive(false);

            StartCoroutine(starting());
        }
    }

    IEnumerator starting() {
        tuto.SetActive(true);
        samurai.transform.position = new Vector3(-10, -3.23f);
        samuch = samurai.GetComponentInChildren<Samurai>();
        fightch = fighter.GetComponentInChildren<Fighter>();
        yield return new WaitForSeconds(1);

        samurai.transform.DOMoveX(-7, 1.5f);
        samuch.animator.SetBool("isMoving", true);

        yield return new WaitForSeconds(1.5f);
        samuch.animator.SetBool("isMoving", false);

        yield return new WaitForSeconds(1f);

        balloon.ShowBalloon("사람인가?", samurai.transform.position + new Vector3(0, 2.5f), 1);

        yield return new WaitForSeconds(2f);

        balloon.ShowBalloon("못해... 못하겠어", mobu[0].transform.position + new Vector3(0, 1f), 1.5f);

        yield return new WaitForSeconds(1.5f);

        balloon.ShowBalloon("사무직인 나한테 왜 전투를 시키는거야..!", mobu[2].transform.position + new Vector3(0, 1f), 1.5f);

        yield return new WaitForSeconds(1.5f);

        balloon.ShowBalloon("그만하고 싶어...", mobu[1].transform.position + new Vector3(0, 1f), 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
