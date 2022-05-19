using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Lemmings
{
    public class TerrainSimulationController : IDisposable
    {
        private ComputeShader DebugVectorTerrainViewShader;

        public RenderTexture TerrainBitRT;
        public RenderTexture TerrainAnalysis;

        private SceneModel sceneModel;
        private Material blitTo8alphaMat;
        private Material blit8rToSeedMat;
        private Material UDFMat;
        private Material BlurMat;
        private JumpFloodAlgorithmBase<RenderTexture> JFAController;

        private bool needsTerrainUpdate = false;
        private Bounds bounds;

        public void Init(SceneModel sceneModel, TerrainSimulationView view, Bounds simulationModelBounds)
        {
            Init(sceneModel);
            view.TerrainBitmapViewer.texture = TerrainBitRT;
            view.JFAResultViewer.texture = TerrainAnalysis;
            this.bounds = simulationModelBounds;
        }

        public void Init(SceneModel sceneModel)
        {
            this.sceneModel = sceneModel;

            PrepareMaterials();
            PrepareTerrainRenderTextures();
            for (var i = 0; i < 2; i++)
            {
                FillRenderTextures(i);
            }

            // ApplyBlur(TerrainAnalysis, TerrainAnalysis);

            needsTerrainUpdate = true;
            UpdateTerrain();
        }


        private void PrepareMaterials()
        {
            blitTo8alphaMat = new Material(Shader.Find("BlitAlpha8"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            blit8rToSeedMat = new Material(Shader.Find("BlitSeeds"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            BlurMat = new Material(Shader.Find("FastBlur"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            UDFMat = new Material(Shader.Find("UDF"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private void PrepareTerrainRenderTextures()
        {
            var terrainBitmapHeight = sceneModel.SceneBitMap.height;
            var terrainBitmapWidth = sceneModel.SceneBitMap.width;

            TerrainBitRT = new RenderTexture(terrainBitmapWidth, terrainBitmapHeight, 0, RenderTextureFormat.R8)
            {
                autoGenerateMips = false,
                antiAliasing = 1,
                filterMode = FilterMode.Point
            };
            TerrainAnalysis = new RenderTexture(terrainBitmapWidth, terrainBitmapHeight, 0, GraphicsFormat.R32G32B32A32_SFloat)
            {
                autoGenerateMips = false,
                antiAliasing = 1,
                filterMode = FilterMode.Point
            };
        }

        private void FillRenderTextures(int bitOnValue)
        {
            var terrainBitmapHeight = sceneModel.SceneBitMap.height;
            var terrainBitmapWidth = sceneModel.SceneBitMap.width;
            RenderTexture seedTexture = RenderTexture.GetTemporary(terrainBitmapWidth, terrainBitmapHeight, 0, GraphicsFormat.R8G8B8A8_UNorm, 1);
            RenderTexture terrainVoronoi = RenderTexture.GetTemporary(terrainBitmapWidth, terrainBitmapHeight, 0, GraphicsFormat.R8G8B8A8_UNorm, 1);

            blitTo8alphaMat.SetInt("bitOnValue", bitOnValue);

            Graphics.Blit(sceneModel.SceneBitMap, TerrainBitRT, blitTo8alphaMat); // converts white-black to black-red or red-black depending on the flag
            Graphics.Blit(TerrainBitRT, seedTexture, blit8rToSeedMat); //fills seeds wherever the bitmap returns 1

            JFAController = new JumpFloodAlgorithmBase<RenderTexture>(seedTexture, new JFAConfig() {Format = seedTexture.graphicsFormat});
            terrainVoronoi = JFAController.Compute();

            UDFMat.SetTexture("_BitMap", TerrainBitRT);
            UDFMat.SetInt("isNegative", bitOnValue * 2 - 1);
            Graphics.Blit(terrainVoronoi, TerrainAnalysis, UDFMat);
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


        public void ApplyBlur(Texture source, RenderTexture targetTexture, int BlurIterations = 2, float BlurSize = 1f, int DownSample = 0, Material overlayMaterial = null)
        {
            float widthMod = 1.0f / (1.0f * (1 << DownSample));

            BlurMat.SetVector("_Parameter", new Vector2(BlurSize * widthMod, -BlurSize * widthMod));

            int rtW = source.width;
            int rtH = source.height;

            RenderTexture bufferRenderTexture = RenderTexture.GetTemporary(rtW, rtH, 0, targetTexture.format);
            bufferRenderTexture.filterMode = FilterMode.Point;
            bufferRenderTexture.wrapMode = TextureWrapMode.Clamp;
            Graphics.Blit(source, bufferRenderTexture, BlurMat, 0);

            for (int i = 0; i < BlurIterations; i++)
            {
                float iterationOffs = (i * 1.0f);
                BlurMat.SetVector("_Parameter", new Vector4(BlurSize * widthMod + iterationOffs, -BlurSize * widthMod - iterationOffs, 0.0f, 0.0f));

                // vertical blur
                RenderTexture blurRenderTexture = RenderTexture.GetTemporary(rtW, rtH, 0, targetTexture.format);
                blurRenderTexture.filterMode = FilterMode.Bilinear;
                Graphics.Blit(bufferRenderTexture, blurRenderTexture, BlurMat, 1);
                RenderTexture.ReleaseTemporary(bufferRenderTexture);
                bufferRenderTexture = blurRenderTexture;

                // horizontal blur
                blurRenderTexture = RenderTexture.GetTemporary(rtW, rtH, 0, targetTexture.format);
                blurRenderTexture.filterMode = FilterMode.Bilinear;
                Graphics.Blit(bufferRenderTexture, blurRenderTexture, BlurMat, 2);
                RenderTexture.ReleaseTemporary(bufferRenderTexture);
                bufferRenderTexture = blurRenderTexture;
            }

            if (overlayMaterial != null)
            {
                RenderTexture blurRenderTexture = RenderTexture.GetTemporary(rtW, rtH, 0, targetTexture.format);
                Graphics.Blit(bufferRenderTexture, blurRenderTexture, overlayMaterial);
                RenderTexture.ReleaseTemporary(bufferRenderTexture);
                bufferRenderTexture = blurRenderTexture;
            }

            targetTexture.DiscardContents();

            //blit into persistent texture
            Graphics.Blit(bufferRenderTexture, targetTexture);

            bufferRenderTexture.DiscardContents();

            RenderTexture.ReleaseTemporary(bufferRenderTexture);
        }
    }
}