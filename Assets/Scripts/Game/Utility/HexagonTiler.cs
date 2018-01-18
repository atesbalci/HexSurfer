using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Utility
{
    public class HexagonTiler : MonoBehaviour
    {
        public const float NoiseHeight = 75f;

        public GameObject HexPrefab;
        public float HexSpacing;
        public int Width;
        public int Height;

        public float CloseHeight;
        public float FarHeight;

        public Dictionary<Collider, Hexagon> ColliderCache { get; private set; }

        private Hexagon[] _hexagons;
        private float _seed;

        private void Awake()
        {
            RefreshHexagons();
            _seed = Random.Range(0f, 10000f);
        }

        private Vector3 DrawSize
        {
            get
            {
                return new Vector3(3 * HexSpacing * Width, 1, Height * Mathf.Sin(Mathf.PI / 3) * HexSpacing);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position, DrawSize);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawCube(transform.position, DrawSize);
        }

        public void RefreshHexagons()
        {
            ColliderCache = new Dictionary<Collider, Hexagon>();
            var hexagons = new List<Hexagon>();
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
                Destroy(transform.GetChild(i).gameObject);
            var midLine = new List<Hexagon>();
            float curx = 0;
            for (var i = 0; i < Width; i++)
            {
                if (i % 2 == 1)
                    curx += 3 * HexSpacing;
                var newHex = Instantiate(HexPrefab).GetComponent<Hexagon>();
                newHex.transform.SetParent(gameObject.transform);
                newHex.transform.localPosition = new Vector3(i % 2 == 0 ? curx : -curx, 0, 0);
                newHex.gameObject.name = "Hex";
                midLine.Add(newHex);
                hexagons.Add(newHex);
                newHex.DefaultHeight = FarHeight;
                newHex.Init();
                ColliderCache.Add(newHex.GetComponent<Collider>(), newHex);
            }
            var xshift = Mathf.Cos(Mathf.PI / 3) * HexSpacing + HexSpacing;
            var zshift = Mathf.Sin(Mathf.PI / 3) * HexSpacing;
            for (var i = 0; i < Height; i++)
            {
                var heightMult = i / 2 + 1;
                foreach (var obj in midLine)
                {
                    var newHex = Instantiate(obj.gameObject).GetComponent<Hexagon>();
                    newHex.transform.SetParent(gameObject.transform);
                    newHex.transform.localPosition =
                        new Vector3(newHex.transform.localPosition.x, 0, newHex.transform.localPosition.z) +
                        new Vector3(i % 4 < 2 ? xshift : 0, 0, (i % 2 == 0 ? 1 : -1) * heightMult * zshift);
                    newHex.gameObject.name = "Hex";
                    hexagons.Add(newHex);
                    newHex.DefaultHeight = FarHeight;
                    newHex.Init();
                    ColliderCache.Add(newHex.GetComponent<Collider>(), newHex);
                }
            }
            _hexagons = hexagons.ToArray();
        }

        private void LateUpdate()
        {
            _seed += Time.deltaTime;
            foreach (var hex in _hexagons)
            {
                hex.Refresh();
                hex.TargetHeight = FarHeight;
                if (hex.State == HexagonState.Idle && hex.Visible.Visible)
                {
                    var scale = hex.Trans.localScale;
                    scale.y = Mathf.Max(FarHeight, FarHeight + Mathf.PerlinNoise(_seed + hex.Position.x / 10, _seed + hex.Position.z / 10) * NoiseHeight);
                    hex.Trans.localScale = scale;
                }
            }
        }

        //private static float DistanceSquared(Vector2 a, Vector2 b)
        //{
        //    return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        //}

        public void RadialRise(Vector3 pos, float radius, AnimationCurve curve, int source, bool locked = false)
        {
            var matchingHexagons = Physics.SphereCastAll(pos + Vector3.up * 10f, radius, Vector3.down, 10f + radius,
                    1 << gameObject.layer)
                .Select(x => ColliderCache[x.collider]);
            foreach (var hex in matchingHexagons)
            {
                var dist = Vector3.Distance(pos, hex.Trans.position);
                if (dist <= radius)
                {
                    hex.TargetHeight = Mathf.Max(hex.TargetHeight,
                        Mathf.Lerp(FarHeight, CloseHeight, curve.Evaluate(1 - dist / radius)));
                    hex.CurrentSource = source;
                    hex.State = hex.State == HexagonState.Locked ? HexagonState.Locked : HexagonState.Falling;
                    if (locked)
                    {
                        hex.State = HexagonState.Locked;
                        hex.Trans.localScale = new Vector3(hex.Trans.localScale.x, hex.TargetHeight, hex.Trans.localScale.z);
                    }
                }
            }
        }
    }
}