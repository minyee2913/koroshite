using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;
using System;

public class PlayFabManager : MonoBehaviour
{
    public static bool login = false;
    public static string playfabId;
    public static bool tutorialEnded;
    [Header("titleScene")]
    [SerializeField] InputField nameField;
    [SerializeField] InputField registerName, registerEmail, registerPw, loginEmail, loginPw;
    [SerializeField] GameObject registerPanel, loginPanel;
    void Start(){
        // string login_type = PlayerPrefs.GetString("login_type");
        // string login_id = PlayerPrefs.GetString("login_id");

        // if (login_type == "guest") {

        // }
    }

    public StartManager GetStartManager() {
        var obj = GameObject.Find("StartManager");
        if (obj != null) {
            var manager = obj.GetComponent<StartManager>();

            if (manager != null) {
                return manager;
            }
        }

        return null;
    }

    public void OpenLogin() {
        var manager = GetStartManager();
        if (manager != null) manager.afterConnected.SetActive(false);

        loginPanel.SetActive(true);
        loginEmail.text = loginPw.text = "";
    }

    public void OpenRegister() {
        var manager = GetStartManager();
        if (manager != null) manager.afterConnected.SetActive(false);

        registerPanel.SetActive(true);
        registerEmail.text = registerName.text = registerPw.text = registerName.text = "";
    }

    public void Login() {
        if (loginEmail.text.Length <= 0 || loginPw.text.Length <= 0) {
            return;
        }

        PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest(){
            Email = loginEmail.text,
            Password = loginPw.text,
        }, loginSuccess, DisplayPlayfabError);
    }
    public static void SetTutorialEnded() {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest{
            Data = new Dictionary<string, string>(){
                {"tutorialEnd", "true"}
            },
        }, success => {}, DisplayPlayfabError);
    }

    public static void GetHasCharacter(Action<GetUserDataResult> callback, Action<PlayFabError> fail){
        if (playfabId == "") {
            return;
        }

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
            PlayFabId = playfabId,
        }, success => {
            callback(success);
        }, fail);
    }

    public static void SetHasCharacter(Dictionary<string, string> data){
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest{
            Data = data,
        }, success => {}, DisplayPlayfabError);
    }

    public static void CheckTutorialEnded() {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
            PlayFabId = playfabId,
        }, success => {
            if (success.Data.ContainsKey("tutorialEnd")) {
                if (success.Data["tutorialEnd"].Value == "true") {
                    tutorialEnded = true;
                } else {
                    tutorialEnded = false;
                }
            } else {
                tutorialEnded = false;
            }
        }, DisplayPlayfabError);
    }

    public void Register() {
        if (registerEmail.text.Length <= 0 || registerName.text.Length <= 0 || registerPw.text.Length <= 0) {
            return;
        }
        PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest(){
            Email = registerEmail.text,
            Password = registerPw.text,
            DisplayName = registerName.text,
            RequireBothUsernameAndEmail = false,
        }, success => {
            registerPanel.SetActive(false);

            var manager = GetStartManager();
            if (manager != null) manager.AfterLogin();

            PhotonNetwork.NickName = registerName.text;
        }, DisplayPlayfabError);
    }

    public void ReturnToConnected() {
        registerPanel.SetActive(false);
        nameField.gameObject.SetActive(false);
        
        var obj = GameObject.Find("StartManager");
        if (obj != null) {
            var manager = obj.GetComponent<StartManager>();

            if (manager != null) {
                manager.afterConnected.SetActive(true);
                manager.afterLogin.SetActive(false);
            }
        }
    }

    public void LoginGuest() {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest(){
            CreateAccount = true,
            CustomId = PlayFabSettings.DeviceUniqueIdentifier,
        }, loginSuccess,
        loginFail
        );
    }

    public void EnterName() {
        string name = nameField.text;

        if (name == "") {
            return;
        }

        nameField.gameObject.SetActive(false);

        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = name
        },
        (result) =>
        {
            Debug.Log("Update User NickName : " + result.DisplayName);

            NetworkManager.instance.playerName = name;

            var obj = GameObject.Find("StartManager");
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

        loginPanel.SetActive(false);

        login = true;

        Debug.Log("NewlyCreated : " + success.NewlyCreated);

        if (success.NewlyCreated) {
            Debug.Log("새로 생성됨");

            startEntering();
        } else {
            GetDisplayName();
            CheckTutorialEnded();

            var obj = GameObject.Find("StartManager");
            if (obj != null) {
                var manager = obj.GetComponent<StartManager>();

                if (manager != null) {
                    manager.AfterLogin();
                }
            }
        }
    }

    void startEntering() {
        var obj = GameObject.Find("StartManager");
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

    private static void DisplayPlayfabError(PlayFabError error) => Debug.LogError("error : " + error.GenerateErrorReport());
 
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
            NetworkManager.instance.playerName = result.PlayerProfile.DisplayName;
            Debug.Log(NetworkManager.instance.playerName);
        },
        DisplayPlayfabError);
    }
}
