using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Boss1Manager : MonoBehaviour
{
    [SerializeField]
    GameObject boss, dummy;
    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    GlobalBalloon balloon;
    [SerializeField]
    Text title;
    [SerializeField]
    Player player;
    bool started;
    void Start()
    {
        StartCoroutine(starting());
    }

    IEnumerator starting() {
        tilemap.color = Color.black;
        CamManager.main.CloseUp(6, 0);
        
        player.preventInput = 100000;

        UIManager.Instance.state.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        balloon.ShowBalloon("정말로...", dummy.transform.position + new Vector3(0, 2), 2);

        yield return new WaitForSeconds(2f);

        balloon.ShowBalloon("그 길을 가야겠느냐.", dummy.transform.position + new Vector3(0, 2), 2);

        yield return new WaitForSeconds(3f);

        balloon.ShowBalloon("...", dummy.transform.position + new Vector3(0, 2), 1.5f);

        yield return new WaitForSeconds(3f);

        balloon.ShowBalloon("나도 각오를 다져야겠지.", dummy.transform.position + new Vector3(0, 2), 2);

        yield return new WaitForSeconds(2f);

        dummy.transform.localScale = new Vector3(-dummy.transform.localScale.x, dummy.transform.localScale.y);

        yield return new WaitForSeconds(0.5f);

        CamManager.main.CloseOut(2f);

        yield return new WaitForSeconds(2.5f);

        tilemap.color = Color.white;

        player.preventInput = 0;

        CamManager.main.Shake(8, 0.1f);
        SoundManager.Instance.Play("criticalTh");

        dummy.SetActive(false);
        boss.SetActive(true);

        Player.Local.ForceWhite = false;
        Player.Local.hprate.gameObject.SetActive(true);
        UIManager.Instance.state.gameObject.SetActive(true);

        Vector2 titleScale = title.transform.localScale;
        title.text = "[ 기억의 환영 ]";
        title.transform.localScale = titleScale * 0.8f;
        title.transform.DOScale(titleScale, 0.5f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(1.5f);

        title.text = "";

        started = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (started) {
            if (Monster.monsters.Count <= 0) {
                LoadingController.LoadScene("Lobby");
            }
        }
    }
}
