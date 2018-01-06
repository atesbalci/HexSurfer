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
        public Player[] Players;
        public PeriodicRiser PlayerSpawnIndicatorPrefab;

        [Space(10)]
        public GameObject DeathPrefab;

        public GameManager GameManager { get; set; }

        private List<GameObject> _spawnIndicators;

        private void OnDrawGizmosSelected()
        {
            foreach (var spawnPoint in SpawnPoints)
            {
                Gizmos.DrawSphere(new Vector3(spawnPoint.x, 0, spawnPoint.y), 1);
            }
        }

        public void Initialize()
        {
            GameManager = new GameManager();
            for (var i = 0; i < Players.Length; i++)
            {
                var player = Players[i];
                player.Id = i;
            }

            MessageManager.ReceiveEvent<StateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.Pre)
                {
                    _spawnIndicators = new List<GameObject>();
                    foreach (var player in GameManager.Players)
                    {
                        var ind = Instantiate(PlayerSpawnIndicatorPrefab, SpawnPoints[player.Id], Quaternion.identity);
                        ind.Id = player.Id;
                        _spawnIndicators.Add(ind.gameObject);
                    }
                }
                else
                {
                    if (_spawnIndicators != null)
                    {
                        foreach (var spawnIndicator in _spawnIndicators)
                        {
                            Destroy(spawnIndicator);
                        }
                        _spawnIndicators = null;
                    }
                }
            });
        }

        public void DefeatPlayer(int id, int order)
        {
            var ps = Instantiate(DeathPrefab, Players[id].transform.position,
                Players[id].transform.rotation).GetComponentInChildren<ParticleSystem>();
            var main = ps.main;
            main.startColor = Player.Colors[id];
            Players[id].gameObject.SetActive(false);
            GameManager.DefeatPlayer(id, order);
        }

        public void PlayerLeft(int id)
        {
            Players[id].gameObject.SetActive(false);
            GameManager.Players.RemoveAll(x => x.Id == id);
        }
    }
}
