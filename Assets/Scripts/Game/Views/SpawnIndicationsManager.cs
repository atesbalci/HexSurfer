using System.Collections.Generic;
using Game.Engine;
using Game.Models;
using Game.Utility;
using UniRx;
using UnityEngine;

namespace Game.Views
{
    public class SpawnIndicationsManager : MonoBehaviour
    {
        public PeriodicRiser PlayerSpawnIndicatorPrefab;

        private List<GameObject> _spawnIndicators;

        private void Start()
        {
            var engine = FindObjectOfType<EngineManager>();
            MessageManager.ReceiveEvent<StateChangeEvent>().Subscribe(ev =>
            {
                if (ev.State == GameState.Pre)
                {
                    _spawnIndicators = new List<GameObject>();
                    foreach (var player in engine.GameManager.Players)
                    {
                        var ind = Instantiate(PlayerSpawnIndicatorPrefab, engine.SpawnPoints[player.Id], Quaternion.identity);
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
    }
}
