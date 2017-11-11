using UnityEngine;

namespace Game.Networking
{
    public class NetworkManager : Photon.MonoBehaviour
    {
        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings(Application.version);
        }
    }
}
