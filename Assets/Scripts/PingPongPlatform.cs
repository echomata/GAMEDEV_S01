using UnityEngine;

public class PingPongPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 0f, 20f);
    [SerializeField] private float speed = 4f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 lastPlatformPos;
    private bool movingToTarget = true;
    private CharacterController activePlayer;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;
        lastPlatformPos = transform.position;
    }

    void Update()
    {
        Vector3 destination = movingToTarget ? targetPos : startPos;

        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        Vector3 platformDelta = transform.position - lastPlatformPos;
        if (activePlayer != null && platformDelta != Vector3.zero)
        {
            activePlayer.Move(platformDelta);
        }

        lastPlatformPos = transform.position;

        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            movingToTarget = !movingToTarget;
        }
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