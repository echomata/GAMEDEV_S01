using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Speed of rotation in degrees per second around X, Y, and Z axes")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 50f, 0f);

    private CharacterController activePlayer;

    void Update()
    {
        Vector3 angleDelta = rotationSpeed * Time.deltaTime;
        Quaternion deltaRotation = Quaternion.Euler(angleDelta);

        // Rotate player around platform pivot if standing on it
        if (activePlayer != null)
        {
            Vector3 playerPos = activePlayer.transform.position;
            Vector3 platformPos = transform.position;

            Vector3 offset = playerPos - platformPos;
            Vector3 newOffset = deltaRotation * offset;
            Vector3 positionDelta = (platformPos + newOffset) - playerPos;

            // Move player position around rotation arc
            activePlayer.Move(positionDelta);

            // Rotate player body orientation with platform Y rotation
            activePlayer.transform.Rotate(0f, angleDelta.y, 0f, Space.World);
        }

        // Rotate the platform
        transform.Rotate(angleDelta);
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
