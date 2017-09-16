using UnityEngine;

namespace Game
{
    public class Energy
    {
        public const float RefillCooldown = 0.5f;

        public float Value
        {
            get { return _value; }
            set
            {
                if (value < _value)
                {
                    _refillCooldownLeft = RefillCooldown;
                }
                _value = value;
            }
        }

        private float _value;
        private float _refillCooldownLeft;

        public Energy()
        {
            _value = 0;
            _refillCooldownLeft = 0;
        }

        public void Update(float deltaTime)
        {
            if (_refillCooldownLeft > 0)
            {
                _refillCooldownLeft -= deltaTime;
            }
            if (_refillCooldownLeft <= 0)
            {
                Value = Mathf.MoveTowards(Value, 1.0f, deltaTime * 0.5f);
            }
        }
    }
}
