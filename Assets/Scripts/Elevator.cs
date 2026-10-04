using UnityEngine;

public class Elevator : MonoBehaviour
{
    [Header("Elevator Settings")]
    [SerializeField] private float riseHeight = 10f;  // How high the elevator rises
    [SerializeField] private float speed = 3f;        // Speed of movement

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool playerOnElevator = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.up * riseHeight;
    }

    void Update()
    {
        if (playerOnElevator)
        {
            Collider col = GetComponent<Collider>();
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                if (col != null)
                {
                    Bounds expanded = col.bounds;
                    expanded.Expand(4f);
                    if (!expanded.Contains(player.transform.position))
                    {
                        playerOnElevator = false;
                        player.transform.SetParent(null);
                    }
                }
                else if (Vector3.Distance(transform.position, player.transform.position) > 8f)
                {
                    playerOnElevator = false;
                    player.transform.SetParent(null);
                }
            }
        }

        Vector3 destination = playerOnElevator ? targetPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null || other.CompareTag("Player"))
        {
            playerOnElevator = true;
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null || other.CompareTag("Player"))
        {
            playerOnElevator = false;
            other.transform.SetParent(null);
        }
    }
}