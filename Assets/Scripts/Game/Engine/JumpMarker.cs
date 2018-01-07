using System.Linq;
using Game.Utility;
using UnityEngine;

namespace Game.Engine
{
    public class JumpMarker : MonoBehaviour
    {
        public LineRenderer Line;
        public Renderer Projector;

        private LayerMask _mask;
        private int _id;
        private float _radius;

        private void Start()
        {
            var player = GetComponentInParent<Player>();
            Projector.transform.localScale = 2 * player.Radius * Vector3.one;
            _id = player.Id;
            _radius = player.Radius;
            _mask = LayerMask.GetMask("Hexagon");
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            var hits = Physics.SphereCastAll(new Ray(transform.position, Vector3.down), _radius, 100f, _mask);
            var danger = hits.Any(x =>
            {
                var source = x.transform.GetComponent<Hexagon>().CurrentSource;
                return source >= 0 && source != _id;
            });
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, 100f, _mask))
            {
                Line.SetPosition(1, hit.point - transform.position);
            }
            Projector.material.color = danger ? new Color(1f, 0f, 0f, 0.5f) : new Color(1f, 0.92f, 0.02f, 0.5f);
            var grad = new Gradient();
            grad.SetKeys(new [] { new GradientColorKey(danger ? Color.red : Color.yellow, 0f) }, new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 1f) });
            Line.colorGradient = grad;
        }
    }
}
