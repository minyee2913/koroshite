using System.Collections;
using UnityEngine;

public class LobbyTutorial : MonoBehaviour
{
    [SerializeField] GameObject afterTutorial;
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
        yield return new WaitForSeconds(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
