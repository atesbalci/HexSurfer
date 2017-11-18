using Photon;

namespace Game.Networking
{
    public class PlayerNetworkSerializer : PunBehaviour
    {
        private Player _player;
        private PlayerInputManager _playerInputManager;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _player = GetComponent<Player>();
            _playerInputManager = GetComponent<PlayerInputManager>();
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(_playerInputManager.Axis);
                stream.SendNext(_playerInputManager.Boost);
                stream.SendNext(_player.JumpProgress);
            }
            else
            {
                _playerInputManager.Axis = (float)stream.ReceiveNext();
                _playerInputManager.Boost = (bool)stream.ReceiveNext();
                _player.JumpProgress = (float)stream.ReceiveNext();
            }
        }
    }
}