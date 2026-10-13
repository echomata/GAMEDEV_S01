using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTargetSpawner : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject targetPrefab;
    [Tooltip("The spawned target's StartPoint will be aligned to this position.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn Settings")]
    [SerializeField, Min(0.01f)] private float speed = 3f;
    [SerializeField, Min(0f)] private float spawnInterval = 4f;
    [Tooltip("Set to 0 for unlimited active targets.")]
    [SerializeField, Min(0)] private int maxActiveTargets = 1;
    [SerializeField, Min(0f)] private float startDelay = 0f;

    private readonly List<MovingTargetInstance> activeTargets = new();

    private void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        if (targetPrefab == null)
        {
            Debug.LogError(
                $"{name}: Assign Target Prefab on MovingTargetSpawner.",
                this);
            enabled = false;
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        while (enabled)
        {
            RemoveDestroyedTargets();

            if (maxActiveTargets <= 0 || activeTargets.Count < maxActiveTargets)
                SpawnTarget();

            if (spawnInterval > 0f)
                yield return new WaitForSeconds(spawnInterval);
            else
                yield return null;
        }
    }

    private void SpawnTarget()
    {
        GameObject targetObject = Instantiate(
            targetPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        MovingTargetInstance target =
            targetObject.GetComponent<MovingTargetInstance>();

        if (target == null)
            target = targetObject.AddComponent<MovingTargetInstance>();

        activeTargets.Add(target);

        target.Initialize(
            spawnPoint.position,
            spawnPoint.rotation,
            speed,
            HandleTargetFinished);
    }

    private void HandleTargetFinished(MovingTargetInstance target)
    {
        activeTargets.Remove(target);

        if (target != null)
            Destroy(target.gameObject);
    }

    private void RemoveDestroyedTargets()
    {
        activeTargets.RemoveAll(target => target == null);
    }
}
