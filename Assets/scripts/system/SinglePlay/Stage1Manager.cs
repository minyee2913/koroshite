using System.Collections;
using UnityEngine;

public class Stage1Manager : MonoBehaviour
{
    [SerializeField] GameObject cover1, cover2, cover_all;
    bool started;
    void Start()
    {
        cover_all.SetActive(true);
        cover1.SetActive(true);
        cover2.SetActive(true);

        StartCoroutine(starting());
    }

    IEnumerator starting() {
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

        started = true;
    }

    void Update() {
        if (started) {
            if (Monster.monsters.Count <= 0) {
                LobbyTutorial.afterTuto = true;
                LoadingController.LoadScene("Lobby");
            }
        }
    }
}
