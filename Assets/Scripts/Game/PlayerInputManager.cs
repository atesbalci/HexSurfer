using UnityEngine;

namespace Game
{
    public enum ControlInputType
    {
        None = 0,
        Mouse,
        Keyboard1,
        Keyboard2
    }

    public class PlayerInputManager : MonoBehaviour
    {
        public ControlInputType ControlInputType { get; private set; }

        public float Axis { get; set; }
        public bool Jump { get; set; }
        public bool Boost { get; set; }

        private Plane _plane;

        public void Init(ControlInputType controlInputType)
        {
            ControlInputType = controlInputType;
            _plane = new Plane(Vector3.up, transform.position);
        }

        public void Refresh()
        {
            switch (ControlInputType)
            {
                case ControlInputType.Mouse:
                    Boost = Input.GetKey(KeyCode.Mouse0);
                    Jump = Input.GetKeyDown(KeyCode.Mouse1);
                    Axis = 0f;
                    float dist;
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (_plane.Raycast(ray, out dist))
                    {
                        var point = ray.GetPoint(dist);
                        if (Vector3.Distance(transform.position, point) > 0.01f)
                        {
                            var angle = Vector3.Angle(transform.forward, point - transform.position);
                            if (angle > 5)
                            {
                                Axis = Vector3.Cross(transform.forward, point - transform.position).y > 0f ? 1f : -1f;
                            }
                        }
                    }
                    break;
                case ControlInputType.Keyboard1:
                    Axis = 0f + (Input.GetKey(KeyCode.A) ? -1f : 0f) + (Input.GetKey(KeyCode.D) ? 1f : 0f);
                    Boost = Input.GetKey(KeyCode.LeftShift);
                    Jump = Input.GetKeyDown(KeyCode.Space);
                    break;
                case ControlInputType.Keyboard2:
                    Axis = 0f + (Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f) + (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f);
                    Boost = Input.GetKey(KeyCode.RightShift);
                    Jump = Input.GetKeyDown(KeyCode.RightControl);
                    break;
            }
        }
    }
}
