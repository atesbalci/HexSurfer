using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CharacterDisplay : MonoBehaviour
    {
        public Player Player;
        public Image Energy;

        private void Update()
        {
            transform.rotation = Quaternion.identity;
            Energy.fillAmount = Player.Energy.Value * 0.5f;
        }
    }
}
