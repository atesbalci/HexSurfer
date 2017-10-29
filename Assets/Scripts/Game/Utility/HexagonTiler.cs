using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Utility
{
    public class HexRiser
    {
        public bool Active { get; set; }
        public Vector3 Location { get; set; }
        public int Source { get; set; }
        public float Radius { get; set; }
        public AnimationCurve RiserCurve { get; set; }

        public HexRiser()
        {
            Active = true;
            Source = -1;
        }
    }

    public class HexagonTiler : MonoBehaviour
    {
        public const float NoiseHeight = 100f;

        public GameObject HexPrefab;
        public float HexSpacing;
        public int Width;
        public int Height;

        public float CloseHeight;
        public float FarHeight;

        private Hexagon[] _hexagons;
        private List<HexRiser> _risers;
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
                }
            }
            _hexagons = hexagons.ToArray();
            _risers = new List<HexRiser>();
        }

        public void AddHexRiser(HexRiser riser)
        {
            _risers.Add(riser);
        }

        private void Update()
        {
            for (var i = 0; i < _risers.Count; i++)
            {
                if (!_risers[i].Active)
                {
                    _risers.RemoveAt(i);
                    i--;
                }
            }
            _seed += Time.deltaTime;
            foreach (var hex in _hexagons)
            {
                hex.Refresh();
                hex.TargetHeight = FarHeight;
                hex.State = HexagonState.Falling;
                foreach (var riser in _risers)
                {
                    var dist = Vector3.Distance(hex.Position, riser.Location);
                    hex.TargetHeight = Mathf.Max(hex.TargetHeight,
                        Mathf.Lerp(FarHeight, CloseHeight, riser.RiserCurve.Evaluate(1 - dist / riser.Radius)));
                    if (dist < riser.Radius)
                    {
                        hex.CurrentSource = riser.Source;
                        hex.State = HexagonState.Rising;
                        hex.TargetHeight += NoiseHeight / 2;
                    }
                }
                if (hex.State != HexagonState.Rising)
                {
                    hex.TargetHeight = Mathf.Max(FarHeight, FarHeight + Mathf.PerlinNoise(_seed + hex.Position.x / 10, _seed + hex.Position.z / 10) * NoiseHeight);
                }
                hex.Refresh();
            }
        }
    }
}