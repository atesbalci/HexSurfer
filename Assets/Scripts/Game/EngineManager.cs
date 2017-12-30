using System.Linq;
using Game.Utility;
using UnityEngine;

namespace Game
{
    public class EngineManager : MonoBehaviour
    {
        public HexagonTiler Hexagons;
        public Vector2[] SpawnPoints;
        public Player[] Players;

        [Space(10)]
        public GameObject DeathPrefab;

        public GameManager GameManager { get; set; }

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
