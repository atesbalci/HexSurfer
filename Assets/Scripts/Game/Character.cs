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
                    _currentRiser = new HexRiser
                    {
                        Location = value.Position,
                        Source = Id
                    };
                    Hexagons.AddHexRiser(_currentRiser);
                }
                _currentHexagon = value;
            }
        }
        private Hexagon _currentHexagon;
        private HexRiser _currentRiser;
        private Plane _plane;

        private void Start()
        {
            _plane = new Plane(Vector3.up, transform.position);
        }

        private void Update()
        {
            var targetRot = transform.rotation;
            if (Input.GetKey(KeyCode.Mouse0))
            {
                float hitDist;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                _plane.Raycast(ray, out hitDist);
                targetRot = Quaternion.LookRotation(ray.GetPoint(hitDist) - transform.position, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Time.deltaTime * 200);
            }
            var targetRotY = targetRot.eulerAngles.y;
            var curRotY = transform.eulerAngles.y;
            Quaternion modelTargetRot;
            var speed = 5f;
            if (targetRotY - curRotY < -0.1f)
            {
                modelTargetRot = Quaternion.Euler(0, 0, 60);
            }
            else if (targetRotY - curRotY > 0.1f)
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
                CurrentHexagon = hit.transform.GetComponent<Hexagon>();
            }
            if (Input.GetKey(KeyCode.Mouse0))
            {
                transform.position += transform.forward * Time.deltaTime * 10;
            }
        }
    }
}
