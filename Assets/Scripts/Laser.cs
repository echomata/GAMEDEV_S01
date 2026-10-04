using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Laser : MonoBehaviour
{
    [Header("Waypoints (Where it Appears & Disappears)")]
    [Tooltip("Drag a GameObject here where the laser will start/appear. If empty, uses this object's starting position.")]
    [SerializeField] private Transform startPoint;

    [Tooltip("Drag a GameObject here where the laser will travel to and disappear.")]
    [SerializeField] private Transform endPoint;

    [Header("Movement & Timing")]
    [Tooltip("Speed the laser moves from start to end")]
    [SerializeField] private float speed = 5f;

    [Tooltip("Initial delay in seconds before the laser starts its very first cycle (great for staggering lasers)")]
    [SerializeField] private float startDelay = 0f;

    [Tooltip("Pause duration in seconds after disappearing before it loops back and appears again")]
    [SerializeField] private float loopDelay = 1f;

    [Header("Player Interaction")]
    [Tooltip("If checked, touching the player will restart the scene")]
    [SerializeField] private bool killPlayerOnTouch = true;

    private Renderer[] renderers;
    private Collider[] colliders;
    private Vector3 startPos;
    private Vector3 endPos;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
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