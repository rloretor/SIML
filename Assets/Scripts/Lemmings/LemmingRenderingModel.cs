using System;
using UnityEngine;

namespace Lemmings
{
    [Serializable]
    public class LemmingRenderingModel
    {
        public Mesh LemmingMesh;
        public Shader LemmingInstancedShader;
        public Material LemmingMaterial { get; private set; } = null;

        public void Init(LemmingSimulationModel simulationModel)
        {
            LemmingMaterial = new Material(LemmingInstancedShader)
            {
                name = "Boids",
                hideFlags = HideFlags.HideAndDontSave,
                enableInstancing = true
            };

            LemmingMaterial.SetBuffer("_LemmingsBuffer", simulationModel.SimulationRWBuffer);
            LemmingMaterial.SetInt("_Instances", simulationModel.LemmingInstances);
        }

        public void Dispose()
        {
            LemmingInstancedShader = null;
        }
    }
}