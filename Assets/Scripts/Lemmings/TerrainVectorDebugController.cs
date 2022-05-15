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
        private Vector2 botLeft;
        private Vector2 topRight;
        private Bounds bounds;


        public void Init(TerrainSimulationController terrainSimulationController, Mesh mesh, Vector2 topRight, Vector2 botLeft)
        {
            this.quad = mesh;
            this.topRight = topRight;
            this.botLeft = botLeft;
            this.terrainSimulationController = terrainSimulationController;
            Shader shader = Shader.Find("Instanced/DebugDrawTerrain");
            bounds = new Bounds((botLeft + topRight) / 2.0f, topRight - botLeft);
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
            RenderQuadMaterial.SetVector(SharedVariablesModel.MinBound, botLeft);
            RenderQuadMaterial.SetVector(SharedVariablesModel.MaxBound, topRight);
            Graphics.DrawMeshInstancedProcedural(quad, 0, RenderQuadMaterial, bounds, instances);
        }
    }
}