using System;
using System.Collections.Generic;
using System.Linq;
using Game.Utility;
using UniRx;

namespace Game.Models
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
        public int Score { get; set; }
        public bool Defeated { get; set; }
        public int DefeatOrder { get; set; }
    }

    public class StateChangeEvent : GameEvent
    {
        public GameState State { get; set; }
    }

    public class GameManager
    {
        public List<PlayerInfo> Players { get; private set; }
        public int RoundsPlayed { get; set; }

        private readonly IDisposable[] _disps;
        private GameState _state;

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
            Players = new List<PlayerInfo>();
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

        public void PlayerLeft(int id)
        {
            Players.RemoveAll(x => x.Id == id);
        }

        public void DefeatPlayer(int id, int order)
        {
            var pl = Players.FirstOrDefault(x => x.Id == id);
            if (pl != null)
            {
                pl.Defeated = true;
                pl.DefeatOrder = order;
                if (Players.Count(x => !x.Defeated) <= 1)
                {
                    var undefeated = Players.FirstOrDefault(x => !x.Defeated);
                    if (undefeated != null)
                    {
                        undefeated.Defeated = true;
                        undefeated.DefeatOrder = Players.Count - 1;
                    }
                    State = GameState.Post;
                }
            }
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
                switch (State)
                {
                    case GameState.Post:
                        RoundsPlayed++;
                        foreach (var player in Players)
                        {
                            player.Score += player.DefeatOrder * 100;
                        }
                        Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(l => State = GameState.Pre);
                        break;
                    case GameState.Playing:
                        foreach (var player in Players)
                        {
                            player.Defeated = false;
                        }
                        break;
                }
            }
        }
    }
}
