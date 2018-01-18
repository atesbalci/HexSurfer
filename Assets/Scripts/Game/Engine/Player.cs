using System;
using Game.Models;
using Game.Utility;
using UnityEngine;

namespace Game.Engine
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class Player : MonoBehaviour
    {
        public static readonly Color[] Colors = { Color.red, Color.green, Color.magenta, new Color(1f, 0.36f, 0f),  };
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
        public Transform ManParent;

        public bool Enabled { get; private set; }
        public Energy Energy { get; set; }
        public float JumpProgress { get; set; }
        public int Id { get; set; }
        public PlayerInputManager Input { get; set; }
        public Action InitAction { get; set; }

        public bool Jumping { get { return JumpProgress < JumpDuration; } }

        private HexagonTiler _hexagons;
        private float _defaultY;
        private Hexagon _currentHexagon;
        private Material _trailMat;
        private float _curSpeed;
        private bool _boosting;
        private bool _initialized;

        private void Start()
        {
            _defaultY = transform.position.y;
            Energy = new Energy();
            SetEnabled(true);
            SetEnabled(false);
        }

        public void Init(bool isMine)
        {
            _initialized = true;
            Input = GetComponent<PlayerInputManager>();
            Input.Init(isMine ? ControlInputType.Mouse : ControlInputType.None);
            _trailMat = Trail.material;
            JumpProgress = JumpDuration;
            Energy.Reset();
            BoardRenderer.material.color = Colors[Id];
            _hexagons = FindObjectOfType<HexagonTiler>();
            foreach (var part in ManParent.GetComponentsInChildren<Renderer>())
            {
                part.material.color = Colors[Id];
            }
            InitAction();
        }

        public Hexagon CurrentHexagon
        {
            get { return _currentHexagon; }
            set
            {
                if (value)
                {
                    _hexagons.RadialRise(value.Position, Radius, RiserCurve, Id);
                }
                _currentHexagon = value;
            }
        }

        private void Update()
        {
            if (!Enabled || !_initialized)
                return;
            Input.Refresh();
            Energy.Update(Time.deltaTime);
            var targetRot = transform.eulerAngles;
            if (Mathf.Abs(Input.Axis) > 0.01f)
            {
                targetRot += Vector3.up * (TurnSpeed * Input.Axis);
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
            else if (Input.Jump && Energy.Value > 0.99f)
            {
                JumpProgress = 0;
                Energy.Value = 0;
            }

            if (!Jumping && Input.Boost && Energy.Value > 0.2f)
            {
                _boosting = true;
            }
            if (!Jumping && _boosting && Input.Boost && Energy.Value > 0f)
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
                CurrentHexagon = !Jumping ? _hexagons.ColliderCache[hit.collider] : null;
            }
            var trailColor = _trailMat.GetColor("_TintColor");
            _trailMat.SetColor("_TintColor", new Color(trailColor.r, trailColor.g, trailColor.b, Mathf.Lerp(trailColor.a, 
                !Jumping ? _boosting ? 1 : 0 : 1, Time.deltaTime * 20)));
            JumpMarker.SetActive(Jumping);
        }

        public void SetEnabled(bool b)
        {
            if(Enabled == b) 
                return;
            Enabled = b;
            if (Enabled)
            {
                _curSpeed = 0;
                if(Energy != null)
                    Energy.Reset();
                transform.position = new Vector3(transform.position.x, _defaultY, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(0, -1000, 0);
            }
        }
    }
}
