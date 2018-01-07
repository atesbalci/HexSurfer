using Game.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public class CharacterDisplayView : MonoBehaviour
    {
        public Player Player;
        public Image Energy;

        private void Update()
        {
            transform.rotation = Quaternion.identity;
            if(Player.Energy != null)
                Energy.fillAmount = Player.Energy.Value * 0.5f;
        }
    }
}
