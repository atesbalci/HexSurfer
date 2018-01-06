using System;
using UniRx;
using UnityEngine;

namespace Game.Utility
{
    public class PeriodicRiser : MonoBehaviour
    {
        public float Period;
        public float Duration;
        public AnimationCurve Curve;
        public float Radius;
        public int Id;

        private HexagonTiler _hex;
        private float _timer;
        private HexRiser _riser;

        private void Start()
        {
            _timer = 0;
            _hex = FindObjectOfType<HexagonTiler>();
            _riser = new HexRiser { Active = false, Location = transform.position, Source = Id, RiserCurve = Curve, Radius = Radius };
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            _riser.Location = transform.position;
            if (_timer >= Period)
            {
                _timer -= Period;
                _riser.Active = true;
                Observable.Timer(TimeSpan.FromSeconds(Duration)).Subscribe(l => _riser.Active = false);
                _hex.AddHexRiser(_riser);
            }
        }
    }
}
