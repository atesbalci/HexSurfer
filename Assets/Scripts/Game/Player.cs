using System;
using Game.Utility;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Player : MonoBehaviour
    {
        public static readonly Color[] Colors = { Color.red, Color.green };
        public const float Speed = 10;
        public const float BoostedSpeed = 15;
        public const float Acceleration = 5;

        public Transform BoardPivot;
        public TrailRenderer Trail;
        public AnimationCurve JumpCurve;
        public float JumpDuration;
        public AnimationCurve RiserCurve;
        public float Radius;

        public Energy Energy { get; set; }
        public int Id { get; set; }
        public HexagonTiler Hexagons { get; set; }

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
                        Hexagons.AddHexRiser(_currentRiser);
                    }
                }
                _currentHexagon = value;
            }
        }

        public bool Jumping
        {
            get
            {
                return _jumpProgress < JumpDuration;
            }
        }

        private float _defaultY;
        private Hexagon _currentHexagon;
        private HexRiser _currentRiser;
        private Plane _plane;
        private Material _trailMat;
        private float _jumpProgress;
        private float _curSpeed;
        private bool _boosting;

        private KeyCode _left;
        private KeyCode _right;
        private KeyCode _jump;
        private KeyCode _boost;

        private void Start()
        {
            _plane = new Plane(Vector3.up, transform.position);
            if (Id == 0)
            {
                _left = KeyCode.A;
                _right = KeyCode.D;
                _jump = KeyCode.Space;
                _boost = KeyCode.LeftShift;
            }
            else
            {
                _left = KeyCode.LeftArrow;
                _right = KeyCode.RightArrow;
                _jump = KeyCode.RightControl;
                _boost = KeyCode.RightShift;
            }
            _defaultY = transform.position.y;
            _trailMat = Trail.material;
            _jumpProgress = JumpDuration;
            _curSpeed = 0;
            Energy = new Energy();
        }

        private void Update()
        {
            Energy.Update(Time.deltaTime);
            var targetRot = transform.eulerAngles;
            if (Input.GetKey(_left) || Input.GetKey(_right))
            {
                float hitDist;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                _plane.Raycast(ray, out hitDist);
                targetRot += Vector3.up * (60 * (Input.GetKey(_right) ? 1 : -1));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRot), Time.deltaTime * 200);
            }

            if (Jumping)
            {
                _jumpProgress += Time.deltaTime;
                var y = _defaultY + JumpCurve.Evaluate(_jumpProgress / JumpDuration);
                if (!Jumping)
                {
                    y = _defaultY;
                }
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
            }
            else if (Input.GetKeyDown(_jump) && Energy.Value > 0.99f)
            {
                _jumpProgress = 0;
                Energy.Value = 0;
            }

            if (!Jumping && Input.GetKeyDown(_boost) && Energy.Value > 0.2f)
            {
                _boosting = true;
            }
            if (!Jumping && _boosting && Input.GetKey(_boost) && Energy.Value > 0f)
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
                modelTargetRot = Quaternion.Euler(0, 0, 60);
            }
            else if (targetRotY - curRotY > 0.1f && !Jumping)
            {
                modelTargetRot = Quaternion.Euler(0, 0, -60);
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
        }

        private void OnDestroy()
        {
            _currentRiser.Active = false;
        }
    }
}
