using System.Collections.Generic;
using Lemmings.Shared;
using UnityEngine;

namespace Lemmings
{
    public class TerrainVectorDebugController
    {
        private TerrainSimulationController terrainSimulationController;
        private Material RenderQuadMaterial;
        private Mesh quad;
        private RectTransform bounds;

        public void Init(TerrainSimulationController terrainSimulationController, Mesh mesh, RectTransform bounds)
        {
            this.quad = mesh;
            this.bounds = bounds;
            this.terrainSimulationController = terrainSimulationController;
            Shader shader = Shader.Find("Instanced/DebugDrawTerrain");
            RenderQuadMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave, enableInstancing = true, renderQueue = 5000
            };
        }

        public void DrawDebug()
        {
            int width = terrainSimulationController.TerrainAnalysis.width;
            int height = terrainSimulationController.TerrainAnalysis.height;
            int instances = width * height;
            RenderQuadMaterial.SetInt("_instances", instances);
            RenderQuadMaterial.SetFloat("_width", width);
            RenderQuadMaterial.SetFloat("_height", height);
            RenderQuadMaterial.SetTexture(SharedVariablesModel.terrainAnalysisTexture, terrainSimulationController.TerrainAnalysis);
            RenderQuadMaterial.SetTexture(SharedVariablesModel.collisionBitMap, terrainSimulationController.TerrainBitRT);
            RenderQuadMaterial.SetVector(SharedVariablesModel.MinBound, bounds.BotLeft());
            RenderQuadMaterial.SetVector(SharedVariablesModel.MaxBound, bounds.TopRight());
            Graphics.DrawMeshInstancedProcedural(quad, 0, RenderQuadMaterial, bounds.GetBounds(), instances);
        }
    }
}