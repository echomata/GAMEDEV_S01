using System.Collections;
using System;
using UnityEngine;

public enum DroneDetectionShape
{
    Sphere,
    Box,
    Capsule
}

public class DroneHazard : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Movement")]
    [SerializeField, Min(0.01f)] private float speed = 4f;
    [SerializeField, Min(0f)] private float startDelay;
    [SerializeField, Min(0f)] private float endPointDelay;
    [SerializeField, Min(0f)] private float startPointDelay;

    [Header("Red Hazard Light")]
    [SerializeField, Min(0.1f)] private float beamLength = 6f;
    [SerializeField, Min(0.05f)] private float beamRadius = 0.8f;
    [SerializeField, Range(1f, 179f)] private float spotAngle = 25f;
    [SerializeField, Min(0f)] private float lightIntensity = 8f;
    [SerializeField] private Color lightColor = Color.red;
    [SerializeField] private bool showSpotLight;

    [Header("Player Detection")]
    [SerializeField] private DroneDetectionShape detectionShape = DroneDetectionShape.Capsule;
    [SerializeField] private LayerMask playerLayers = ~0;
    [SerializeField] private LayerMask lineOfSightLayers = ~0;
    [SerializeField, Min(0.01f)] private float detectionInterval = 0.05f;
    [SerializeField] private bool requireLineOfSight = true;

    [Header("Visible Hazard Marker")]
    [SerializeField] private bool showVisibleMarker = true;
    [SerializeField, Min(0.01f)] private float visibleBeamWidth = 0.12f;
    [SerializeField, Range(0.05f, 1f)] private float markerOpacity = 0.35f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 visualOffset;
    private Vector3 visualLocalOffset;
    private Light redLight;
    private BoxCollider hazardZone;
    private LineRenderer visibleBeam;
    private Transform visibleSpot;
    private Material visibleMaterial;
    private bool playerWasDetected;
    private float nextDetectionTime;

    private void Awake()
    {
        ResolveWaypoints();
        visualOffset = GetVisualCenter() - transform.position;
        visualLocalOffset = transform.InverseTransformVector(visualOffset);

        Vector3 startWaypoint = startPoint != null ? startPoint.position : GetVisualCenter();
        Vector3 endWaypoint = endPoint != null ? endPoint.position : startWaypoint + transform.right * 10f;

        startPosition = startWaypoint - visualOffset;
        endPosition = endWaypoint - visualOffset;

        EnsureKinematicRigidbody();
        EnsureRedLight();
        EnsureHazardZone();
        EnsureVisibleMarker();
    }

    private void Start()
    {
        StartCoroutine(PatrolRoutine());
    }

    private void Update()
    {
        if (showVisibleMarker)
            UpdateVisibleMarker();

        if (Time.time >= nextDetectionTime)
        {
            nextDetectionTime = Time.time + detectionInterval;
            DetectPlayer();
        }
    }

    private IEnumerator PatrolRoutine()
    {
        transform.position = startPosition;

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        while (true)
        {
            yield return MoveTo(endPosition);

            if (endPointDelay > 0f)
                yield return new WaitForSeconds(endPointDelay);

            yield return MoveTo(startPosition);

            if (startPointDelay > 0f)
                yield return new WaitForSeconds(startPointDelay);
        }
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        while ((transform.position - destination).sqrMagnitude > 0.0025f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
    }

    public void KillPlayer(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (health != null)
            health.KillInstantly();
    }

    private void DetectPlayer()
    {
        PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
        if (player == null)
        {
            playerWasDetected = false;
            return;
        }

        int layerMask = playerLayers.value == 0 ? Physics.AllLayers : playerLayers.value;
        Vector3 center = transform.TransformPoint(visualLocalOffset);
        Vector3 capsuleTop = center + Vector3.down * 0.1f;
        Vector3 capsuleBottom = center + Vector3.down * Mathf.Max(0.1f, beamLength - 0.1f);
        Vector3 boxCenter = (capsuleTop + capsuleBottom) * 0.5f;
        Vector3 boxHalfExtents = new Vector3(beamRadius, Mathf.Max(0.1f, beamLength * 0.5f), beamRadius);

        bool volumeHit;
        Collider[] overlaps;

        switch (detectionShape)
        {
            case DroneDetectionShape.Sphere:
                volumeHit = Physics.CheckSphere(boxCenter, beamRadius, layerMask, QueryTriggerInteraction.Ignore);
                overlaps = volumeHit
                    ? Physics.OverlapSphere(boxCenter, beamRadius, layerMask, QueryTriggerInteraction.Ignore)
                    : Array.Empty<Collider>();
                break;

            case DroneDetectionShape.Box:
                volumeHit = Physics.CheckBox(boxCenter, boxHalfExtents, transform.rotation, layerMask, QueryTriggerInteraction.Ignore);
                overlaps = volumeHit
                    ? Physics.OverlapBox(boxCenter, boxHalfExtents, transform.rotation, layerMask, QueryTriggerInteraction.Ignore)
                    : Array.Empty<Collider>();
                break;

            default:
                volumeHit = Physics.CheckCapsule(capsuleTop, capsuleBottom, beamRadius, layerMask, QueryTriggerInteraction.Ignore);
                overlaps = volumeHit
                    ? Physics.OverlapCapsule(capsuleTop, capsuleBottom, beamRadius, layerMask, QueryTriggerInteraction.Ignore)
                    : Array.Empty<Collider>();
                break;
        }

        bool playerInside = false;
        foreach (Collider overlap in overlaps)
        {
            if (overlap != null && overlap.GetComponentInParent<PlayerHealth>() == player)
            {
                playerInside = true;
                break;
            }
        }

        bool detected = playerInside && (!requireLineOfSight || HasLineOfSight(player, center));

        if (detected && !playerWasDetected)
            player.KillInstantly();

        playerWasDetected = detected;
    }

    private bool HasLineOfSight(PlayerHealth player, Vector3 origin)
    {
        Vector3 target = player.transform.position + Vector3.up;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;
        if (distance <= 0.001f)
            return true;

        int layerMask = lineOfSightLayers.value == 0 ? Physics.AllLayers : lineOfSightLayers.value;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction.normalized, distance, layerMask, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;
            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                continue;

            return hit.collider.GetComponentInParent<PlayerHealth>() == player;
        }

        return true;
    }

    private void ResolveWaypoints()
    {
        if (startPoint == null)
            startPoint = transform.Find("StartPoint") ?? transform.parent?.Find("StartPoint");

        if (endPoint == null)
            endPoint = transform.Find("EndPoint") ?? transform.parent?.Find("EndPoint");
    }

    private void OnValidate()
    {
        if (startPoint == null)
            startPoint = transform.Find("StartPoint") ?? transform.parent?.Find("StartPoint");

        if (endPoint == null)
            endPoint = transform.Find("EndPoint") ?? transform.parent?.Find("EndPoint");
    }

    private Vector3 GetVisualCenter()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return transform.position;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds.center;
    }

    private void EnsureKinematicRigidbody()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null)
            body = gameObject.AddComponent<Rigidbody>();

        body.isKinematic = true;
        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void EnsureRedLight()
    {
        Transform existing = transform.Find("Drone Red Light");
        GameObject lightObject = existing != null ? existing.gameObject : new GameObject("Drone Red Light");
        lightObject.transform.SetParent(transform, false);
        lightObject.SetActive(true);

        redLight = lightObject.GetComponent<Light>();
        if (redLight == null)
            redLight = lightObject.AddComponent<Light>();

        if (redLight == null)
        {
            Debug.LogError($"{name}: Could not create a Light on '{lightObject.name}'. The drone will still patrol.", this);
            return;
        }

        redLight.type = LightType.Spot;
        redLight.color = lightColor;
        redLight.intensity = lightIntensity;
        redLight.range = beamLength;
        redLight.spotAngle = spotAngle;
        redLight.shadows = LightShadows.None;
        redLight.transform.localPosition = visualLocalOffset;
        redLight.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        redLight.enabled = showSpotLight;
    }

    private void EnsureHazardZone()
    {
        Transform existing = transform.Find("Drone Hazard Zone");
        GameObject hazardObject = existing != null ? existing.gameObject : new GameObject("Drone Hazard Zone");
        hazardObject.transform.SetParent(transform, false);
        hazardObject.transform.localPosition = visualLocalOffset + new Vector3(0f, -beamLength * 0.5f, 0f);
        hazardObject.transform.localRotation = Quaternion.identity;

        hazardZone = hazardObject.GetComponent<BoxCollider>();
        if (hazardZone == null)
            hazardZone = hazardObject.AddComponent<BoxCollider>();

        if (hazardZone == null)
        {
            Debug.LogError($"{name}: Could not create a BoxCollider on '{hazardObject.name}'. The drone will still patrol.", this);
            return;
        }

        hazardZone.isTrigger = true;
        hazardZone.size = new Vector3(beamRadius * 2f, beamLength, beamRadius * 2f);

        DroneHazardTrigger relay = hazardObject.GetComponent<DroneHazardTrigger>();
        if (relay == null)
            relay = hazardObject.AddComponent<DroneHazardTrigger>();

        relay.parentDrone = this;
    }

    private void EnsureVisibleMarker()
    {
        if (!showVisibleMarker)
            return;

        Transform beamTransform = transform.Find("Visible Red Beam");
        GameObject beamObject = beamTransform != null ? beamTransform.gameObject : new GameObject("Visible Red Beam");
        beamObject.transform.SetParent(transform, false);

        visibleBeam = beamObject.GetComponent<LineRenderer>();
        if (visibleBeam == null)
            visibleBeam = beamObject.AddComponent<LineRenderer>();

        if (visibleBeam == null)
            return;

        visibleMaterial = CreateVisibleMaterial();
        visibleBeam.material = visibleMaterial;
        visibleBeam.useWorldSpace = false;
        beamObject.transform.localPosition = visualLocalOffset;
        beamObject.transform.localRotation = Quaternion.identity;
        visibleBeam.positionCount = 2;
        visibleBeam.startWidth = visibleBeamWidth;
        visibleBeam.endWidth = visibleBeamWidth * 3f;
        visibleBeam.startColor = lightColor;
        visibleBeam.endColor = new Color(lightColor.r, lightColor.g, lightColor.b, markerOpacity);
        visibleBeam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        visibleBeam.receiveShadows = false;

        Transform spotTransform = transform.Find("Visible Red Ground Spot");
        GameObject spotObject;
        if (spotTransform != null)
        {
            spotObject = spotTransform.gameObject;
        }
        else
        {
            spotObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spotObject.name = "Visible Red Ground Spot";
            spotObject.transform.SetParent(transform, false);

            Collider spotCollider = spotObject.GetComponent<Collider>();
            if (spotCollider != null)
                Destroy(spotCollider);
        }

        visibleSpot = spotObject.transform;
        MeshRenderer spotRenderer = spotObject.GetComponent<MeshRenderer>();
        if (spotRenderer != null)
        {
            spotRenderer.sharedMaterial = visibleMaterial;
            spotRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spotRenderer.receiveShadows = false;
        }
    }

    private void UpdateVisibleMarker()
    {
        if (visibleBeam == null || visibleSpot == null)
            return;

        Vector3 origin = transform.TransformPoint(visualLocalOffset);
        Vector3 groundPoint = origin + Vector3.down * beamLength;
        Vector3 groundNormal = Vector3.up;

        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, beamLength, ~0, QueryTriggerInteraction.Ignore);
        float closestDistance = float.PositiveInfinity;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                continue;
            if (hit.collider.GetComponentInParent<PlayerHealth>() != null)
                continue;
            if (hit.collider.GetComponentInParent<DroneHazard>() != null)
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                groundPoint = hit.point + hit.normal * 0.02f;
                groundNormal = hit.normal;
            }
        }

        if (groundNormal.y < 0.5f)
            groundNormal = Vector3.up;

        visibleBeam.SetPosition(0, Vector3.zero);
        visibleBeam.SetPosition(1, Vector3.down * beamLength);

        visibleSpot.position = groundPoint;
        visibleSpot.rotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
        visibleSpot.localScale = new Vector3(beamRadius * 2f, 0.02f, beamRadius * 2f);
    }

    private Material CreateVisibleMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        Material material = new Material(shader);
        Color color = new Color(lightColor.r, lightColor.g, lightColor.b, markerOpacity);

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else
            material.color = color;

        if (material.HasProperty("_Surface"))
            material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Blend"))
            material.SetFloat("_Blend", 0f);

        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(lightColor.r, lightColor.g, lightColor.b, 0.8f);
        Vector3 origin = Application.isPlaying ? transform.TransformPoint(visualLocalOffset) : GetVisualCenter();
        Vector3 end = origin + Vector3.down * beamLength;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(end, beamRadius);
    }
}

public class DroneHazardTrigger : MonoBehaviour
{
    public DroneHazard parentDrone;

    private void OnTriggerEnter(Collider other)
    {
        if (parentDrone != null)
            parentDrone.KillPlayer(other);
    }
}