using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CharacterDisplay : MonoBehaviour
    {
        public Character Character;
        public Image Energy;

        private void Update()
        {
            transform.rotation = Quaternion.identity;
            Energy.fillAmount = Character.Energy.Value * 0.5f;
        }
    }
}
