using UnityEngine;

namespace Helper
{
    public class Visibility : MonoBehaviour
    {
        public bool Visible { get; set; }

        private void Start()
        {
            Visible = GetComponent<Renderer>().isVisible;
        }

        private void OnBecameInvisible()
        {
            Visible = false;
        }

        private void OnBecameVisible()
        {
            Visible = true;
        }
    }
}
