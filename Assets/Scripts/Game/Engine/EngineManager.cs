using System.Linq;
using Game.Models;
using Game.Utility;
using UniRx;
using UnityEngine;

namespace Game.Engine
{
    public class EngineManager : MonoBehaviour
    {
        public HexagonTiler Hexagons;
        public Vector2[] SpawnPoints;
        public Player[] Players;
        public Bounds Bounds;

        [Space(10)]
        public GameObject DeathPrefab;

        public GameManager GameManager { get; set; }

        private void OnDrawGizmosSelected()
        {
            foreach (var spawnPoint in SpawnPoints)
            {
                Gizmos.DrawSphere(new Vector3(spawnPoint.x, 0, spawnPoint.y), 1);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
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
                    for (var i = 0; i < Players.Length; i++)
                    {
                        var player = Players[i];
                        player.transform.position = new Vector3(SpawnPoints[i].x, player.transform.position.y, SpawnPoints[i].y);
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

        private void Update()
        {
            var defeateds = Players.Where(x => x.gameObject.activeInHierarchy && !Bounds.Contains(x.transform.position)).Select(x => x.Id).ToList();
            if (defeateds.Count > 0)
            {
                MessageManager.SendEvent(new PlayersDefeatedEvent { Ids = defeateds });
            }
        }
    }
}
