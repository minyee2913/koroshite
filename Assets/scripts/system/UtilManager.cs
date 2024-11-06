using Photon.Pun;
using UnityEngine;

public class UtilManager : MonoBehaviour
{
    public DamageInfo damInfo;
    public static UtilManager Instance {get; private set;}
    void Start()
    {
        Instance = this;
    }

    public static bool CheckPhoton() {
        return PhotonNetwork.IsConnected && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom;
    }

    public static void DisplayDamage(Vector2 pos, int damage, Color col = default) {
        var obj = Instantiate(Instance.damInfo, pos, Quaternion.identity);

        obj.txt = damage.ToString();
        obj.color = col;
    }
}
