using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    [Header("Finish Settings")]
    [SerializeField] private ParticleSystem winParticles;
    [SerializeField] private bool reloadSceneOnWin = true;
    [SerializeField] private float reloadDelay = 3f;

    private bool hasWon = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasWon) return;

        if (other.GetComponent<CharacterController>() != null || other.CompareTag("Player"))
        {
            hasWon = true;

            Debug.Log("LEVEL COMPLETE! YOU WIN!");

            if (winParticles != null)
            {
                winParticles.Play();
            }

            if (reloadSceneOnWin)
            {
                Invoke(nameof(RestartLevel), reloadDelay);
            }
        }
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}