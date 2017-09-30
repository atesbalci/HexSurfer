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
                 Source = 2,
                 RiserCurve = RiseCurve,
                 Location = transform.position,
                 Radius = Radius
            });
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.58f, 0.2f, 0f, 0.51f);
            Gizmos.DrawSphere(transform.position, Radius);
        }
    }
}
