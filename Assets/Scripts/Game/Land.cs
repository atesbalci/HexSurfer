using Game.Utility;
using UnityEngine;

namespace Game
{
    public class Land : MonoBehaviour
    {
        public HexagonTiler Hexagons;
        public AnimationCurve RiseCurve;
        public float Radius;

        private void Start()
        {
            Hexagons.AddHexRiser(new HexRiser
            {
                 Source = 4,
                 RiserCurve = RiseCurve,
                 Location = transform.position,
                 Radius = Radius
            });
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.5f, 0.25f, 0f, 0.5f);
            Gizmos.DrawSphere(transform.position, Radius);
        }
    }
}
