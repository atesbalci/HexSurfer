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

        private GameManager _gameManager;
        private List<Player> _players;

        private void Awake()
        {
            Initialize();
            MessageManager.ReceiveEvent<PlayersDefeatedEvent>().Subscribe(ev =>
            {
                foreach (var id in ev.Ids)
                {
                    if (_players[id])
                    {
                        var ps = Instantiate(DeathPrefab, _players[id].transform.position, _players[id].transform.rotation).GetComponentInChildren<ParticleSystem>();
                        var main = ps.main;
                        main.startColor = Player.Colors[id];
                        
                        Destroy(_players[id].gameObject);
                    }
                }
            });
        }

        public void Initialize()
        {
            _gameManager = new GameManager();
            _players = new List<Player>();
            foreach (var spawnPoint in SpawnPoints)
            {
                var player = Instantiate(CharacterPrefab,
                    new Vector3(spawnPoint.x, CharacterPrefab.transform.position.y, spawnPoint.y),
                    CharacterPrefab.transform.rotation).GetComponent<Player>();
                player.Id = _players.Count;
                player.Hexagons = Hexagons;
                _players.Add(player);
            }
        }
    }
}
