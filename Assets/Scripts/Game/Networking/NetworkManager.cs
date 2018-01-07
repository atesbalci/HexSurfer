using System;
using System.Collections.Generic;
using System.Linq;
using Game.Engine;
using Game.Models;
using Game.Utility;
using Photon;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Networking
{
    public class CountdownTickEvent : GameEvent
    {
        public int Number { get; set; }
    }

    public class BeginGameEvent : GameEvent { }

    [RequireComponent(typeof(EngineManager))]
    public class NetworkManager : PunBehaviour
    {
        private const float CountdownInterval = 1f;

        private EngineManager _engineManager;
        private float _defaultHeight;
        private Dictionary<PhotonPlayer, int> _playerIdCache;

        private void Start()
        {
            _playerIdCache = new Dictionary<PhotonPlayer, int>();
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
                            photonView.RPC("DefeatPlayer", PhotonTargets.All, id, _engineManager.GameManager.Players.Count(x => x.Defeated));
                        }
                    }
                }
            }).AddTo(gameObject);
            MessageManager.ReceiveEvent<StateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.Pre && PhotonNetwork.isMasterClient)
                {
                    var count = 3;
                    var disp = new IDisposable[1];
                    disp[0] = Observable.Interval(TimeSpan.FromSeconds(CountdownInterval)).Subscribe(lng =>
                    {
                        photonView.RPC("CountdownTick", PhotonTargets.All, count);
                        count--;
                        if (count < 0)
                        {
                            disp[0].Dispose();
                            photonView.RPC("Begin", PhotonTargets.All);
                        }
                    });
                }
            }).AddTo(gameObject);
            MessageManager.ReceiveEvent<BeginGameEvent>().Subscribe(ev => _engineManager.GameManager.State = GameState.Pre);
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
            var success = false;
            for (var i = 0; !success; i++)
            {
                success = PhotonNetwork.CreateRoom(PhotonNetwork.playerName + "'s Game" + (i > 0 ? " (" + i + ")" : ""));
            }
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
            _engineManager.Initialize();
            AddPlayer(PhotonNetwork.player);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            if (!PhotonNetwork.isMasterClient)
            {
                _engineManager.Initialize();
            }
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
            _playerIdCache[owner] = gameId;
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
        public void DefeatPlayer(int id, int defeatOrder)
        {
            _engineManager.DefeatPlayer(id, defeatOrder);
        }

        [PunRPC]
        public void CountdownTick(int n)
        {
            MessageManager.SendEvent(new CountdownTickEvent { Number = n });
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            base.OnPhotonPlayerDisconnected(otherPlayer);
            var id = _playerIdCache[otherPlayer];
            _engineManager.PlayerLeft(id);
            _playerIdCache.Remove(otherPlayer);
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(_engineManager.GameManager.State);
            }
            else
            {
                _engineManager.GameManager.State = (GameState)stream.ReceiveNext();
            }
        }
    }
}
