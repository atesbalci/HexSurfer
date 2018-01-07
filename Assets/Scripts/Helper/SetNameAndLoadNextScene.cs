using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helper
{
    public class SetNameAndLoadNextScene : MonoBehaviour
    {
        public TMP_InputField Input;

        public void Execute()
        {
            PhotonNetwork.playerName = Input.text;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
