using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
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
        if (activePlayer != null)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Bounds expanded = col.bounds;
                expanded.Expand(4f);
                if (!expanded.Contains(activePlayer.transform.position))
                {
                    activePlayer = null;
                }
            }
            else if (Vector3.Distance(transform.position, activePlayer.transform.position) > 8f)
            {
                activePlayer = null;
            }
        }

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