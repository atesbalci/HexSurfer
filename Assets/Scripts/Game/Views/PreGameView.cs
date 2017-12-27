using System.Text;
using Game.Networking;
using Game.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public class PreGameView : MonoBehaviour
    {
        public TextMeshProUGUI Text;
        public Button StartButton;

        private void Start()
        {
            StartButton.onClick.AddListener(() => MessageManager.SendEvent(new BeginGameEvent()));
        }

        private void Update()
        {
            StartButton.gameObject.SetActive(PhotonNetwork.isMasterClient);
            var str = new StringBuilder();
            foreach (var player in PhotonNetwork.playerList)
            {
                if (str.Length > 0)
                    str.AppendLine();
                str.Append(player.NickName);
            }
            Text.text = str.ToString();
        }
    }
}
