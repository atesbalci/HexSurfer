using UnityEngine;

namespace Game.Views
{
    public class UIView : MonoBehaviour
    {
        public GameObject ServerListing;
        public GameObject PreGame;

        private EngineManager _engine;

        private void Start()
        {
            _engine = FindObjectOfType<EngineManager>();
        }

        private void Update()
        {
            ServerListing.SetActive(PhotonNetwork.insideLobby);
            PreGame.SetActive(_engine.GameManager != null && _engine.GameManager.State == GameState.Idle);
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PhotonNetwork.Disconnect();
            }
        }
    }
}
