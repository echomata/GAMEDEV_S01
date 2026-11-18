using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AuraPad : MonoBehaviour
{
    public AuraKind aura = AuraKind.Transformation;
    public bool resetOnExit;
    public float surfaceHeight = -0.6f;
    BoxCollider zone;

    void Awake() => zone = GetComponent<BoxCollider>();
    void OnTriggerEnter(Collider other) => TryActivate(other);
    void OnTriggerStay(Collider other) => TryActivate(other);

    void TryActivate(Collider other)
    {
        var player = other.GetComponentInParent<PlayerAura>();
        if (player == null) return;
        Vector3 feet = transform.InverseTransformPoint(player.transform.position);
        Vector3 center = zone.center;
        Vector3 half = zone.size * 0.5f;
        if (Mathf.Abs(feet.x - center.x) > half.x || Mathf.Abs(feet.z - center.z) > half.z) return;
        if (feet.y < surfaceHeight - 0.12f || feet.y > surfaceHeight + 0.35f) return;
        player.Activate(aura);
    }

    void OnTriggerExit(Collider other)
    {
        if (!resetOnExit) return;
        var player = other.GetComponentInParent<PlayerAura>();
        if (player != null && player.CurrentAura == aura) player.ResetToNeutral();
    }
}