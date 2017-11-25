using System;
using Game.Utility;
using Photon;
using UniRx;
using UnityEngine;

namespace Game.Networking
{
    [RequireComponent(typeof(EngineManager))]
    public class NetworkManager : PunBehaviour
    {
        public GameObject Menu;

        private EngineManager _engineManager;
        private float _defaultHeight;

        private void Start()
        {
            _engineManager = GetComponent<EngineManager>();
            PhotonNetwork.ConnectUsingSettings(Application.version);
            _engineManager.Players.ObserveAdd().Subscribe(x =>
            {
                if (_engineManager.Players.Count == 2 && PhotonNetwork.isMasterClient)
                {
                    Observable.Timer(TimeSpan.FromSeconds(2f)).Subscribe(lng => photonView.RPC("Begin", PhotonTargets.AllBufferedViaServer));
                }
            });
            _defaultHeight = Resources.Load<GameObject>("Player").transform.position.y;
            MessageManager.ReceiveEvent<PlayersDefeatedEvent>().Subscribe(ev =>
            {
                if (PhotonNetwork.isMasterClient)
                {
                    foreach (var id in ev.Ids)
                    {
                        if (id < _engineManager.Players.Count && _engineManager.Players[id])
                        {
                            photonView.RPC("DefeatPlayer", PhotonTargets.All, id);
                        }
                    }
                }
            });
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Menu.SetActive(true);
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
            int gameId;
            for (gameId = 0; gameId < 4; gameId++)
            {
                if (!_engineManager.Players.ContainsKey(gameId))
                    break;
            }
            photonView.RPC("InstantiatePlayer", newPlayer, gameId);
        }

        [PunRPC]
        public void InstantiatePlayer(int gameId)
        {
            var pl = PhotonNetwork.Instantiate("Player",
                new Vector3(_engineManager.SpawnPoints[gameId].x, -1000f, _engineManager.SpawnPoints[gameId].y),
                Quaternion.identity, 0).GetComponent<PhotonView>();
            photonView.RPC("InitializePlayer", PhotonTargets.AllViaServer, pl.viewID, gameId);
        }

        [PunRPC]
        public void InitializePlayer(int networkId, int gameId)
        {
            IDisposable disp = null;
            disp = Observable.EveryUpdate().Subscribe(lng =>
            {
                var playerView = PhotonView.Find(networkId);
                if (playerView != null)
                {
                    var player = playerView.GetComponent<Player>();
                    player.Init(gameId, playerView.isMine);
                    player.gameObject.SetActive(_engineManager.GameManager.State == GameState.Playing);
                    _engineManager.Players.Add(gameId, player);
                    if (disp != null)
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
                pl.Value.transform.position = new Vector3(_engineManager.SpawnPoints[pl.Key].x, _defaultHeight, _engineManager.SpawnPoints[pl.Key].y);
                pl.Value.gameObject.SetActive(true);
            }
        }

        [PunRPC]
        public void DefeatPlayer(int id)
        {
            _engineManager.DefeatPlayer(id);
        }
    }
}
