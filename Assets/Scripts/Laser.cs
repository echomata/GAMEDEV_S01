using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Laser : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Movement & Timing")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private float loopDelay = 1f;

    [Header("Player Interaction")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private bool killPlayerOnTouch = true;

    private Renderer[] renderers;
    private Collider[] colliders;
    private Vector3 startPos;
    private Vector3 endPos;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        foreach (var col in colliders)
        {
            if (col.gameObject != this.gameObject)
            {
                var relay = col.gameObject.GetComponent<LaserDamageRelay>();
                if (relay == null) relay = col.gameObject.AddComponent<LaserDamageRelay>();
                relay.parentLaser = this;
            }
        }
    }

    void Start()
    {
        if (startPoint == null) startPoint = transform.Find("StartPoint") ?? transform.parent?.Find("StartPoint");
        if (endPoint == null) endPoint = transform.Find("EndPoint") ?? transform.parent?.Find("EndPoint");

        startPos = startPoint != null ? startPoint.position : transform.position;
        endPos = endPoint != null ? endPoint.position : transform.position + transform.forward * 10f;

        StartCoroutine(LaserLoopRoutine());
    }

    private IEnumerator LaserLoopRoutine()
    {
        transform.position = startPos;

        if (startDelay > 0f)
        {
            SetLaserActive(false);
            yield return new WaitForSeconds(startDelay);
        }

        // Continuous loop until the scene finishes
        while (true)
        {
            // 1. Appear at start point
            transform.position = startPos;
            SetLaserActive(true);

            // 2. Move towards end point
            while (Vector3.Distance(transform.position, endPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = endPos;

            // 3. Disappear at end point
            SetLaserActive(false);

            if (loopDelay > 0f)
            {
                yield return new WaitForSeconds(loopDelay);
            }
        }
    }

    private void SetLaserActive(bool isActive)
    {
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                r.enabled = isActive;
            }
        }

        if (colliders != null)
        {
            foreach (var c in colliders)
            {
                c.enabled = isActive;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    public void HandleHit(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            return;
        }

        if (!killPlayerOnTouch) return;

        if (other.GetComponent<CharacterController>() != null || other.CompareTag("Player"))
        {
            Debug.Log("Hit by Laser!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 p1 = startPoint != null ? startPoint.position : transform.position;
        Vector3 p2 = endPoint != null ? endPoint.position : transform.position + transform.forward * 10f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(p1, p2);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(p1, 0.35f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p2, 0.35f);
    }
}

public class LaserDamageRelay : MonoBehaviour
{
    public Laser parentLaser;

    private void OnTriggerEnter(Collider other)
    {
        if (parentLaser != null)
        {
            parentLaser.HandleHit(other);
        }
    }
}