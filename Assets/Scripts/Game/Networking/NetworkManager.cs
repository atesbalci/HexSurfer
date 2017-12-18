using System;
using Game.Utility;
using Photon;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Networking
{
    [RequireComponent(typeof(EngineManager))]
    public class NetworkManager : PunBehaviour
    {
        private EngineManager _engineManager;
        private float _defaultHeight;

        private void Start()
        {
            _engineManager = GetComponent<EngineManager>();
            while (PhotonNetwork.connectionState != ConnectionState.Disconnected) { }
            PhotonNetwork.ConnectUsingSettings(Application.version);
            _defaultHeight = Resources.Load<GameObject>("Player").transform.position.y;
            MessageManager.ReceiveEvent<PlayersDefeatedEvent>().Subscribe(ev =>
            {
                if (PhotonNetwork.isMasterClient)
                {
                    foreach (var id in ev.Ids)
                    {
                        if (id < _engineManager.Players.Length && _engineManager.Players[id].gameObject.activeInHierarchy)
                        {
                            photonView.RPC("DefeatPlayer", PhotonTargets.All, id);
                        }
                    }
                }
            }).AddTo(gameObject);
            _engineManager.GameManager.Players.ObserveAdd().Subscribe(add =>
            {
                if(PhotonNetwork.isMasterClient && _engineManager.GameManager.Players.Count > 1)
                    Observable.Timer(TimeSpan.FromSeconds(2f)).Subscribe(lng => photonView.RPC("Begin", PhotonTargets.AllBuffered));
            });
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            PhotonNetwork.JoinLobby();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                PhotonNetwork.Disconnect();
            }
        }

        public override void OnDisconnectedFromPhoton()
        {
            base.OnDisconnectedFromPhoton();
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        public void Host()
        {
            PhotonNetwork.CreateRoom("Room " + Random.Range(0, int.MaxValue));
        }

        public void Join(string room)
        {
            PhotonNetwork.JoinRoom(room);
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
            var gameId = _engineManager.GameManager.GetNextAvailableId();
            photonView.RPC("InitializePlayer", PhotonTargets.AllBuffered, gameId, newPlayer);
        }

        [PunRPC]
        public void InitializePlayer(int gameId, PhotonPlayer owner)
        {
            _engineManager.GameManager.AddPlayer(gameId, owner.NickName);
            _engineManager.Players[gameId].GetComponent<PhotonView>().TransferOwnership(owner);
            _engineManager.Players[gameId].Init(ReferenceEquals(PhotonNetwork.player, owner));
        }

        [PunRPC]
        public void Begin()
        {
            _engineManager.GameManager.State = GameState.Playing;
            foreach (var player in _engineManager.GameManager.Players)
            {
                var pl = _engineManager.Players[player.Id];
                pl.transform.position = new Vector3(_engineManager.SpawnPoints[player.Id].x, _defaultHeight,
                    _engineManager.SpawnPoints[player.Id].y);
                pl.gameObject.SetActive(true);
            }
        }

        [PunRPC]
        public void DefeatPlayer(int id)
        {
            _engineManager.DefeatPlayer(id);
        }
    }
}
