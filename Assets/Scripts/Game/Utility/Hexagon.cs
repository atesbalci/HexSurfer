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

        public float RiseSpeed;
        public float FallSpeed;

        private float _currentHeight;
        private int _currentSource;
        private Color _defaultColor;
        private Material _defaultMaterial;
        private Renderer _rend;

        public int CurrentSource
        {
            get { return _currentSource; }
            set
            {
                if (value == -1)
                {
                    _rend.sharedMaterial = _defaultMaterial;
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
            _defaultColor = _defaultMaterial.color;
            _currentSource = -1;
        }

        public void Refresh()
        {
            var state = GetState();
            if (state != HexagonState.Idle)
            {
                var scale = Trans.localScale;
                _currentHeight = Mathf.Lerp(scale.y, TargetHeight, Time.deltaTime * (_currentHeight > TargetHeight ? FallSpeed : RiseSpeed));
                Trans.localScale = new Vector3(scale.x, _currentHeight, scale.z);
                if (CurrentSource >= 0)
                {
                    var mat = _rend.material;
                    mat.color = state == HexagonState.Falling ? Color.Lerp(mat.color, _defaultColor, TargetHeight / _currentHeight) : Character.Colors[CurrentSource];
                }
            }
            else
            {
                CurrentSource = -1;
            }

        }

        private HexagonState GetState()
        {
            var diff = _currentHeight - TargetHeight;
            if (_currentHeight - DefaultHeight < 0.01f && _currentHeight - DefaultHeight > 0.01f)
            {
                return HexagonState.Idle;
            }
            if (diff < 0)
            {
                return HexagonState.Rising;
            }
            return HexagonState.Falling;
        }
    }
}