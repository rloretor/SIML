using Lemmings.Shared;
using UnityEngine;

public class JumpFloodingAlgorithmTerrain : JumpFloodAlgorithmBase<RenderTexture>
{
    private readonly Vector3 minBound;
    private readonly Vector3 maxBound;

    public JumpFloodingAlgorithmTerrain(RenderTexture seedTexture, JFAConfig configParameters, Vector3 minBound, Vector3 maxBound) : base(seedTexture, configParameters)
    {
        this.minBound = minBound;
        this.maxBound = maxBound;
    }

    //protected override void AddAdditionalParameters()
    //{
    //    Shader.SetGlobalVector(SharedVariablesModel.MaxBound, maxBound);
    //    Shader.SetGlobalVector(SharedVariablesModel.MinBound, minBound);
    //    base.AddAdditionalParameters();
    //}

    // protected override void InitMaterial()
    // {
    //     JFA = Shader.Find("JFA_Terrain");
    //     JFAMat = new Material(JFA)
    //     {
    //         name = "JFAMATTerrain",
    //         hideFlags = HideFlags.HideAndDontSave
    //     };
    // }
}