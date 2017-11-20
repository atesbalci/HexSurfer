using System;
using System.Linq;
using Photon;
using UniRx;
using UnityEngine;

namespace Game.Networking
{
    [RequireComponent(typeof(EngineManager))]
    public class NetworkManager : PunBehaviour
    {
        private EngineManager _engineManager;

        private void Start()
        {
            _engineManager = GetComponent<EngineManager>();
            PhotonNetwork.ConnectUsingSettings(Application.version);
            _engineManager.Players.ObserveAdd().Subscribe(x =>
            {
                if (_engineManager.Players.Count == 2)
                {
                    photonView.RPC("Begin", PhotonTargets.AllBuffered);
                }
            });
        }

        public void Host()
        {
            PhotonNetwork.CreateRoom("Room");
        }

        public void Join()
        {
            PhotonNetwork.JoinRoom("Room");
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            base.OnPhotonPlayerConnected(newPlayer);
            if (PhotonNetwork.isMasterClient)
                AddPlayer(newPlayer);
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            AddPlayer(PhotonNetwork.player);
        }

        public void AddPlayer(PhotonPlayer newPlayer)
        {
            var id = _engineManager.Players.Count;
            PhotonNetwork.Instantiate("Player",
                new Vector3(_engineManager.SpawnPoints[id].x, 0, _engineManager.SpawnPoints[id].y),
                Quaternion.identity, 0);
            photonView.RPC("InitializePlayer", PhotonTargets.All, newPlayer.ID, id);
        }

        [PunRPC]
        public void InitializePlayer(int networkId, int gameId)
        {
            IDisposable disp = null;
            disp = Observable.EveryUpdate().Subscribe(lng =>
            {
                var player = FindObjectsOfType<Player>()
                    .FirstOrDefault(x => x.GetComponent<PhotonView>().owner.ID == networkId);
                if (player != null)
                {
                    player.Id = gameId;
                    player.gameObject.SetActive(_engineManager.GameManager.State == GameState.Playing);
                    _engineManager.Players.Add(player);
                    disp.Dispose();
                }
            });
        }

        [PunRPC]
        public void Begin()
        {
            _engineManager.GameManager.State = GameState.Playing;
            foreach (var pl in _engineManager.Players)
            {
                pl.gameObject.SetActive(true);
            }
        }
    }
}
