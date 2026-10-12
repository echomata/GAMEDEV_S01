using UnityEngine;

public class KnockdownTarget : MonoBehaviour
{
    [SerializeField, Range(1f, 89f)] private float knockdownAngle = 45f;
    [SerializeField, Min(0f)] private float minimumFallDistance = 0.25f;

    private KnockdownCounter counter;
    private float startingWorldY;
    private bool hasBeenCounted;

    private void Awake()
    {
        startingWorldY = transform.position.y;
    }

    public void Initialize(KnockdownCounter owner)
    {
        counter = owner;
    }

    private void Update()
    {
        if (hasBeenCounted || counter == null)
            return;

        float tilt = Vector3.Angle(transform.up, Vector3.up);
        bool isTilted = tilt >= knockdownAngle;
        bool hasFallen = transform.position.y <= startingWorldY - minimumFallDistance;

        if (isTilted || hasFallen)
        {
            hasBeenCounted = true;
            counter.ReportKnockedDown(this);
        }
    }
}
