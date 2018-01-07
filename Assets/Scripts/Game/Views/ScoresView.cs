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

        private void Awake()
        {
            StartButton.onClick.AddListener(() => MessageManager.SendEvent(new BeginGameEvent()));
            _engineManager = FindObjectOfType<EngineManager>();
        }

        private void Update()
        {
            var pre = _engineManager.GameManager != null && _engineManager.GameManager.State == GameState.Idle && _engineManager.GameManager.RoundsPlayed == 0;
            StartButton.gameObject.SetActive(PhotonNetwork.isMasterClient && pre);
            if (_engineManager.GameManager == null)
                return;
            var str = new StringBuilder();
            foreach (var player in _engineManager.GameManager.Players)
            {
                if (str.Length > 0)
                    str.AppendLine();
                str.Append("<color=#");
                str.Append(ColorUtility.ToHtmlStringRGBA(Player.Colors[player.Id]));
                str.Append(">");
                str.Append(player.Name.Length > 0 ? player.Name : " ");
                str.Append("</color>");
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
