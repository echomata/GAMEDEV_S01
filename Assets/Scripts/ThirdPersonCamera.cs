using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    [SerializeField] float distance = 6f;
    [SerializeField] float mouseSensitivity = 0.12f;
    [SerializeField] float minimumPitch = -15f;
    [SerializeField] float maximumPitch = 65f;
    [SerializeField] float followSmoothTime = 0.08f;
    [SerializeField] LayerMask collisionLayers = ~0;

    float yaw;
    float pitch = 20f;
    Vector3 followVelocity;
    readonly RaycastHit[] hits = new RaycastHit[16];

    void Start() => SnapToTarget();

    void LateUpdate()
    {
        if (target == null) return;
        var mouse = Mouse.current;
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, minimumPitch, maximumPitch);
            distance = Mathf.Clamp(distance - mouse.scroll.y.ReadValue() * 0.005f, 3f, 8f);
        }
        Vector3 position = DesiredPosition(out bool obstructed);
        if (obstructed)
        {
            transform.position = position;
            followVelocity = Vector3.zero;
        }
        else transform.position = Vector3.SmoothDamp(transform.position, position, ref followVelocity, followSmoothTime);
        transform.LookAt(target.position);
    }

    Vector3 DesiredPosition(out bool obstructed)
    {
        Vector3 backward = -(Quaternion.Euler(pitch, yaw, 0) * Vector3.forward);
        float permittedDistance = distance;
        int count = Physics.SphereCastNonAlloc(target.position, 0.2f, backward, hits, distance, collisionLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider.transform.IsChildOf(target.root)) continue;
            permittedDistance = Mathf.Min(permittedDistance, Mathf.Max(0.3f, hits[i].distance - 0.15f));
        }
        obstructed = permittedDistance < distance - 0.001f;
        return target.position + backward * permittedDistance;
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        followVelocity = Vector3.zero;
        transform.position = DesiredPosition(out _);
        transform.LookAt(target.position);
    }
}