using System.Collections.Generic;
using Game.Utility;
using UniRx;
using UnityEngine;

namespace Game
{
    public class EngineManager : MonoBehaviour
    {
        public HexagonTiler Hexagons;
        public Vector2[] SpawnPoints;

        [Space(10)]
        public GameObject CharacterPrefab;
        public GameObject DeathPrefab;

        public List<Player> Players { get; set; }

        private GameManager _gameManager;

        private void Awake()
        {
            Initialize();
            MessageManager.ReceiveEvent<PlayersDefeatedEvent>().Subscribe(ev =>
            {
                foreach (var id in ev.Ids)
                {
                    if (id < Players.Count && Players[id])
                    {
                        var ps = Instantiate(DeathPrefab, Players[id].transform.position, Players[id].transform.rotation).GetComponentInChildren<ParticleSystem>();
                        var main = ps.main;
                        main.startColor = Player.Colors[id];
                        
                        Destroy(Players[id].gameObject);
                    }
                }
            });
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var spawnPoint in SpawnPoints)
            {
                Gizmos.DrawSphere(new Vector3(spawnPoint.x, 0, spawnPoint.y), 1);
            }
        }

        public void Initialize()
        {
            _gameManager = new GameManager();
            Players = new List<Player>();
        }
    }
}
