using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    [Header("Respawn Settings")]
    [SerializeField] private Transform spawnPoint;

    private float currentHealth;
    private CharacterController controller;
    private Vector3 initialPosition;
    private Vector3 currentRespawnPoint;
    private float lastDamageTime = -999f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        initialPosition = transform.position;

        if (spawnPoint == null)
        {
            GameObject spawnObj = GameObject.Find("Spawn");
            if (spawnObj != null) spawnPoint = spawnObj.transform;
        }

        currentRespawnPoint = spawnPoint != null ? spawnPoint.position : initialPosition;
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void SetCheckpoint(Vector3 checkpointPos)
    {
        currentRespawnPoint = checkpointPos;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        if (Time.time < lastDamageTime + invulnerabilityDuration) return;
        lastDamageTime = Time.time;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (currentHealth <= 0f)
        {
            DieAndRespawn();
        }
    }

    private void DieAndRespawn()
    {
        Debug.Log("Player died! Respawning at checkpoint...");

        transform.SetParent(null);
        if (controller != null) controller.enabled = false;

        transform.position = currentRespawnPoint;

        if (controller != null) controller.enabled = true;

        // Restore health on respawn
        currentHealth = maxHealth;
        UpdateUI();

        // Reset speed and jump boosts back to normal on death
        GetComponent<PlayerMovement>()?.ResetModifiers();
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
}