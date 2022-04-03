using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Lemmings
{
    public class TerrainSimulationController : IDisposable
    {
        public RenderTexture TerrainBitRT;
        public RenderTexture TerrainAnalysis;

        private SceneModel sceneModel;
        private Material blitTo8alphaMat;
        private Material blit8alphaToSeedMat;
        private Material JFA_Analysis;
        private JumpFloodAlgorithmBase<RenderTexture> JFAController;

        private bool needsTerrainUpdate = false;

        public void Init(SceneModel sceneModel, TerrainSimulationView view)
        {
            this.sceneModel = sceneModel;
            PrepareMaterials();
            PrepareTerrainRenderTextures();
            FillRenderTextures(true);
            FillRenderTextures(false);

            needsTerrainUpdate = true;
            UpdateTerrain();
            view.TerrainView.texture = TerrainAnalysis;
        }

        private void PrepareMaterials()
        {
            blit8alphaToSeedMat = new Material(Shader.Find("BlitSeeds"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            JFA_Analysis = new Material(Shader.Find("JFA_Analysis"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            blitTo8alphaMat = new Material(Shader.Find("BlitAlpha8"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private void PrepareTerrainRenderTextures()
        {
            var terrainBitmapHeight = sceneModel.TerrainBitmap.height;
            var terrainBitmapWidth = sceneModel.TerrainBitmap.width;

            TerrainBitRT = new RenderTexture(terrainBitmapWidth, terrainBitmapHeight, 0, RenderTextureFormat.R8);
            TerrainAnalysis = new RenderTexture(terrainBitmapWidth, terrainBitmapHeight, 0, GraphicsFormat.R8G8B8A8_SNorm);
        }

        private void FillRenderTextures(bool isReversedBitMap)
        {
            var terrainBitmapHeight = sceneModel.TerrainBitmap.height;
            var terrainBitmapWidth = sceneModel.TerrainBitmap.width;
            RenderTexture seedTexture = RenderTexture.GetTemporary(terrainBitmapWidth, terrainBitmapHeight, 0, GraphicsFormat.R8G8B8A8_UNorm);
            RenderTexture terrainVoronoi = RenderTexture.GetTemporary(terrainBitmapWidth, terrainBitmapHeight, 0, GraphicsFormat.R8G8B8A8_UNorm);

            blitTo8alphaMat.SetInt("isReversedBitmap", isReversedBitMap ? 1 : 0);
            Graphics.Blit(sceneModel.TerrainBitmap, TerrainBitRT, blitTo8alphaMat);
            Graphics.Blit(TerrainBitRT, seedTexture, blit8alphaToSeedMat);
            JFAController = new JumpFloodAlgorithmBase<RenderTexture>(seedTexture, new JFAConfig() {Format = seedTexture.graphicsFormat});

            terrainVoronoi = JFAController.Compute();

            JFA_Analysis.SetInt("pack", 1);
            JFA_Analysis.SetInt("negative", isReversedBitMap ? -1 : 1);
            JFA_Analysis.SetTexture("_BitMap", TerrainBitRT);
            Graphics.Blit(terrainVoronoi, TerrainAnalysis, JFA_Analysis);
        }


        public void UpdateTerrain()
        {
            if (!needsTerrainUpdate) return;
            //TerrainVoronoi = JFAController.Compute(); 
            needsTerrainUpdate = false;
        }

        public void Dispose()
        {
            TerrainBitRT?.Release();
            TerrainAnalysis?.Release();
        }
    }
}