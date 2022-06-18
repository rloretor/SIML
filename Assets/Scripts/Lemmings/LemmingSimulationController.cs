using System;
using System.Diagnostics;
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
        protected TimeLogger timeLogger;

        private void Start()
        {
            timeLogger = new TimeLogger();
            timeLogger.StartRecording("All");
            timeLogger.StartRecording("Init");
            SetCanvas();
            PrepareTerrainController();
            SimulationModel.Init();
            RenderingModel.Init(SimulationModel);
            PrepareSimulator();
            timeLogger.StopRecording("Init");
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
            timeLogger.StartRecording($"Update_{Time.frameCount}");


            terrainController.UpdateTerrain();
            Draw();
            simulate = false;
            timeLogger.StopRecording($"Update_{Time.frameCount}");
        }

        private void FixedUpdate()
        {
            Simulate();
        }


        private void UpdateComputeShader()
        {
            SimulationModel.SimulationShader.SetBool("_Simulate", this.simulate);
            SimulationModel.SimulationShader.SetVector(SharedVariablesModel.MaxBound, SimulationModel.TopRight);
            SimulationModel.SimulationShader.SetVector(SharedVariablesModel.MinBound, SimulationModel.BotLeft);
            SimulationModel.SimulationShader.SetVector(SharedVariablesModel.TexDimensions, new Vector2(terrainController.TerrainAnalysis.width, terrainController.TerrainAnalysis.height));
            SimulationModel.SimulationShader.SetFloat(SharedVariablesModel.DeltaTime, Time.fixedDeltaTime);
            SimulationModel.SimulationShader.SetVector(SharedVariablesModel.LemmingSize, RenderingModel.lemmingTemplate.localScale);
            SimulationModel.SimulationShader.SetTexture(SimulationModel.ComputeKernel, SharedVariablesModel.collisionBitMap, terrainController.TerrainBitRT);
            SimulationModel.SimulationShader.SetTexture(SimulationModel.ComputeKernel, SharedVariablesModel.terrainAnalysisTexture, terrainController.TerrainAnalysis);
        }

        protected virtual void Simulate()
        {
            //timeLogger.StartRecording($"Simulate_{Time.frameCount}");
            UpdateComputeShader();
            SimulationModel.SimulationShader.Dispatch(SimulationModel.ComputeKernel, SimulationModel.ThreadGroupSize, 1, 1);
            //timeLogger.StopRecording($"Simulate_{Time.frameCount}");
        }

        private void Draw()
        {
            Graphics.DrawMeshInstancedProcedural(RenderingModel.LemmingTemplateMesh, 0, RenderingModel.LemmingMaterial,
                SimulationModel.Bounds, SimulationModel.LemmingInstances, null, ShadowCastingMode.Off,
                false);
        }

        private void OnDestroy()
        {
            timeLogger.StopRecording("All");

            var name = this.GetType().Name;
            var instances = SimulationModel.LemmingInstances.ToString();
            timeLogger.ToCSV($"{name}_{instances}");

            timeLogger = null;
            SimulationModel.Dispose();


            RenderingModel.Dispose();
        }
    }
}