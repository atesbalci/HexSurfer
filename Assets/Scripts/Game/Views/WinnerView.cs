using Game.Engine;
using Game.Models;
using Game.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Game.Views
{
    public class WinnerView : MonoBehaviour
    {
        private Animator _anim;
        private TextMeshProUGUI _text;
        private EngineManager _engine;

        private void Start()
        {
            _anim = GetComponent<Animator>();
            _text = GetComponent<TextMeshProUGUI>();
            _engine = FindObjectOfType<EngineManager>();
            MessageManager.ReceiveEvent<GameOverEvent>().Subscribe(ev =>
            {
                _text.text = ev.Winner.Name + " Wins!";
                _anim.Play(0, 0, 0f);
            });
        }

        private void Update()
        {
            if (_engine.GameManager == null)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
