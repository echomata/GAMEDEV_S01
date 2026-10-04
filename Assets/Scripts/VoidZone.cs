using UnityEngine;
using UnityEngine.SceneManagement;

public class VoidZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null || other.CompareTag("Player"))
        {
            Debug.Log("YOU FELL!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}