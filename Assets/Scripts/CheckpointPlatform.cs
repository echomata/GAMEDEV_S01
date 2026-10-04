using UnityEngine;

public class CheckpointPlatform : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private bool singleUse = true;

    [Header("Visual Feedback")]
    [SerializeField] private bool changeColorOnActivate = true;
    [SerializeField] private Color activatedColor = Color.green;

    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        TryActivateCheckpoint(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryActivateCheckpoint(collision.gameObject);
    }

    private void TryActivateCheckpoint(GameObject target)
    {
        if (singleUse && isActivated) return;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>() ?? target.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            Vector3 respawnPos = transform.position + spawnOffset;
            playerHealth.SetCheckpoint(respawnPos);

            isActivated = true;
            Debug.Log("Checkpoint set!");

            if (changeColorOnActivate)
            {
                Renderer rend = GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = activatedColor;
                }
            }
        }
    }
}