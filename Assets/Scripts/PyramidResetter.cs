using System.Collections;
using UnityEngine;

public class PyramidResetter : MonoBehaviour
{
    [SerializeField, Min(0f)] private float resetDelay = 5f;
    [SerializeField, Range(1f, 89f)] private float knockdownAngle = 45f;
    [SerializeField, Min(0f)] private float minimumFallDistance = 0.25f;

    private Transform[] cans;
    private KnockdownTarget[] targets;
    private Rigidbody[] bodies;
    private Vector3[] startingLocalPositions;
    private Quaternion[] startingLocalRotations;
    private float[] startingWorldHeights;
    private bool resetQueued;

    private void Awake()
    {
        targets = GetComponentsInChildren<KnockdownTarget>();
        cans = new Transform[targets.Length];
        bodies = new Rigidbody[targets.Length];
        startingLocalPositions = new Vector3[targets.Length];
        startingLocalRotations = new Quaternion[targets.Length];
        startingWorldHeights = new float[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            cans[i] = targets[i].transform;
            bodies[i] = targets[i].GetComponent<Rigidbody>();
            startingLocalPositions[i] = cans[i].localPosition;
            startingLocalRotations[i] = cans[i].localRotation;
            startingWorldHeights[i] = cans[i].position.y;
        }
    }

    private void Update()
    {
        if (resetQueued)
            return;

        for (int i = 0; i < cans.Length; i++)
        {
            float tilt = Vector3.Angle(cans[i].up, Vector3.up);
            bool isTilted = tilt >= knockdownAngle;
            bool hasFallen = cans[i].position.y <= startingWorldHeights[i] - minimumFallDistance;

            if (isTilted || hasFallen)
            {
                resetQueued = true;
                StartCoroutine(ResetAfterDelay());
                return;
            }
        }
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        for (int i = 0; i < cans.Length; i++)
        {
            if (bodies[i] != null)
            {
                bodies[i].linearVelocity = Vector3.zero;
                bodies[i].angularVelocity = Vector3.zero;
                bodies[i].isKinematic = true;
            }

            cans[i].SetLocalPositionAndRotation(startingLocalPositions[i], startingLocalRotations[i]);
        }

        Physics.SyncTransforms();

        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i] != null)
            {
                bodies[i].isKinematic = false;
                bodies[i].Sleep();
            }

            targets[i].ResetCountedState();
        }

        resetQueued = false;
    }
}
