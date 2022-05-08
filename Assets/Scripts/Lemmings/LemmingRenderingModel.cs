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

        public void Init(LemmingSimulationModel simulationModel, TerrainSimulationController controller, SceneModel sceneModel)
        {
            LemmingMaterial = new Material(LemmingInstancedShader)
            {
                name = "Boids",
                hideFlags = HideFlags.HideAndDontSave,
                enableInstancing = true
            };


            LemmingMaterial.SetBuffer("_LemmingsBuffer", simulationModel.SimulationRWBuffer);
            LemmingMaterial.SetInt(SharedVariablesModel.Instances, simulationModel.LemmingInstances);
            if (controller != null)
            {
                LemmingMaterial.SetTexture(SharedVariablesModel.collisionBitMap, controller.TerrainBitRT);
                LemmingMaterial.SetVector(SharedVariablesModel.MinBound, sceneModel.bounds.BotLeft());
                LemmingMaterial.SetVector(SharedVariablesModel.MaxBound, sceneModel.bounds.TopRight());
                LemmingMaterial.SetVector(SharedVariablesModel.LemmingSize, lemmingTemplate.localScale);
            }
        }

        public void Dispose()
        {
            LemmingInstancedShader = null;
        }
    }
}