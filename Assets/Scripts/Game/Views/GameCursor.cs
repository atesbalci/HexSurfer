using System.Linq;
using Game.Engine;
using Game.Models;
using Game.Utility;
using UniRx;
using UnityEngine;

namespace Game.Views
{
    public class GameCursor : MonoBehaviour
    {
        private EngineManager _engine;
        private Transform _trackedTransform;
        private Material _mat;
        private Plane _plane;

        private void Start()
        {
            _plane = new Plane(Vector3.up, Vector3.up * 1f);
            _engine = FindObjectOfType<EngineManager>();
            _mat = GetComponentInChildren<Renderer>().material;
            MessageManager.ReceiveEvent<StateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.Pre || ev.State == GameState.Playing)
                {
                    var mouseInputPlayer = _engine.Players.FirstOrDefault(x => x.Input.ControlInputType == ControlInputType.Mouse);
                    if (mouseInputPlayer)
                    {
                        _trackedTransform = mouseInputPlayer.transform;
                        _mat.color = Player.Colors[mouseInputPlayer.Id];
                        gameObject.SetActive(true);
                        return;
                    }
                }
                gameObject.SetActive(false);
            });
        }

        private void Update()
        {
            if (_engine.GameManager == null)
            {
                gameObject.SetActive(false);
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float dist;
            if (_plane.Raycast(ray, out dist))
            {
                var pos = ray.GetPoint(dist);
                transform.position = pos;
                var look = pos - _trackedTransform.position;
                look.y = 0f;
                transform.rotation = Quaternion.LookRotation(look, Vector3.up);
            }
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Cursor.visible = true;
        }
    }
}
