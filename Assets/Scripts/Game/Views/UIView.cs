using Game.Engine;
using Game.Models;
using UnityEngine;

namespace Game.Views
{
    public class UIView : MonoBehaviour
    {
        public GameObject ServerListing;
        public GameObject Intermission;

        private EngineManager _engine;

        private void Start()
        {
            _engine = FindObjectOfType<EngineManager>();
        }

        private void Update()
        {
            ServerListing.SetActive(PhotonNetwork.insideLobby);
            Intermission.SetActive(_engine.GameManager != null &&
                                   (_engine.GameManager.State == GameState.Idle &&
                                     _engine.GameManager.RoundsPlayed == 0 ||
                                     Input.GetKey(KeyCode.Tab) && _engine.GameManager.State == GameState.Playing ||
                                    _engine.GameManager.State == GameState.Post));
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PhotonNetwork.Disconnect();
            }
        }
    }
}