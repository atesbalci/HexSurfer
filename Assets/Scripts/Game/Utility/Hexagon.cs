using Game.Engine;
using Helper;
using UniRx;
using UnityEngine;

namespace Game.Utility
{
    public enum HexagonState
    {
        Idle, Falling, Locked
    }
    public class HexOverlapEvent : GameEvent
    {
        public int Overlapper { get; set; }
        public int Overlappee { get; set; }
        public float TimeGap { get; set; }
    }
    public class Hexagon : MonoBehaviour
    {
        public float RiseSpeed;
        public float FallSpeed;
        public AnimationCurve ColorFadeCurve;
        public float FadeDuration;

        public float TargetHeight { get; set; }
        public Transform Trans { get; set; }
        public Vector3 Position { get; set; }
        public float DefaultHeight { get; set; }
        public Visibility Visible { get; set; }

        private float _currentHeight;
        private int _currentSource;
        private Renderer _rend;
        private MaterialPropertyBlock _prop;
        private Color _defCol;
        private Color _col;
        private HexagonState _state;
        private float _fade;
        private float _timeSinceSwitch;
        private bool _noFade;

        public HexagonState State
        {
            get { return _state; }
            set
            {
                if(State != value)
                {
                    if (value == HexagonState.Idle)
                    {
                        _prop.Clear();
                        _rend.SetPropertyBlock(_prop);
                    }
                }
                _state = value;
            }
        }

        public int CurrentSource
        {
            get { return _currentSource; }
            set
            {
                if (value != CurrentSource)
                {
                    if(value > -1)
                    {
                        if (CurrentSource > -1)
                        {
                            MessageManager.SendEvent(new HexOverlapEvent
                            {
                                Overlappee = CurrentSource,
                                Overlapper = value,
                                TimeGap = _timeSinceSwitch
                            });
                            return;
                        }
                        else
                        {
                            _col = value < Player.Colors.Length ? Player.Colors[value] : new Color(1f, 0.78f, 0.49f);
                            _prop.SetColor("_Color", _col);
                            _rend.SetPropertyBlock(_prop);
                            _noFade = value >= Player.Colors.Length;
                            _fade = 0;
                            _timeSinceSwitch = 0f;
                        }
                    }
                }
                _currentSource = value;
            }
        }

        public void Init()
        {
            Visible = GetComponentInChildren<Visibility>();
            _rend = GetComponentInChildren<Renderer>();
            Trans = _rend.transform;
            TargetHeight = Trans.localScale.y;
            _currentHeight = Trans.localScale.y;
            Position = Trans.position;
            _currentSource = -1;
            _prop = new MaterialPropertyBlock();
            _defCol = _rend.sharedMaterial.color;
            State = HexagonState.Falling;
            _fade = FadeDuration;
            _noFade = false;
        }

        public void Refresh()
        {
            if (State == HexagonState.Locked)
            {
                return;
            }
            if (State != HexagonState.Idle)
            {
                _timeSinceSwitch += Time.deltaTime;
                var scale = Trans.localScale;
                _currentHeight = Mathf.MoveTowards(scale.y, TargetHeight, Time.deltaTime * (_currentHeight > TargetHeight ? FallSpeed : RiseSpeed));
                Trans.localScale = new Vector3(scale.x, _currentHeight, scale.z);
                if (State == HexagonState.Falling && 
                    Mathf.Abs(TargetHeight - _currentHeight) < 1)
                {
                    State = HexagonState.Idle;
                    CurrentSource = -1;
                }
            }
            //Color related update
            if (!_noFade && _fade < FadeDuration)
            {
                _fade += Time.deltaTime;
                _col = Color.Lerp(_col, _defCol, ColorFadeCurve.Evaluate(_fade / FadeDuration));
                _prop.SetColor("_Color", _col);
                if (_fade >= FadeDuration)
                {
                    _col = _defCol;
                    _prop.Clear();
                }
                _rend.SetPropertyBlock(_prop);
            }
        }
    }
}