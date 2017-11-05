using UnityEngine;

namespace Game.Utility
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineVertexPrefabSynchronizer : MonoBehaviour
    {
        public GameObject Prefab;
        public bool ExcludeFirst;
        public bool ExcludeLast;

        private LineRenderer _line;
        private Transform[] _pool;

        private void Start()
        {
            _line = GetComponent<LineRenderer>();
            _pool = new Transform[_line.positionCount];
            var parent = new GameObject("VertexParent").transform;
            parent.SetParent(transform);
            parent.localPosition = Vector3.zero;
            parent.localScale = Vector3.one;
            parent.localRotation = Quaternion.identity;
            for (var i = 0; i < _pool.Length; i++)
            {
                _pool[i] = Instantiate(Prefab, parent, false).transform;
            }
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            _pool[0].gameObject.SetActive(!ExcludeFirst);
            _pool[_pool.Length - 1].gameObject.SetActive(!ExcludeLast);
            for (var i = 0; i < _pool.Length; i++)
            {
                _pool[i].position = _line.GetPosition(i);
            }
        }
    }
}
