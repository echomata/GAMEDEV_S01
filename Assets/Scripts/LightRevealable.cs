using UnityEngine;

public class LightRevealable : MonoBehaviour
{
    private static readonly float[] HorizontalSampleFractions = { 0f, 0.5f, 1f };
    private static readonly float[] VerticalSampleFractions = { 0f, 0.25f, 0.5f, 0.75f, 1f };

    [Header("Reveal Settings")]
    [SerializeField] private bool startRevealed;
    [SerializeField] private bool stayRevealedOnceFound;
    [SerializeField, Min(0.01f)] private float checkInterval = 0.05f;

    private Renderer[] renderers;
    private Collider targetCollider;
    private Bounds revealBounds;
    private float nextCheckTime;
    private bool hasBeenRevealed;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        targetCollider = GetComponentInChildren<Collider>();
        revealBounds = GetCombinedBounds();
        hasBeenRevealed = startRevealed;
        ApplyVisibility(startRevealed);
    }

    private void Update()
    {
        if (stayRevealedOnceFound && hasBeenRevealed)
            return;

        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkInterval;
        bool illuminated = IsIlluminated();

        if (illuminated)
            hasBeenRevealed = true;

        ApplyVisibility(illuminated || (stayRevealedOnceFound && hasBeenRevealed));
    }

    private bool IsIlluminated()
    {
        if (FlashlightController.IsTargetHitByAnyBeam(transform))
            return true;

        Vector3 min = revealBounds.min;
        Vector3 max = revealBounds.max;

        foreach (float x in HorizontalSampleFractions)
        {
            foreach (float y in VerticalSampleFractions)
            {
                foreach (float z in HorizontalSampleFractions)
                {
                    Vector3 samplePoint = new Vector3(
                        Mathf.Lerp(min.x, max.x, x),
                        Mathf.Lerp(min.y, max.y, y),
                        Mathf.Lerp(min.z, max.z, z));

                    if (IsPointIlluminated(samplePoint))
                        return true;
                }
            }
        }

        return false;
    }

    private bool IsPointIlluminated(Vector3 point)
    {
        if (FlashlightController.IsPointIlluminatedByAnyFlashlight(point, transform))
            return true;

        if (SupportsClosestPoint(targetCollider))
        {
            Vector3 closestPoint = targetCollider.ClosestPoint(point);
            return FlashlightController.IsPointIlluminatedByAnyFlashlight(closestPoint, transform);
        }

        return false;
    }

    private bool SupportsClosestPoint(Collider collider)
    {
        if (collider is BoxCollider || collider is SphereCollider || collider is CapsuleCollider)
            return true;

        MeshCollider meshCollider = collider as MeshCollider;
        return meshCollider != null && meshCollider.convex;
    }

    private Bounds GetCombinedBounds()
    {
        if (renderers == null || renderers.Length == 0)
            return new Bounds(transform.position, Vector3.one * 0.1f);

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        return combined;
    }

    private void ApplyVisibility(bool visible)
    {
        if (renderers == null)
            return;

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer != null)
                targetRenderer.enabled = visible;
        }
    }
}