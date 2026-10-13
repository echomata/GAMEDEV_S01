using UnityEngine;

public class KnockdownTarget : MonoBehaviour
{
    [SerializeField, Range(1f, 89f)] private float knockdownAngle = 45f;
    [SerializeField, Min(0f)] private float minimumFallDistance = 0.25f;
    [SerializeField, Min(0)] private int scoreValue = 1;

    private KnockdownCounter counter;
    private float startingWorldY;
    private bool hasBeenCounted;

    public int ScoreValue => scoreValue;

    private void Awake()
    {
        startingWorldY = transform.position.y;
    }

    private void Start()
    {
        // Runtime-spawned cans are created after KnockdownCounter.Start(),
        // so they need to find the counter themselves.
        if (counter == null)
            counter = FindFirstObjectByType<KnockdownCounter>();
    }

    public void Initialize(KnockdownCounter owner)
    {
        counter = owner;
    }

    public void ResetCountedState()
    {
        hasBeenCounted = false;
    }

    private void Update()
    {
        if (hasBeenCounted)
            return;

        float tilt = Vector3.Angle(transform.up, Vector3.up);
        bool isTilted = tilt >= knockdownAngle;
        bool hasFallen = transform.position.y <= startingWorldY - minimumFallDistance;

        if (isTilted || hasFallen)
        {
            hasBeenCounted = true;

            if (counter != null)
                counter.ReportKnockedDown(this);
        }
    }
}
