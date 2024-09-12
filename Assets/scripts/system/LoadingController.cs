using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    static string nextScene;
    public GameObject[] images;
    public static bool loaded;

    [SerializeField]
    Slider progress;

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Loading");
    }

    private void Start()
    {
        StartCoroutine(SceneProcess());
    }

    IEnumerator SceneProcess()
    {
        loaded = false;
        AsyncOperation load = SceneManager.LoadSceneAsync(nextScene);
        load.allowSceneActivation = false;

        float timer = 0f;
        while (!load.isDone)
        {
            yield return null;

            timer += Time.unscaledTime;
            if (load.progress < 0.9f)
            {
                progress.value = load.progress;
            } else
            {
                progress.value = Mathf.Lerp(0.9f, 1f, timer);
                if (progress.value >= 1f)
                {
                    loaded = true;
                    load.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
