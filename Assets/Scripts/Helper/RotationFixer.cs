using UnityEngine;

namespace Helper
{
    public class RotationFixer : MonoBehaviour
    {
        private void Update()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}
