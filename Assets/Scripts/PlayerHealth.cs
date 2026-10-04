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

        currentHealth = maxHealth;
        UpdateUI();
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
        Debug.Log("Player died! Respawning at spawn point...");

        if (controller != null) controller.enabled = false;

        Vector3 targetRespawn = spawnPoint != null ? spawnPoint.position : initialPosition;
        transform.position = targetRespawn;

        if (controller != null) controller.enabled = true;

        // Restore health on respawn
        currentHealth = maxHealth;
        UpdateUI();
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