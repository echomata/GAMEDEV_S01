using System;
using UnityEngine;

public enum AuraKind { None, Transformation, Elemental, Boss }

[Serializable]
public class AuraLayer
{
    public Texture2D sourceTexture;
    public Vector3 localOffset = Vector3.zero;
    public Vector3 localRotation = Vector3.zero;
    public Vector3 scaleMultiplier = Vector3.one;
}

[Serializable]
public class AuraTheme : ISerializationCallbackReceiver
{
    public AuraLayer layer1 = new AuraLayer();
    public AuraLayer layer2 = new AuraLayer();
    public AuraLayer layer3 = new AuraLayer();
    public Color skyColor = Color.gray;
    public Color ambientColor = Color.gray;
    public Color sunlightColor = Color.white;
    public Color playerColor = Color.white;
    [SerializeField, HideInInspector] Texture2D sourceTexture;
    [SerializeField, HideInInspector] Vector3 localOffset = Vector3.zero;
    [SerializeField, HideInInspector] Vector3 localRotation = Vector3.zero;
    [SerializeField, HideInInspector] Vector3 scaleMultiplier = Vector3.one;

    public AuraLayer GetLayer(int index)
    {
        switch (index)
        {
            case 0: return layer1;
            case 1: return layer2;
            case 2: return layer3;
            default: throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        if (layer1 == null) layer1 = new AuraLayer();
        if (layer2 == null) layer2 = new AuraLayer();
        if (layer3 == null) layer3 = new AuraLayer();
        if (ReferenceEquals(sourceTexture, null)) return;
        if (ReferenceEquals(layer1.sourceTexture, null))
        {
            layer1.sourceTexture = sourceTexture;
            layer1.localOffset = localOffset;
            layer1.localRotation = localRotation;
            layer1.scaleMultiplier = scaleMultiplier;
        }
        sourceTexture = null;
        localOffset = Vector3.zero;
        localRotation = Vector3.zero;
        scaleMultiplier = Vector3.one;
    }
}

public class PlayerAura : MonoBehaviour
{
    [Header("Scene references")]
    public Transform auraAnchor;
    public Camera sceneCamera;
    public Light sunlight;
    public Renderer capsuleRenderer;
    public float transitionSpeed = 2.5f;

    public AuraTheme transformation = new AuraTheme
    {
        skyColor = new Color(0.96f, 0.52f, 0.76f), ambientColor = new Color(0.8f, 0.57f, 0.72f),
        sunlightColor = new Color(1, 0.84f, 0.94f), playerColor = new Color(1, 0.75f, 0.9f)
    };
    public AuraTheme elemental = new AuraTheme
    {
        skyColor = new Color(0.98f, 0.6f, 0.32f), ambientColor = new Color(0.82f, 0.66f, 0.48f),
        sunlightColor = new Color(1, 0.86f, 0.62f), playerColor = new Color(1, 0.82f, 0.58f)
    };
    public AuraTheme boss = new AuraTheme
    {
        skyColor = new Color(0.16f, 0.1f, 0.27f), ambientColor = new Color(0.4f, 0.3f, 0.54f),
        sunlightColor = new Color(0.8f, 0.65f, 1), playerColor = new Color(0.74f, 0.57f, 0.95f)
    };

    public AuraKind CurrentAura { get; private set; }
    public GameObject ActiveEffect => activeEffect;
    GameObject activeEffect;
    GameObject textureAuraTemplate;
    readonly Texture2D[] appliedTextures = new Texture2D[3];
    readonly Vector3[] appliedOffsets = new Vector3[3];
    readonly Vector3[] appliedRotations = new Vector3[3];
    readonly Vector3[] appliedScales = new Vector3[3];
    Material playerMaterial;
    Color defaultSky, defaultAmbient, defaultSunlight, defaultPlayer, defaultFog;
    AuraTheme currentTheme;

    void Awake()
    {
        if (auraAnchor == null) auraAnchor = transform;
        if (sceneCamera == null) sceneCamera = Camera.main;
        defaultSky = sceneCamera != null ? sceneCamera.backgroundColor : Color.gray;
        defaultAmbient = RenderSettings.ambientLight;
        defaultSunlight = sunlight != null ? sunlight.color : Color.white;
        defaultFog = RenderSettings.fogColor;
        if (capsuleRenderer != null)
        {
            playerMaterial = capsuleRenderer.material;
            defaultPlayer = playerMaterial.GetColor("_BaseColor");
        }
    }

    public void Activate(AuraKind kind)
    {
        if (kind == AuraKind.None) { ResetToNeutral(); return; }
        var theme = kind == AuraKind.Transformation ? transformation : kind == AuraKind.Elemental ? elemental : boss;
        bool hasTextures = HasTextures(theme);
        if (CurrentAura == kind && MatchesAppliedLayers(theme)
            && (!hasTextures || activeEffect != null)) return;
        ClearEffect();
        CurrentAura = kind;
        currentTheme = theme;
        CacheLayers(theme);
        if (!hasTextures) return;
        if (textureAuraTemplate == null)
            textureAuraTemplate = Resources.Load<GameObject>("TextureAuraTemplate");
        if (textureAuraTemplate == null) return;

        activeEffect = new GameObject(kind + " Aura");
        activeEffect.transform.SetParent(auraAnchor, false);
        for (int i = 0; i < 3; i++)
        {
            var layer = theme.GetLayer(i);
            if (layer.sourceTexture == null) continue;
            var effect = Instantiate(textureAuraTemplate, activeEffect.transform, false);
            effect.name = "Layer " + (i + 1);
            effect.transform.localPosition = layer.localOffset + Vector3.up;
            effect.transform.localRotation = textureAuraTemplate.transform.localRotation * Quaternion.Euler(layer.localRotation);
            effect.transform.localScale = Vector3.Scale(textureAuraTemplate.transform.localScale, layer.scaleMultiplier);
            var textureSource = effect.GetComponentInChildren<ParticleTextureSource>(true);
            if (textureSource != null)
            {
                textureSource.sourceTexture = layer.sourceTexture;
                textureSource.ApplyTexture();
            }
            effect.SetActive(true);
            foreach (var particles in effect.GetComponentsInChildren<ParticleSystem>()) particles.Play(true);
        }
    }

    static bool HasTextures(AuraTheme theme)
    {
        for (int i = 0; i < 3; i++)
            if (theme.GetLayer(i).sourceTexture != null) return true;
        return false;
    }

    bool MatchesAppliedLayers(AuraTheme theme)
    {
        for (int i = 0; i < 3; i++)
        {
            var layer = theme.GetLayer(i);
            if (appliedTextures[i] != layer.sourceTexture || appliedOffsets[i] != layer.localOffset
                || appliedRotations[i] != layer.localRotation || appliedScales[i] != layer.scaleMultiplier) return false;
        }
        return true;
    }

    void CacheLayers(AuraTheme theme)
    {
        for (int i = 0; i < 3; i++)
        {
            var layer = theme.GetLayer(i);
            appliedTextures[i] = layer.sourceTexture;
            appliedOffsets[i] = layer.localOffset;
            appliedRotations[i] = layer.localRotation;
            appliedScales[i] = layer.scaleMultiplier;
        }
    }

    public void ResetToNeutral()
    {
        ClearEffect();
        CurrentAura = AuraKind.None;
        currentTheme = null;
    }

    void ClearEffect()
    {
        Array.Clear(appliedTextures, 0, appliedTextures.Length);
        if (activeEffect == null) return;
        activeEffect.SetActive(false);
        Destroy(activeEffect);
        activeEffect = null;
    }

    void Update()
    {
        float blend = 1 - Mathf.Exp(-Mathf.Max(0.01f, transitionSpeed) * Time.deltaTime);
        Color sky = currentTheme != null ? currentTheme.skyColor : defaultSky;
        if (sceneCamera != null) sceneCamera.backgroundColor = Color.Lerp(sceneCamera.backgroundColor, sky, blend);
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, currentTheme != null ? sky : defaultFog, blend);
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, currentTheme != null ? currentTheme.ambientColor : defaultAmbient, blend);
        if (sunlight != null) sunlight.color = Color.Lerp(sunlight.color, currentTheme != null ? currentTheme.sunlightColor : defaultSunlight, blend);
        if (playerMaterial != null)
            playerMaterial.SetColor("_BaseColor", Color.Lerp(playerMaterial.GetColor("_BaseColor"), currentTheme != null ? currentTheme.playerColor : defaultPlayer, blend));
    }

    void OnDestroy()
    {
        ClearEffect();
        if (playerMaterial != null) Destroy(playerMaterial);
    }
}