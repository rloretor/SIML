using System;
using UnityEngine;

namespace Lemmings
{
    public class TerrainSimulationController : IDisposable
    {
        public RenderTexture TerrainBitRT;
        public RenderTexture TerrainRepulsionFlow;
        public RenderTexture TerrainSDF;
        private SceneModel sceneModel;
        private Material blit8alphaMat;


        public void Init(SceneModel sceneModel, TerrainSimulationView view)
        {
            this.sceneModel = sceneModel;
            TerrainBitRT = new RenderTexture(sceneModel.TerrainBitmap.width, sceneModel.TerrainBitmap.height, 0, RenderTextureFormat.R8);

            blit8alphaMat = new Material(Shader.Find("BlitAlpha8"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            UpdateTerrain();

            view.TerrainView.texture = TerrainBitRT;
        }

        public void UpdateTerrain()
        {
            Graphics.Blit(sceneModel.TerrainBitmap, TerrainBitRT, blit8alphaMat);
        }

        public void Dispose()
        {
            TerrainBitRT.Release();
        }
    }
}