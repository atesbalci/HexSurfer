using System;
using System.Collections.Generic;
using Game.Utility;
using UniRx;

namespace Game
{
    public class PlayersDefeatedEvent : GameEvent
    {
        public List<int> Ids { get; set; }
    }

    public class GameManager
    {
        private readonly IDisposable[] _disps;

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
        }

        ~GameManager()
        {
            foreach (var disp in _disps)
            {
                disp.Dispose();
            }
        }
    }
}
