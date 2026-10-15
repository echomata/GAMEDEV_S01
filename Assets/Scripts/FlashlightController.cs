using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    private static readonly List<FlashlightController> ActiveFlashlights = new();

    [Header("References")]
    [SerializeField] private Transform lightAnchor;
    [SerializeField] private Light flashlight;

    [Header("Flashlight Settings")]
    [SerializeField] private bool startOn;
    [SerializeField] private float range = 25f;
    [SerializeField, Range(1f, 179f)] private float spotAngle = 55f;
    [SerializeField] private float intensity = 8f;
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private float shadowStrength = 1f;

    public bool IsOn => flashlight != null && flashlight.enabled;
    public Light LightSource => flashlight;

    private void Awake()
    {
        if (lightAnchor == null)
            lightAnchor = GetComponentInChildren<Camera>()?.transform;

        if (lightAnchor == null)
            lightAnchor = transform;

        EnsureLightExists();
        ApplyLightSettings();
        SetFlashlightState(startOn);
    }

    private void OnEnable()
    {
        if (!ActiveFlashlights.Contains(this))
            ActiveFlashlights.Add(this);
    }

    private void OnDisable()
    {
        ActiveFlashlights.Remove(this);
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
            Cursor.lockState == CursorLockMode.Locked)
        {
            ToggleFlashlight();
        }
    }

    public void ToggleFlashlight()
    {
        SetFlashlightState(!IsOn);
    }

    public void SetFlashlightState(bool shouldBeOn)
    {
        EnsureLightExists();
        flashlight.enabled = shouldBeOn;
    }

    public bool IsPointIlluminated(Vector3 worldPoint, Transform target = null)
    {
        if (!IsOn || flashlight == null)
            return false;

        Vector3 origin = flashlight.transform.position;
        Vector3 toPoint = worldPoint - origin;
        float distance = toPoint.magnitude;

        if (distance <= 0.001f || distance > flashlight.range)
            return false;

        float halfAngle = flashlight.spotAngle * 0.5f;
        if (Vector3.Angle(flashlight.transform.forward, toPoint) > halfAngle)
            return false;

        if (Physics.Raycast(origin, toPoint.normalized, out RaycastHit hit, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (target == null || (!hit.transform.IsChildOf(target) && hit.transform != target))
                return false;
        }

        return true;
    }

    public bool IsTargetHitByBeam(Transform target)
    {
        if (!IsOn || flashlight == null || target == null)
            return false;

        if (!Physics.Raycast(flashlight.transform.position, flashlight.transform.forward,
                out RaycastHit hit, flashlight.range, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return hit.transform == target || hit.transform.IsChildOf(target);
    }

    public static bool IsPointIlluminatedByAnyFlashlight(Vector3 worldPoint, Transform target = null)
    {
        for (int i = ActiveFlashlights.Count - 1; i >= 0; i--)
        {
            FlashlightController candidate = ActiveFlashlights[i];
            if (candidate == null)
            {
                ActiveFlashlights.RemoveAt(i);
                continue;
            }

            if (candidate.IsPointIlluminated(worldPoint, target))
                return true;
        }

        return false;
    }

    public static bool IsTargetHitByAnyBeam(Transform target)
    {
        for (int i = ActiveFlashlights.Count - 1; i >= 0; i--)
        {
            FlashlightController candidate = ActiveFlashlights[i];
            if (candidate == null)
            {
                ActiveFlashlights.RemoveAt(i);
                continue;
            }

            if (candidate.IsTargetHitByBeam(target))
                return true;
        }

        return false;
    }

    private void EnsureLightExists()
    {
        if (flashlight != null)
            return;

        GameObject lightObject = new GameObject("Player Flashlight");
        lightObject.transform.SetParent(lightAnchor, false);
        lightObject.transform.localPosition = Vector3.zero;
        lightObject.transform.localRotation = Quaternion.identity;

        flashlight = lightObject.AddComponent<Light>();
        flashlight.type = LightType.Spot;
        flashlight.renderMode = LightRenderMode.Auto;
    }

    private void ApplyLightSettings()
    {
        flashlight.range = range;
        flashlight.spotAngle = spotAngle;
        flashlight.intensity = intensity;
        flashlight.color = lightColor;
        flashlight.shadowStrength = shadowStrength;
        flashlight.shadows = shadowStrength > 0f ? LightShadows.Soft : LightShadows.None;
    }
}