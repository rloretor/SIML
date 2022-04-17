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

        [Header("Scene")] public BitMapSceneModel SceneModel;
        public TerrainSimulationView TerrainSimulationView;

        private TerrainSimulationController terrainController;
        private TerrainVectorDebugController terrainDebugController;

        private void Start()
        {
            SetCanvas();
            PrepareTerrainController();
            PrepareComputeShader();
        }

        private void PrepareTerrainController()
        {
            terrainController = new TerrainSimulationController();
            terrainDebugController = new TerrainVectorDebugController();
            SceneModel sceneModel = new SDFSceneModel();
            sceneModel.Init(SceneModel.SceneBitMap.width, SceneModel.SceneBitMap.height);

            terrainController.Init(SceneModel, TerrainSimulationView);
            terrainDebugController.Init(terrainController, RenderingModel.LemmingMesh, SimulationModel.Bounds);
        }

        private void SetCanvas()
        {
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = Camera.main;
        }


        private void Update()
        {
            RenderingModel.Init(SimulationModel, terrainController, SceneModel);

            terrainController.UpdateTerrain();
            UpdateComputeShader();
            Simulate();
            Draw();
            terrainDebugController.DrawDebug();
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
            SimulationModel.SimulationShader.SetVector("_texDimensions", new Vector2(terrainController.TerrainAnalysis.width, terrainController.TerrainAnalysis.height));
            SimulationModel.SimulationShader.SetFloat("_DeltaTime", Time.deltaTime);
            SimulationModel.SimulationShader.SetFloat("_Time", Time.time);
            SimulationModel.SimulationShader.SetTexture(SimulationModel.ComputeKernel, "_collisionBitMap", terrainController.TerrainBitRT);
            SimulationModel.SimulationShader.SetTexture(SimulationModel.ComputeKernel, "_terrainAnalysisTexture", terrainController.TerrainAnalysis);
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