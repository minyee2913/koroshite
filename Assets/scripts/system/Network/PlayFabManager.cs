using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using Photon.Pun;

public class PlayFabManager : MonoBehaviour
{
    public static bool login = false;
    public static string playfabId;
    [SerializeField] InputField nameField;
    void Start(){
        // string login_type = PlayerPrefs.GetString("login_type");
        // string login_id = PlayerPrefs.GetString("login_id");

        // if (login_type == "guest") {

        // }
    }



    public void LoginQuest() {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest(){
            CreateAccount = true,
            CustomId = PlayFabSettings.DeviceUniqueIdentifier,
        }, loginSuccess,
        loginFail
        );
    }

    public void EnterName() {
        string name = nameField.text;

        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = name
        },
        (result) =>
        {
            Debug.Log("Update User NickName : " + result.DisplayName);

            var obj = GameObject.Find("startManager");
            if (obj != null) {
                var manager = obj.GetComponent<StartManager>();

                if (manager != null) {
                    manager.AfterLogin();
                }
            }


        },
        DisplayPlayfabError);
    }

    void loginSuccess(LoginResult success) {
        Debug.Log("로그인 성공");

        playfabId = success.PlayFabId;

        if (success.NewlyCreated) {
            Debug.Log("새로 생성됨");

            startEntering();
        } else {
            GetDisplayName();

            startEntering();
        }
    }

    void startEntering() {
        var obj = GameObject.Find("startManager");
        if (obj != null) {
            var manager = obj.GetComponent<StartManager>();

            if (manager != null) {
                manager.afterConnected.SetActive(false);
            }
        }

        nameField.gameObject.SetActive(true);
        nameField.text = "";
    }

    void loginFail(PlayFabError fail) {
        Debug.Log("실패");
        Debug.Log(fail.ErrorMessage);
    }

    private void DisplayPlayfabError(PlayFabError error) => Debug.LogError("error : " + error.GenerateErrorReport());
 
    public void GetDisplayName()
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            PlayFabId = playfabId,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true
            }
        },
        (result) =>
        {
            PhotonNetwork.NickName = result.PlayerProfile.DisplayName;
        },
        DisplayPlayfabError);
    }
}
