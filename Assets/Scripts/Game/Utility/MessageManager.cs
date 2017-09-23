using UniRx;

namespace Game.Utility
{
    public class GameEvent { }

    public class MessageManager
    {
        public static void SendEvent<T>(T ev) where T : GameEvent
        {
            MessageBroker.Default.Publish(ev);
        }

        public static IObservable<T> ReceiveEvent<T>() where T : GameEvent
        {
            return MessageBroker.Default.Receive<T>();
        }
    }
}
