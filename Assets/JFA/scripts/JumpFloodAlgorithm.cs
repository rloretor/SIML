using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[Serializable]
public class JumpFloodAlgorithmBase<T> where T : Texture
{
    public T SeedTexture => seedTexture;
    private int passID = Shader.PropertyToID("_pass");
    private int maxPassID = Shader.PropertyToID("_maxPasses");

    protected Material JFAMat;
    protected Shader JFA;
    protected T seedTexture;

    protected JFAConfig config;
    protected bool recordProcess => config.RecordProcess;
    protected bool forceMaxPasses => config.ForceMaxPasses;
    protected int maxPasses => config.MaxPasses;
    protected GraphicsFormat format => config.Format;

    public JumpFloodAlgorithmBase(T seedTexture, JFAConfig configParameters)
    {
        this.seedTexture = seedTexture;
        this.config = configParameters;
        InitMaterial();
    }

    public virtual RenderTexture Compute()
    {
        var result = ExecutePasses(ComputePasses(config.ForceMaxPasses, config.MaxPasses));
        return result;
    }

    private RenderTexture ExecutePasses(int passes)
    {
        var height = seedTexture.height;
        var width = seedTexture.width;
        GraphicsFormat format = this.format;
        var source = RenderTexture.GetTemporary(width, height, 0, format);
        var dest = RenderTexture.GetTemporary(width, height, 0, format);
        Shader.SetGlobalInt(maxPassID, passes);

        Graphics.Blit(seedTexture, source);
        for (int passNumber = 0; passNumber < passes; passNumber++)
        {
            Pass(passNumber, source, dest);
            (source, dest) = (dest, source);
            Debug.Log("swap " + passNumber);
        }

        RenderTexture result = new RenderTexture(width, height, 0, format);
        Pass(passes, source, result);
        RenderTexture.active = null;
        source.Release();
        dest.Release();
        return result;
    }

    protected virtual void Pass(int pass, RenderTexture sourceId, RenderTexture destId)
    {
        Shader.SetGlobalInt(passID, pass);
        Graphics.Blit(sourceId, destId, JFAMat);
    }

    private void InitMaterial()
    {
        JFA = Shader.Find("JFA");
        JFAMat = new Material(JFA)
        {
            name = "JFAMat",
            hideFlags = HideFlags.HideAndDontSave
        };
    }

    private int ComputePasses(bool forceMaxPasses, int maxPasses)
    {
        var Height = seedTexture.height;
        var Width = seedTexture.width;
        int passes = Mathf.CeilToInt(Mathf.Log(Mathf.Max(Width, Height), 2f));

        return forceMaxPasses ? maxPasses : passes;
    }
}