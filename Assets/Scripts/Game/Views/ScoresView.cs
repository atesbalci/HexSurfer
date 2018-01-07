using System.Collections.Generic;
using System.Text;
using Game.Engine;
using Game.Models;
using Game.Networking;
using Game.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public class ScoresView : MonoBehaviour
    {
        public TextMeshProUGUI Text;
        public Button StartButton;
        
        private EngineManager _engineManager;
        private Dictionary<int, string> _nicknameCache;

        private void Awake()
        {
            StartButton.onClick.AddListener(() => MessageManager.SendEvent(new BeginGameEvent()));
            _engineManager = FindObjectOfType<EngineManager>();
            _nicknameCache = new Dictionary<int, string>();
        }

        private void OnEnable()
        {
            RefreshNickNameCache();
        }

        private void RefreshNickNameCache()
        {
            _nicknameCache.Clear();
            foreach (var player in _engineManager.GameManager.Players)
            {
                _nicknameCache.Add(player.Id, _engineManager.Players[player.Id].GetComponent<PhotonView>().owner.NickName);
            }
        }

        private void Update()
        {
            if(_nicknameCache.Count != PhotonNetwork.playerList.Length)
                RefreshNickNameCache();
            var pre = _engineManager.GameManager != null && _engineManager.GameManager.State == GameState.Idle && _engineManager.GameManager.RoundsPlayed == 0;
            StartButton.gameObject.SetActive(PhotonNetwork.isMasterClient && pre);
            if (_engineManager.GameManager == null)
                return;
            var str = new StringBuilder();
            foreach (var player in _engineManager.GameManager.Players)
            {
                if (str.Length > 0)
                    str.AppendLine();
                var nick = _nicknameCache[player.Id];
                str.Append(nick.Length > 0 ? nick : " ");
                if (!pre)
                {
                    str.Append(": ");
                    str.Append(player.Score);
                }
            }
            Text.text = str.ToString();
        }
    }
}
