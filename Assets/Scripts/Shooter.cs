using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField, Min(0.01f)] private float fireInterval = 1f;
    [SerializeField, Min(0)] private int startingAmmo = 10;
    [SerializeField] private bool unlimitedAmmo = false;
    [SerializeField] private bool holdToFire = true;

    [Header("Projectile")]
    [Tooltip("Optional prefab. If empty, a small sphere projectile is created automatically.")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Camera aimCamera;
    [SerializeField, Min(0.1f)] private float bulletSpeed = 35f;
    [SerializeField, Min(0.1f)] private float bulletLifetime = 5f;
    [SerializeField, Min(0f)] private float muzzleForwardOffset = 0.5f;

    [Header("Crosshair")]
    [SerializeField] private bool showCrosshair = true;
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField, Min(1f)] private float crosshairLength = 12f;
    [SerializeField, Min(1f)] private float crosshairThickness = 2f;
    [SerializeField, Min(0f)] private float crosshairGap = 4f;

    private float nextFireTime;
    private int bulletsRemaining;
    private Texture2D crosshairTexture;

    public int BulletsRemaining => bulletsRemaining;
    public bool HasAmmo => unlimitedAmmo || bulletsRemaining > 0;

    private void Awake()
    {
        bulletsRemaining = Mathf.Max(0, startingAmmo);

        if (muzzle == null)
            muzzle = transform;

        if (aimCamera == null)
            aimCamera = GetComponentInChildren<Camera>();

        if (aimCamera == null)
            aimCamera = Camera.main;

        crosshairTexture = new Texture2D(1, 1);
        crosshairTexture.name = "Shooter Crosshair Texture";
        crosshairTexture.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnDestroy()
    {
        if (crosshairTexture != null)
            Destroy(crosshairTexture);
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || Cursor.lockState != CursorLockMode.Locked)
            return;

        bool wantsToFire = holdToFire
            ? mouse.leftButton.isPressed
            : mouse.leftButton.wasPressedThisFrame;

        if (wantsToFire)
            TryFire();
    }

    public bool TryFire()
    {
        if (Time.time < nextFireTime || !HasAmmo)
            return false;

        nextFireTime = Time.time + Mathf.Max(0.01f, fireInterval);

        if (!unlimitedAmmo)
            bulletsRemaining--;

        Transform source = muzzle != null ? muzzle : transform;
        Vector3 direction = GetAimDirection(source.position, source.forward);
        Vector3 spawnPosition = source.position + direction * muzzleForwardOffset;

        GameObject projectile = projectilePrefab != null
            ? Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction))
            : CreateDefaultProjectile(spawnPosition, direction);

        ConfigureProjectile(projectile, direction, source);
        return true;
    }

    public void Reload()
    {
        bulletsRemaining = Mathf.Max(0, startingAmmo);
    }

    private Vector3 GetAimDirection(Vector3 sourcePosition, Vector3 fallback)
    {
        if (aimCamera == null)
            return fallback.normalized;

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 target = ray.origin + ray.direction * 1000f;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            target = hit.point;

        return (target - sourcePosition).normalized;
    }

    private GameObject CreateDefaultProjectile(Vector3 position, Vector3 direction)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Projectile (Runtime)";
        projectile.transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
        projectile.transform.localScale = Vector3.one * 0.12f;

        Renderer renderer = projectile.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = Color.yellow;

        return projectile;
    }

    private void ConfigureProjectile(GameObject projectile, Vector3 direction, Transform source)
    {
        if (projectile == null)
            return;

        Rigidbody body = projectile.GetComponent<Rigidbody>();
        if (body == null)
            body = projectile.AddComponent<Rigidbody>();

        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.linearVelocity = direction * bulletSpeed;

        foreach (Collider sourceCollider in source.GetComponentsInParent<Collider>())
        {
            foreach (Collider projectileCollider in projectile.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(sourceCollider, projectileCollider);
        }

        Destroy(projectile, bulletLifetime);
    }

    private void OnGUI()
    {
        if (!showCrosshair || crosshairTexture == null)
            return;

        crosshairTexture.SetPixel(0, 0, crosshairColor);
        crosshairTexture.Apply();

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        float halfGap = crosshairGap * 0.5f;

        GUI.DrawTexture(new Rect(centerX - crosshairThickness * 0.5f, centerY - halfGap - crosshairLength, crosshairThickness, crosshairLength), crosshairTexture);
        GUI.DrawTexture(new Rect(centerX - crosshairThickness * 0.5f, centerY + halfGap, crosshairThickness, crosshairLength), crosshairTexture);
        GUI.DrawTexture(new Rect(centerX - halfGap - crosshairLength, centerY - crosshairThickness * 0.5f, crosshairLength, crosshairThickness), crosshairTexture);
        GUI.DrawTexture(new Rect(centerX + halfGap, centerY - crosshairThickness * 0.5f, crosshairLength, crosshairThickness), crosshairTexture);
    }
}
