using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SinglePlayManager : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] string stageCharacter;
    void Start()
    {
        SetCh(stageCharacter);
    }

    public void SetCh(string id) {
        var chData = CharacterManager.instance.Get(id);

        if (chData != null) {
            var ch = Instantiate(chData);

            player._setCh(ch);
        }
    }
}
