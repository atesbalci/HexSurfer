using System;
using Game.Utility;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Character : MonoBehaviour
    {
        public static readonly Color[] Colors = { Color.red, Color.green };
        public const float Speed = 10;
        public const float BoostedSpeed = 15;
        public const float Acceleration = 10;

        public int Id;
        public Transform BoardPivot;
        public TrailRenderer Trail;
        public AnimationCurve JumpCurve;
        public float JumpDuration;

        public Energy Energy { get; set; }

        [Space(10)]
        public HexagonTiler Hexagons;

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
                            Source = Id
                        };
                        Hexagons.AddHexRiser(_currentRiser);
                    }
                }
                _currentHexagon = value;
            }
        }

        private float _defaultY;
        private Hexagon _currentHexagon;
        private HexRiser _currentRiser;
        private Plane _plane;
        private Material _trailMat;
        private float _jumpProgress;
        private float _curSpeed;

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
            var boosting = false;
            if (Input.GetKey(_left) || Input.GetKey(_right))
            {
                float hitDist;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                _plane.Raycast(ray, out hitDist);
                targetRot += Vector3.up * (60 * (Input.GetKey(_right) ? 1 : -1));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRot), Time.deltaTime * 200);
            }
            if (_jumpProgress < JumpDuration)
            {
                _jumpProgress += Time.deltaTime;
                var y = _defaultY + JumpCurve.Evaluate(_jumpProgress / JumpDuration);
                if (_jumpProgress >= JumpDuration)
                {
                    y = _defaultY;
                }
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
            }
            else if (Input.GetKeyDown(_jump) && Energy.Value >= 1f)
            {
                _jumpProgress = 0;
                Energy.Value = 0;
            }
            else if (Input.GetKey(_boost) && Energy.Value >= 0f)
            {
                Energy.Value -= Time.deltaTime;
                boosting = true;
            }
            _curSpeed = Mathf.MoveTowards(_curSpeed, boosting ? BoostedSpeed : Speed, Acceleration * Time.deltaTime);
            transform.position += transform.forward * Time.deltaTime * _curSpeed;
            var targetRotY = targetRot.y;
            var curRotY = transform.eulerAngles.y;
            Quaternion modelTargetRot;
            var speed = 5f;
            if (targetRotY - curRotY < -0.1f && _jumpProgress >= JumpDuration)
            {
                modelTargetRot = Quaternion.Euler(0, 0, 60);
            }
            else if (targetRotY - curRotY > 0.1f && _jumpProgress >= JumpDuration)
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
                CurrentHexagon = _jumpProgress >= JumpDuration ? hit.transform.GetComponent<Hexagon>() : null;
            }
            var trailColor = _trailMat.GetColor("_TintColor");
            _trailMat.SetColor("_TintColor", new Color(trailColor.r, trailColor.g, trailColor.b, Mathf.Lerp(trailColor.a, 
                _jumpProgress >= JumpDuration ? boosting ? 1 : 0 : 1, Time.deltaTime * 20)));
        }
    }
}
