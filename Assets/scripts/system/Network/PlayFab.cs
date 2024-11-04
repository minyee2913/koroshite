using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabManager : MonoBehaviour
{
    public bool login = false;
    string guestId;
    void Start(){
        // string login_type = PlayerPrefs.GetString("login_type");
        // string login_id = PlayerPrefs.GetString("login_id");

        // if (login_type == "guest") {

        // }
    }

    public void InitPlayFab(){
        PlayFabSettings.TitleId = "koroshite";

        

    }

    public void LoginQuest() {
        guestId = "guest" + Random.Range(1000000, 9999999);
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest(){
            CreateAccount = true,
            CustomId = guestId,
        }, loginSuccess,
        fail=>{}
        );
    }

    void loginSuccess(LoginResult success) {
        Debug.Log("로그인 성공");

        PlayerPrefs.SetString("login_type", "quest");
        PlayerPrefs.SetString("login_id", guestId);
    }
}
