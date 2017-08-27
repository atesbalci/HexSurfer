using UnityEngine;

namespace Game.Utility
{
    public enum HexagonState
    {
        Rising, Falling, Idle
    }
    public class Hexagon : MonoBehaviour
    {
        public float TargetHeight { get; set; }
        public Transform Trans { get; set; }
        public Vector3 Position { get; set; }
        public float DefaultHeight { get; set; }
        public HexagonState State;

        public float RiseSpeed;
        public float FallSpeed;

        private float _currentHeight;
        private int _currentSource;
        private Material _defaultMaterial;
        private Renderer _rend;

        public int CurrentSource
        {
            get { return _currentSource; }
            set
            {
                if (value != CurrentSource)
                {
                    if (value == -1)
                    {
                        _rend.sharedMaterial = _defaultMaterial;
                    }
                    else
                    {
                        _rend.material.color = Character.Colors[value];
                    }
                }
                _currentSource = value;
            }
        }

        public void Init()
        {
            Trans = transform;
            TargetHeight = Trans.localScale.y;
            _currentHeight = Trans.localScale.y;
            Position = Trans.position;
            _rend = GetComponentInChildren<Renderer>();
            _defaultMaterial = _rend.sharedMaterial;
            _currentSource = -1;
            State = HexagonState.Falling;
        }

        public void Refresh()
        {
            if (State != HexagonState.Idle)
            {
                var scale = Trans.localScale;
                _currentHeight = Mathf.MoveTowards(scale.y, TargetHeight, Time.deltaTime * (_currentHeight > TargetHeight ? FallSpeed : RiseSpeed));
                Trans.localScale = new Vector3(scale.x, _currentHeight, scale.z);
                if (State == HexagonState.Falling)
                {
                    if (Mathf.Abs(TargetHeight - _currentHeight) < 1)
                    {
                        State = HexagonState.Idle;
                        CurrentSource = -1;
                    }
                }
            }
        }
    }
}