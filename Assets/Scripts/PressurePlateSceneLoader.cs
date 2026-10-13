using UnityEngine;
using UnityEngine.SceneManagement;

public class PressurePlateSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string playerTag = "Player";

    private bool hasLoaded;

    private void OnTriggerEnter(Collider other)
    {
        if (hasLoaded || !other.CompareTag(playerTag))
            return;

        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError($"{name} has no destination scene assigned.", this);
            return;
        }

        hasLoaded = true;
        SceneManager.LoadScene(sceneToLoad);
    }
}
