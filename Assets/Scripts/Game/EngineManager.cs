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

        public ReactiveDictionary<int,Player> Players { get; set; }
        public GameManager GameManager { get; set; }

        private void Awake()
        {
            Initialize();
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
            GameManager = new GameManager();
            Players = new ReactiveDictionary<int, Player>();
        }

        public void DefeatPlayer(int id)
        {
            var ps = Instantiate(DeathPrefab, Players[id].transform.position,
                Players[id].transform.rotation).GetComponentInChildren<ParticleSystem>();
            var main = ps.main;
            main.startColor = Player.Colors[id];
            Players[id].gameObject.SetActive(false);
        }
    }
}
