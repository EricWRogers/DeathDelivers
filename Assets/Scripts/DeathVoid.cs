using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathVoid : MonoBehaviour
{
        private void OnCollisionEnter(Collision collision)
    {
        SceneManager.LoadScene("Main Menu");
    }
}
