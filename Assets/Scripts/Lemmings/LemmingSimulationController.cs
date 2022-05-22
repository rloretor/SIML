using Lemmings.Shared;
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

        protected TerrainSimulationController terrainController;

        protected bool simulate = false;

        private void Start()
        {
            SetCanvas();
            PrepareTerrainController();
            SimulationModel.Init();
            RenderingModel.Init(SimulationModel, terrainController, SceneModel);
            PrepareSimulator();
        }

        protected virtual void PrepareSimulator()
        {
            UpdateComputeShader();
        }

        protected virtual void PrepareTerrainController()
        {
            terrainController = new TerrainSimulationController();
            SceneModel sceneModel = new SDFSceneModel();
            sceneModel.Init(SceneModel.SceneBitMap.width, SceneModel.SceneBitMap.height);

            terrainController.Init(SceneModel, TerrainSimulationView, SimulationModel.Bounds);
        }

        private void SetCanvas()
        {
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = Camera.main;
        }


        private void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                simulate = true;
            }

            terrainController.UpdateTerrain();
            Simulate();
            Draw();
            simulate = false;
        }


        private void UpdateComputeShader()
        {
            SimulationModel.SimulationShader.SetBool("_Simulate", this.simulate);
            SimulationModel.SimulationShader.SetVector(SharedVariablesModel.MaxBound, SimulationModel.TopRight);
            SimulationModel.SimulationShader.SetVector(SharedVariablesModel.MinBound, SimulationModel.BotLeft);
            SimulationModel.SimulationShader.SetVector(SharedVariablesModel.TexDimensions, new Vector2(terrainController.TerrainAnalysis.width, terrainController.TerrainAnalysis.height));
            SimulationModel.SimulationShader.SetFloat(SharedVariablesModel.DeltaTime, Time.deltaTime);
            SimulationModel.SimulationShader.SetVector(SharedVariablesModel.LemmingSize, RenderingModel.lemmingTemplate.localScale);
            SimulationModel.SimulationShader.SetTexture(SimulationModel.ComputeKernel, SharedVariablesModel.collisionBitMap, terrainController.TerrainBitRT);
            SimulationModel.SimulationShader.SetTexture(SimulationModel.ComputeKernel, SharedVariablesModel.terrainAnalysisTexture, terrainController.TerrainAnalysis);
        }

        protected virtual void Simulate()
        {
            UpdateComputeShader();
            SimulationModel.SimulationShader.Dispatch(SimulationModel.ComputeKernel, SimulationModel.ThreadGroupSize, 1, 1);
        }

        private void Draw()
        {
            Graphics.DrawMeshInstancedProcedural(RenderingModel.LemmingTemplateMesh, 0, RenderingModel.LemmingMaterial,
                SimulationModel.Bounds, SimulationModel.LemmingInstances, null, ShadowCastingMode.Off,
                false);
        }

        private void OnDestroy()
        {
            SimulationModel.Dispose();
            RenderingModel.Dispose();
        }
    }
}