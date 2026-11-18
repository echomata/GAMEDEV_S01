using System.Text.RegularExpressions;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleTextureSource : MonoBehaviour
{
    public Texture2D sourceTexture;

    MaterialPropertyBlock properties;
    static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    void OnEnable() => ApplyTexture();

    void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += ApplyAfterValidation;
#endif
    }

#if UNITY_EDITOR
    void ApplyAfterValidation()
    {
        if (this != null && isActiveAndEnabled) ApplyTexture();
    }
#endif

    public Vector2Int GetGrid()
    {
        if (sourceTexture == null) return Vector2Int.one;
        var match = Regex.Match(sourceTexture.name, @"(?:^|_)(\d+)[xX](\d+)$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int columns)
            && int.TryParse(match.Groups[2].Value, out int rows) && columns > 0 && rows > 0)
            return new Vector2Int(columns, rows);
        return Vector2Int.one;
    }

    public void ApplyTexture()
    {
        if (!gameObject.scene.IsValid()) return;
        var particles = GetComponent<ParticleSystem>();
        var renderer = GetComponent<ParticleSystemRenderer>();
        if (particles == null || renderer == null) return;

        if (properties == null) properties = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(properties);
        properties.SetTexture(BaseMap, sourceTexture != null ? sourceTexture : Texture2D.blackTexture);
        renderer.SetPropertyBlock(properties);

        Vector2Int grid = GetGrid();
        var sheet = particles.textureSheetAnimation;
        sheet.enabled = grid.x > 1 || grid.y > 1;
        sheet.mode = ParticleSystemAnimationMode.Grid;
        sheet.numTilesX = grid.x;
        sheet.numTilesY = grid.y;
        sheet.animation = ParticleSystemAnimationType.WholeSheet;
        sheet.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 0, 1, 1));
        sheet.startFrame = 0;
        sheet.cycleCount = 1;

        if (Application.isPlaying)
        {
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.Play(true);
        }
    }

    void OnDisable()
    {
        var renderer = GetComponent<ParticleSystemRenderer>();
        if (renderer != null) renderer.SetPropertyBlock(null);
    }
}