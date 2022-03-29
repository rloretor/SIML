
using UnityEngine;
using UnityEngine.Rendering;

namespace Lemmings
{
    public class LemmingSimulationController : MonoBehaviour
    {
        public LemmingSimulationModel SimulationModel;
        public LemmingRenderingModel RenderingModel;

        private void Start()
        {
            PrepareComputeShader();
            RenderingModel.Init(SimulationModel);
        }


        private void Update()
        {
            UpdateComputeShader();
            Simulate();
            Draw();
        }

        private void PrepareComputeShader()
        {
            SimulationModel.Init();
            UpdateComputeShader();
        }


        private void UpdateComputeShader()
        {
            SimulationModel.SimulationShader.SetVector("_MaxBound", SimulationModel.Bounds.TopRight());
            SimulationModel.SimulationShader.SetVector("_MinBound", SimulationModel.Bounds.BotLeft());
            SimulationModel.SimulationShader.SetFloat("_DeltaTime", Time.deltaTime);
            SimulationModel.SimulationShader.SetFloat("_Time", Time.time);
        }

        private void Simulate()
        {
            SimulationModel.SimulationShader.Dispatch(SimulationModel.ComputeKernel, SimulationModel.ThreadGroupSize, 1, 1);
        }

        private void Draw()
        {
            Graphics.DrawMeshInstancedProcedural(RenderingModel.LemmingMesh, 0, RenderingModel.LemmingMaterial,
                SimulationModel.Bounds.GetBounds(), SimulationModel.LemmingInstances, null, ShadowCastingMode.Off,
                false);
        }

        private void OnDestroy()
        {
            SimulationModel.Dispose();
            RenderingModel.Dispose();
        }
    }
}