using UnityEngine;

public class MovingTargetGroup : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 0f, 24f);
    [SerializeField, Min(0f)] private float speed = 8f;
    [SerializeField] private bool moveContinuously = true;
    [SerializeField] private bool reverseAtEnds = true;
    [SerializeField, Min(0f)] private float waitAtEnds = 0f;

    [Tooltip("Usually the Cylinder child. If empty, a child named Cylinder is searched for automatically.")]
    [SerializeField] private Transform movingPlatform;

    [Header("Optional Player Carrying")]
    [SerializeField] private bool carryPlayer = true;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool movingToTarget = true;
    private float endWaitTimer;
    private CharacterController activePlayer;
    private Rigidbody[] carriedBodies;

    private void Awake()
    {
        if (movingPlatform == null)
            movingPlatform = transform.Find("Cylinder");

        if (movingPlatform == null)
            movingPlatform = transform;

        carriedBodies = GetComponentsInChildren<Rigidbody>(true);

        startPosition = movingPlatform.position;
        targetPosition = startPosition + moveOffset;
    }

    private void FixedUpdate()
    {
        Vector3 destination;

        if (moveContinuously)
        {
            destination = movingToTarget ? targetPosition : startPosition;
        }
        else
        {
            destination = activePlayer != null ? targetPosition : startPosition;
        }

        if (endWaitTimer > 0f)
        {
            endWaitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 previousPosition = movingPlatform.position;
        Vector3 nextPosition = Vector3.MoveTowards(
            movingPlatform.position,
            destination,
            speed * Time.fixedDeltaTime);

        Vector3 movementDelta = nextPosition - previousPosition;

        movingPlatform.position = nextPosition;

        // Move dynamic cans by the same physics delta. This keeps them on the
        // moving cylinder while preserving their Rigidbody physics and bullets.
        foreach (Rigidbody body in carriedBodies)
        {
            if (body == null || body.transform.IsChildOf(movingPlatform))
                continue;

            body.MovePosition(body.position + movementDelta);
        }

        if (carryPlayer && activePlayer != null && movementDelta != Vector3.zero)
        {
            activePlayer.Move(movementDelta);
        }

        if (moveContinuously && movingPlatform.position == destination)
        {
            if (reverseAtEnds)
            {
                movingToTarget = !movingToTarget;
                endWaitTimer = waitAtEnds;
            }
            else
            {
                moveContinuously = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterController characterController =
            other.GetComponent<CharacterController>();

        if (characterController != null)
        {
            activePlayer = characterController;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterController characterController =
            other.GetComponent<CharacterController>();

        if (characterController != null && activePlayer == characterController)
        {
            activePlayer = null;
        }
    }
}
