using Lemmings;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;


public class TerrainTester : MonoBehaviour
{
    public RawImage source;
    public RawImage sourceTruth;
    public RawImage approximation;
    public int width;
    public int height;

    private void Start()
    {
        SDFSceneModel model = new SDFSceneModel();
        model.Init(width, height);
        RenderTexture SDF = new RenderTexture(width, height, 0, GraphicsFormat.R8G8B8A8_SRGB, 0)
        {
            autoGenerateMips = false,
            antiAliasing = 1
        };

        model.Material.SetInt("ignoreAlpha", 1);
        Graphics.Blit(SDF, SDF, model.Material);


        TerrainSimulationController controller = new TerrainSimulationController();
        controller.Init(model);
        source.texture = model.SceneBitMap;
        sourceTruth.texture = SDF;
        approximation.texture = controller.TerrainAnalysis;
    }
}

public static class RenderTextureExtensions
{
    public static Texture2D toTexture2D(this RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
}