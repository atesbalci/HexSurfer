using UnityEngine;

namespace Game.Utility
{
    public class Hexagon : MonoBehaviour
    {
        public float TargetHeight { get; set; }
        public Transform Trans { get; set; }
        public Vector3 Position { get; set; }

        public float RiseSpeed;
        public float FallSpeed;

        private float _currentHeight;

        public void Init()
        {
            Trans = transform;
            TargetHeight = Trans.localScale.y;
            _currentHeight = Trans.localScale.y;
            Position = Trans.position;
        }

        public void Refresh()
        {
            if (Abs(_currentHeight - TargetHeight) > 0.01f)
            {
                var scale = Trans.localScale;
                _currentHeight = Mathf.Lerp(scale.y, TargetHeight, Time.deltaTime * (_currentHeight > TargetHeight ? FallSpeed : RiseSpeed));
                Trans.localScale = new Vector3(scale.x, _currentHeight, scale.z);
            }
        }

        private static float Abs(float f)
        {
            return f > 0 ? f : -f;
        }
    }
}