using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Lemmings
{
    public class LemmingSimulationController : MonoBehaviour
    {
        public Canvas Canvas;
        public LemmingSimulationModel SimulationModel;
        public LemmingRenderingModel RenderingModel;

        [Header("Scene")] public SceneModel SceneModel;
        public TerrainSimulationView TerrainSimulationView;

        private TerrainSimulationController terrainController;

        private void Start()
        {
            SetCanvas();
            PrepareTerrainController();
            PrepareComputeShader();
            RenderingModel.Init(SimulationModel);
        }

        private void PrepareTerrainController()
        {
            terrainController = new TerrainSimulationController();
            terrainController.Init(SceneModel, TerrainSimulationView);
        }

        private void SetCanvas()
        {
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = Camera.main;
        }


        private void Update()
        {
            terrainController.UpdateTerrain();
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
            SimulationModel.SimulationShader.SetTexture(SimulationModel.ComputeKernel, "_collisionBitMap", terrainController.TerrainBitRT);
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