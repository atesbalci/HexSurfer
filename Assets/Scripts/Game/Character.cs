using System;
using Game.Utility;
using UniRx;
using UnityEngine;

namespace Game
{
    public class Character : MonoBehaviour
    {
        public static readonly Color[] Colors = { Color.red, Color.green };

        public int Id;
        public Transform BoardPivot;
        public TrailRenderer Trail;

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

        public bool MidAir { get; set; }

        private float _defaultY;
        private Rigidbody _body;
        private Hexagon _currentHexagon;
        private HexRiser _currentRiser;
        private Plane _plane;
        private Material _trailMat;

        private KeyCode _left;
        private KeyCode _right;
        private KeyCode _jump;

        private void Start()
        {
            _plane = new Plane(Vector3.up, transform.position);
            if (Id == 0)
            {
                _left = KeyCode.A;
                _right = KeyCode.D;
                _jump = KeyCode.Space;
            }
            else
            {
                _left = KeyCode.LeftArrow;
                _right = KeyCode.RightArrow;
                _jump = KeyCode.RightControl;
            }
            MidAir = false;
            _defaultY = transform.position.y;
            _body = GetComponent<Rigidbody>();
            _trailMat = Trail.material;
        }

        private void Update()
        {
            var targetRot = transform.eulerAngles;
            if (Input.GetKey(_left) || Input.GetKey(_right))
            {
                float hitDist;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                _plane.Raycast(ray, out hitDist);
                targetRot += Vector3.up * (60 * (Input.GetKey(_right) ? 1 : -1));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRot), Time.deltaTime * 200);
            }
            var pos = transform.position;
            pos.y = _defaultY;
            if (transform.position.y - 0.2f < _defaultY && _body.velocity.y <= 0)
            {
                _body.velocity = Vector3.zero;
                MidAir = false;
            }
            transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * 10);
            if (Input.GetKeyDown(_jump) && !MidAir)
            {
                _body.velocity += Vector3.up * 15;
                MidAir = true;
            }
            transform.position += transform.forward * Time.deltaTime * 10;
            var targetRotY = targetRot.y;
            var curRotY = transform.eulerAngles.y;
            Quaternion modelTargetRot;
            var speed = 5f;
            if (targetRotY - curRotY < -0.1f && !MidAir)
            {
                modelTargetRot = Quaternion.Euler(0, 0, 60);
            }
            else if (targetRotY - curRotY > 0.1f && !MidAir)
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
                CurrentHexagon = !MidAir ? hit.transform.GetComponent<Hexagon>() : null;
            }
            var trailColor = _trailMat.GetColor("_TintColor");
            _trailMat.SetColor("_TintColor", new Color(trailColor.r, trailColor.g, trailColor.b, Mathf.Lerp(trailColor.a, MidAir ? 1 : 0, Time.deltaTime * 20)));
        }
    }
}
