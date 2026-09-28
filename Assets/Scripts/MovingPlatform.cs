using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Distance to move relative to starting position (X, Y, Z whole units)")]
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 0f, 24f);
    [SerializeField] private float speed = 8f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 lastPlatformPos;
    private CharacterController activePlayer;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;
        lastPlatformPos = transform.position;
    }

    void Update()
    {
        Vector3 destination = (activePlayer != null) ? targetPos : startPos;

        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        Vector3 platformDelta = transform.position - lastPlatformPos;
        if (activePlayer != null && platformDelta != Vector3.zero)
        {
            activePlayer.Move(platformDelta);
        }

        lastPlatformPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
        {
            activePlayer = cc;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null && activePlayer == cc)
        {
            activePlayer = null;
        }
    }
}