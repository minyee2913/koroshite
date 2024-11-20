using UnityEngine;

public class StoryEvent : MonoBehaviour
{
    public void Tutorial0() {
        LoadingController.LoadScene("Tutorial");
    }
    public void Tutorial1() {
        LobbyTutorial.storyTuto = true;
        LoadingController.LoadScene("Lobby");
    }
    public void Tutorial2() {
        LoadingController.LoadScene("StageTuto-1");
    }

    public void Page1_1() {
        LoadingController.LoadScene("Boss1");
    }
}
