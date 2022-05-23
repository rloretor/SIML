using System;
using Lemmings.Shared;
using UnityEngine;

namespace Lemmings
{
    [Serializable]
    public class LemmingRenderingModel
    {
        public Mesh LemmingTemplateMesh;
        public Transform lemmingTemplate;
        public Shader LemmingInstancedShader;
        public Material LemmingMaterial { get; private set; } = null;
        public Texture2D LemmingAnimationImage;
        public int Frames;

        public void Init(LemmingSimulationModel simulationModel)
        {
            LemmingMaterial = new Material(LemmingInstancedShader)
            {
                name = "Boids",
                hideFlags = HideFlags.HideAndDontSave,
                enableInstancing = true
            };


            LemmingMaterial.SetBuffer("_LemmingsBuffer", simulationModel.SimulationRWBuffer);
            LemmingMaterial.SetInt(SharedVariablesModel.Instances, simulationModel.LemmingInstances);
            LemmingMaterial.SetInt("_animationFrames", Frames);
            LemmingMaterial.SetTexture("_animationTexture", LemmingAnimationImage);
            LemmingMaterial.SetVector(SharedVariablesModel.LemmingSize, lemmingTemplate.localScale);
        }

        public void Dispose()
        {
            LemmingInstancedShader = null;
        }
    }
}