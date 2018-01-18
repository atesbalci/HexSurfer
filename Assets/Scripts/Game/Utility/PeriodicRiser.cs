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

        private void Start()
        {
            _timer = 0;
            _hex = FindObjectOfType<HexagonTiler>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= Period)
            {
                _timer -= Period;
                _hex.RadialRise(transform.position, Radius, Curve, Id);
            }
        }
    }
}
