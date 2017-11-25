using System;
using Game.Utility;
using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class Player : MonoBehaviour
    {
        public static readonly Color[] Colors = { Color.red, Color.green };
        public const float Speed = 10;
        public const float BoostedSpeed = 15;
        public const float Acceleration = 5;
        public const float LeaningAngle = 30;
        public const float TurnSpeed = 60f;

        public Transform BoardPivot;
        public Renderer BoardRenderer;
        public TrailRenderer Trail;
        public AnimationCurve JumpCurve;
        public float JumpDuration;
        public AnimationCurve RiserCurve;
        public float Radius;
        public GameObject JumpMarker;

        public Energy Energy { get; set; }
        public int Id { get; set; }
        public float JumpProgress { get; set; }

        public Hexagon CurrentHexagon
        {
            get { return _currentHexagon; }
            set
            {
                if (value != CurrentHexagon)
                {
                    if (CurrentHexagon)
                    {
                        var riser = _currentRiser;
                        Observable.Timer(TimeSpan.FromSeconds(0)).Subscribe(lng =>
                        {
                            riser.Active = false;
                        });
                    }
                    if (value)
                    {
                        _currentRiser = new HexRiser
                        {
                            Location = value.Position,
                            Source = Id,
                            RiserCurve = RiserCurve,
                            Radius = Radius
                        };
                        _hexagons.AddHexRiser(_currentRiser);
                    }
                }
                _currentHexagon = value;
            }
        }

        public bool Jumping
        {
            get
            {
                return JumpProgress < JumpDuration;
            }
        }

        private HexagonTiler _hexagons;
        private float _defaultY;
        private Hexagon _currentHexagon;
        private HexRiser _currentRiser;
        private Material _trailMat;
        private float _curSpeed;
        private bool _boosting;
        private PlayerInputManager _input;
        private bool _initialized;

        public void Init(int id, bool isMine)
        {
            _initialized = true;
            Id = id;
            _input = GetComponent<PlayerInputManager>();
            _input.Init(isMine ? ControlInputType.Mouse : ControlInputType.None);
            _defaultY = transform.position.y;
            _trailMat = Trail.material;
            JumpProgress = JumpDuration;
            _curSpeed = 0;
            Energy = new Energy();
            BoardRenderer.material.color = Colors[Id];
            _hexagons = FindObjectOfType<HexagonTiler>();
        }

        private void Update()
        {
            if (!_initialized)
                return;
            _input.Refresh();
            Energy.Update(Time.deltaTime);
            var targetRot = transform.eulerAngles;
            if (Mathf.Abs(_input.Axis) > 0.01f)
            {
                targetRot += Vector3.up * (TurnSpeed * _input.Axis);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRot), Time.deltaTime * 200);
            }

            if (Jumping)
            {
                JumpProgress += Time.deltaTime;
                var y = _defaultY + JumpCurve.Evaluate(JumpProgress / JumpDuration);
                if (!Jumping)
                {
                    y = _defaultY;
                }
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
            }
            else if (_input.Jump && Energy.Value > 0.99f)
            {
                JumpProgress = 0;
                Energy.Value = 0;
            }

            if (!Jumping && _input.Boost && Energy.Value > 0.2f)
            {
                _boosting = true;
            }
            if (!Jumping && _boosting && _input.Boost && Energy.Value > 0f)
            {
                Energy.Value -= Time.deltaTime;
                _boosting = true;
            }
            else
            {
                _boosting = false;
            }

            _curSpeed = Mathf.MoveTowards(_curSpeed, _boosting ? BoostedSpeed : Speed, Acceleration * Time.deltaTime);
            transform.position += transform.forward * Time.deltaTime * _curSpeed;
            var targetRotY = targetRot.y;
            var curRotY = transform.eulerAngles.y;
            Quaternion modelTargetRot;
            var speed = 5f;
            if (targetRotY - curRotY < -0.1f && !Jumping)
            {
                modelTargetRot = Quaternion.Euler(0, 0, LeaningAngle);
            }
            else if (targetRotY - curRotY > 0.1f && !Jumping)
            {
                modelTargetRot = Quaternion.Euler(0, 0, -LeaningAngle);
            }
            else
            {
                modelTargetRot = Quaternion.identity;
                speed = 2f;
            }
            BoardPivot.localRotation = Quaternion.Lerp(BoardPivot.localRotation, modelTargetRot, Time.deltaTime * speed);
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, 100f,
                LayerMask.GetMask("Hexagon")))
            {
                CurrentHexagon = !Jumping ? hit.transform.GetComponent<Hexagon>() : null;
            }
            var trailColor = _trailMat.GetColor("_TintColor");
            _trailMat.SetColor("_TintColor", new Color(trailColor.r, trailColor.g, trailColor.b, Mathf.Lerp(trailColor.a, 
                !Jumping ? _boosting ? 1 : 0 : 1, Time.deltaTime * 20)));
            JumpMarker.SetActive(Jumping);
        }

        private void OnDisable()
        {
            if (_currentRiser != null)
                _currentRiser.Active = false;
        }

        private void OnDestroy()
        {
            if(_currentRiser != null)
                _currentRiser.Active = false;
        }
    }
}
