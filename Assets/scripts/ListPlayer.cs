using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ListPlayer : MonoBehaviour
{
    public Text nametag, blockTx;
    public Image profile;
    public Button giveMaster, block, kick;
    public string userId;
    public int actorNumber;

    public void KickPlayer() {
        var p = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        PhotonNetwork.CloseConnection(p);

        ChatManager.Instance.SendComment("<color=\"red\">" + p.NickName + "님을 추방하였습니다.</color>");

        kick.gameObject.SetActive(false);

        Invoke("UpdateList", 0.3f);
    }

    public void GiveMaster() {
        var p = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

        PhotonNetwork.SetMasterClient(p);

        ChatManager.Instance.SendComment("<color=\"yellow\">방장이 " + p.NickName + "님으로 변경되었습니다!</color>");

        Invoke("UpdateList", 0.3f);
    }

    public void BlockPlayer() {
        var p = Player.players.Find((v)=>v.pv.Owner.UserId == userId);

        p.blocked = !p.blocked;
        Invoke("UpdateList", 0.3f);
    }

    void UpdateList() {
        UIManager.Instance.UpdatePlayerList();
    }
}
