using UnityEngine;
using UnityEngine.SceneManagement;

public class VoidZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            Debug.Log("YOU FELL!");
            health.TakeDamage(999999f);
            return;
        }

        if (other.GetComponent<CharacterController>() != null || other.CompareTag("Player"))
        {
            Debug.Log("YOU FELL!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}