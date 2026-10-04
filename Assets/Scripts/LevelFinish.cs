using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    [Header("Finish Settings")]
    [SerializeField] private float resetDelay = 5f;
    [SerializeField] private ParticleSystem winParticles;

    private bool hasWon = false;

    private void OnTriggerEnter(Collider other)
    {
        CheckPlayerWin(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckPlayerWin(collision.gameObject);
    }

    private void CheckPlayerWin(GameObject target)
    {
        if (hasWon) return;

        if (target.GetComponent<CharacterController>() != null || 
            target.GetComponent<PlayerMovement>() != null || 
            target.CompareTag("Player"))
        {
            hasWon = true;

            Debug.Log("LEVEL COMPLETE! YOU WIN!");

            if (winParticles != null)
            {
                winParticles.Play();
            }

            Invoke(nameof(ResetLevel), resetDelay);
        }
    }

    private void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}