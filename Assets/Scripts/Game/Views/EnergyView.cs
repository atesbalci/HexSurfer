using System.Linq;
using Game.Engine;
using UnityEngine;

namespace Game.Views
{
    public class EnergyView : MonoBehaviour
    {
        public Transform[] Indicators;

        private Vector3[] _defaultScales;
        private Material[] _materials;
        private float[] _defaultOutlineWidths;
        private Player _player;

        private void Awake()
        {
            _player = GetComponentInParent<Player>();
            _defaultScales = Indicators.Select(x => x.localScale).ToArray();
            _materials = Indicators.Select(x => x.GetComponent<Renderer>().material).ToArray();
            _defaultOutlineWidths = _materials.Select(x => x.GetFloat("_Outline")).ToArray();
        }

        private void OnEnable()
        {
            foreach (var mat in _materials)
            {
                mat.color = Player.Colors[_player.Id];
            }
        }

        private void Update()
        {
            if (!_player.Enabled)
                return;
            transform.rotation = Camera.main.transform.rotation;
            for (var i = 0; i < Indicators.Length; i++)
            {
                var indicator = Indicators[i];
                var ang = indicator.localEulerAngles;
                ang.y += Time.deltaTime * 90;
                if (ang.y >= 360f)
                    ang.y -= 360f;
                indicator.localEulerAngles = ang;
                var value = Mathf.Clamp(_player.Energy.Value * Indicators.Length - i, 0f, 1f);
                indicator.localScale = _defaultScales[i] * value;
                _materials[i].SetFloat("_Outline", _defaultOutlineWidths[i] * value);
            }
        }
    }
}
