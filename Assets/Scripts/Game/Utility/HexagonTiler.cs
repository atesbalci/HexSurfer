using System.Collections.Generic;
using UnityEngine;

namespace Game.Utility
{
    public class HexRiser
    {
        public bool Active { get; set; }
        public Vector3 Location { get; set; }
        public int Source { get; set; }

        public HexRiser()
        {
            Active = true;
            Source = -1;
        }
    }

    public class HexagonTiler : MonoBehaviour
    {
        public GameObject HexPrefab;
        public float HexSpacing;
        public int Width;
        public int Height;

        public AnimationCurve HeightCurve;
        public float Radius;
        public float CloseHeight;
        public float FarHeight;

        private Hexagon[] _hexagons;
        private List<HexRiser> _risers;

        private void Start()
        {
            RefreshHexagons();
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
                    curx += (3 * HexSpacing);
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
                var heightMult = (i / 2) + 1;
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
            for(var i = 0; i < _risers.Count; i++)
            {
                if (!_risers[i].Active)
                {
                    _risers.RemoveAt(i);
                    i--;
                }
            }
            foreach (var hex in _hexagons)
            {
                hex.Refresh();
                hex.TargetHeight = FarHeight;
                hex.State = HexagonState.Falling;
                foreach (var riser in _risers)
                {
                    var dist = DistanceSquare(hex.Position, riser.Location);
                    hex.TargetHeight = Mathf.Max(hex.TargetHeight,
                        Mathf.Lerp(CloseHeight, FarHeight, HeightCurve.Evaluate(Mathf.Min(1, dist / Radius))));
                    if (dist < Radius)
                    {
                        hex.CurrentSource = riser.Source;
                        hex.State = HexagonState.Rising;
                    }
                }
                hex.Refresh();
            }
        }

        private float DistanceSquare(Vector3 a, Vector3 b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
        }
    }
}