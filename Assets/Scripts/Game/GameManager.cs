using System;
using System.Collections.Generic;
using System.Linq;
using Game.Utility;
using UniRx;

namespace Game
{
    public enum GameState
    {
        Idle, Pre, Playing, Post
    }

    public class PlayersDefeatedEvent : GameEvent
    {
        public List<int> Ids { get; set; }
    }

    public class PlayerInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StateChangeEvent : GameEvent
    {
        public GameState State { get; set; }
    }

    public class GameManager
    {
        public ReactiveCollection<PlayerInfo> Players { get; private set; }

        private readonly IDisposable[] _disps;
        private GameState _state;
        private float _time;

        public GameManager()
        {
            _disps = new []
            {
                MessageManager.ReceiveEvent<HexOverlapEvent>().Subscribe(ev =>
                {
                    var ids = new List<int> {ev.Overlapper};
                    if (ev.TimeGap < 0.1f)
                    {
                        ids.Add(ev.Overlappee);
                    }
                    MessageManager.SendEvent(new PlayersDefeatedEvent {Ids = ids});
                })
            };
            Players = new ReactiveCollection<PlayerInfo>();
        }

        ~GameManager()
        {
            foreach (var disp in _disps)
            {
                disp.Dispose();
            }
        }

        public void AddPlayer(int id, string name)
        {
            Players.Add(new PlayerInfo { Id = id, Name = name });
        }

        public int GetNextAvailableId()
        {
            var id = -1;
            for (var i = 0; i < 4; i++)
            {
                if (Players.All(x => x.Id != i))
                {
                    id = i;
                    break;
                }
            }
            return id;
        }

        public void Tick(float delta)
        {
            
        }

        public GameState State
        {
            get { return _state; }
            set
            {
                if (State == value)
                    return;
                _state = value;
                MessageManager.SendEvent(new StateChangeEvent { State = State });
            }
        }
    }
}
