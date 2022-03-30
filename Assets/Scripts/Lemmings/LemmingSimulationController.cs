using UnityEngine;
using UnityEngine.Rendering;

namespace Lemmings
{
    public class LemmingSimulationController : MonoBehaviour
    {
        public Canvas Canvas;
        public Texture2D collisionBitmap;
        public LemmingSimulationModel SimulationModel;
        public LemmingRenderingModel RenderingModel;

        private void Start()
        {
            SetCanvas();
            PrepareComputeShader();
            RenderingModel.Init(SimulationModel);
        }

        private void SetCanvas()
        {
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = Camera.main;
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
            SimulationModel.SimulationShader.SetTexture(SimulationModel.ComputeKernel, "_collisionBitMap", collisionBitmap);
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