using System.Collections.Generic;
using UnityEngine;

namespace Game.Engine
{
    public class Shark : MonoBehaviour
    {
        public Transform SharkSpotsParent;

        private const float MinTime = 4;
        private const float MaxTime = 4;

        private List<Transform> _spots;
        private float _timer;
        private Animator _anim;

        private void Start()
        {
            _anim = GetComponent<Animator>();
            _spots = new List<Transform>();
            foreach (Transform t in SharkSpotsParent)
            {
                _spots.Add(t);
            }
            _timer = Random.Range(MinTime, MaxTime);
            _anim.Play(0, 0, 1f);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = Random.Range(MinTime, MaxTime);
                Appear();
            }
        }

        public void Appear()
        {
            var spot = _spots[Random.Range(0, _spots.Count - 1)];
            transform.position = spot.position;
            transform.rotation = spot.rotation;
            _anim.Play(0, 0, 0f);
        }
    }
}
