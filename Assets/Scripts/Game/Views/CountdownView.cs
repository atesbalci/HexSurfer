using Game.Networking;
using Game.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Game.Views
{
    public class CountdownView : MonoBehaviour
    {
        public Animator CountdownNumberAnimation;
        public TextMeshProUGUI Text;

        private void Start()
        {
            CountdownNumberAnimation.Play(0, 0, 1f);
            MessageManager.ReceiveEvent<CountdownTickEvent>().Subscribe(ev =>
            {
                Text.text = ev.Number > 0 ? ev.Number.ToString() : "GO";
                CountdownNumberAnimation.Play(0, 0, 0f);
            });
        }
    }
}
