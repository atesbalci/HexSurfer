using Game.Networking;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public class ServersListingView : MonoBehaviour
    {
        public Button RefreshButton;
        public Button CreateServerButton;
        public RectTransform Content;

        [Space(10)]
        public GameObject ServerButtonPrefab;

        private void Start()
        {
            RefreshButton.onClick.AddListener(() =>
            {
                foreach (Transform child in Content)
                {
                    if (child.gameObject != ServerButtonPrefab)
                    {
                        Destroy(child.gameObject);
                    }
                }
                var rooms = PhotonNetwork.GetRoomList();
                foreach (var room in rooms)
                {
                    var but = Instantiate(ServerButtonPrefab, Content, false).GetComponent<Button>();
                    but.GetComponentInChildren<TextMeshProUGUI>().text = room.Name;
                    var room1 = room;
                    but.onClick.AddListener(() => FindObjectOfType<NetworkManager>().Join(room1.Name));
                    but.gameObject.SetActive(true);
                }
            });
            RefreshButton.onClick.Invoke();
            CreateServerButton.onClick.AddListener(FindObjectOfType<NetworkManager>().Host);
        }
    }
}
