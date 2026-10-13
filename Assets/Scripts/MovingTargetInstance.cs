using System;
using UnityEngine;

public class MovingTargetInstance : MonoBehaviour
{
    [Header("Route Markers")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    private Rigidbody targetBody;
    private Vector3 targetPosition;
    private float speed;
    private Action<MovingTargetInstance> finishedCallback;
    private bool initialized;

    private void Awake()
    {
        if (startPoint == null)
            startPoint = transform.Find("StartPoint");

        if (endPoint == null)
            endPoint = transform.Find("EndPoint");

        targetBody = GetComponent<Rigidbody>();

        // Moving targets are destroyed at the endpoint, so they should not
        // reset themselves during the trip.
        foreach (PyramidResetter resetter in
                 GetComponentsInChildren<PyramidResetter>(true))
        {
            resetter.enabled = false;
        }
    }

    public void Initialize(
        Vector3 spawnPosition,
        Quaternion spawnRotation,
        float movementSpeed,
        Action<MovingTargetInstance> onFinished)
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError(
                $"{name}: MovingTargetInstance needs child objects named StartPoint and EndPoint.",
                this);
            return;
        }

        speed = Mathf.Max(0f, movementSpeed);
        finishedCallback = onFinished;

        // Align the prefab's StartPoint with the spawner's spawn position.
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        Vector3 startOffset = startPoint.position - transform.position;
        transform.position -= startOffset;

        Vector3 routeDelta = endPoint.position - startPoint.position;
        targetPosition = transform.position + routeDelta;
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.fixedDeltaTime);

        if (targetBody != null)
            targetBody.MovePosition(nextPosition);
        else
            transform.position = nextPosition;

        if (nextPosition == targetPosition)
            finishedCallback?.Invoke(this);
    }
}
