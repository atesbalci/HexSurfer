using System;
using System.Collections.Generic;
using System.Linq;
using Photon;
using UniRx;
using UnityEngine;

namespace Game.Networking
{
    public class NetworkManager : PunBehaviour
    {
        public static Dictionary<int, int> Ids { get; private set; }

        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings(Application.version);
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
            var engine = FindObjectOfType<EngineManager>();
            var id = engine.Players.Count;
            PhotonNetwork.Instantiate("Player",
                new Vector3(engine.SpawnPoints[id].x, 0, engine.SpawnPoints[id].y),
                Quaternion.identity, 0);
            photonView.RPC("InitializePlayer", PhotonTargets.AllBuffered, newPlayer.ID, id);
            if (PhotonNetwork.playerList.Length > 1)
            {
                photonView.RPC("Begin", PhotonTargets.AllBuffered);
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            if (Ids == null)
                Ids = new Dictionary<int, int>();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Ids = null;
        }

        [PunRPC]
        public void InitializePlayer(int networkId, int gameId, PhotonMessageInfo info)
        {
            if (Ids == null)
                Ids = new Dictionary<int, int>();
            Ids.Add(networkId, gameId);
            IDisposable disp = null;
            disp = Observable.EveryUpdate().Subscribe(lng =>
            {
                var player = FindObjectsOfType<Player>()
                    .FirstOrDefault(x => x.GetComponent<PhotonView>().owner.ID == networkId);
                if (player != null)
                {
                    player.Id = gameId;
                    if (Ids.ContainsKey(PhotonNetwork.player.ID))
                        player.IsMine = Ids[PhotonNetwork.player.ID] == player.Id;
                    var engine = FindObjectOfType<EngineManager>();
                    engine.Players.Add(player);
                    disp.Dispose();
                }
            });
        }

        [PunRPC]
        public void Begin()
        {
            var engine = FindObjectOfType<EngineManager>();
            foreach (var pl in engine.Players)
            {
                pl.gameObject.SetActive(true);
            }
        }
    }
}
