using UnityEngine;

public class PowerUpPlatform : MonoBehaviour
{
    [Header("Boost Multipliers")]
    [SerializeField] private float speedMultiplier = 1.75f;
    [SerializeField] private float jumpMultiplier = 2f;
    [SerializeField] private float duration = 0f;

    [Header("Visual Feedback")]
    [SerializeField] private Color activatedColor = Color.cyan;
    [SerializeField] private bool changeColorOnUse = true;

    private bool playerOnPlatform = false;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryActivatePowerUp(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryActivatePowerUp(collision.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        playerOnPlatform = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        playerOnPlatform = false;
    }

    private void TryActivatePowerUp(GameObject target)
    {
        if (playerOnPlatform) return;

        PlayerMovement player = target.GetComponent<PlayerMovement>() ?? target.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            playerOnPlatform = true;
            player.ApplyPowerUp(speedMultiplier, jumpMultiplier, duration);

            if (changeColorOnUse && rend != null)
            {
                rend.material.color = activatedColor;
            }

            Debug.Log("Power-up received!");
        }
    }
}