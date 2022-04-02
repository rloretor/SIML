using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Lemmings
{
    public class TerrainSimulationController : IDisposable
    {
        public RenderTexture TerrainBitRT;
        public RenderTexture TerrainRepulsionFlow;
        public RenderTexture TerrainSDF;


        public void Init(SceneModel sceneModel)
        {
        }

        public void Dispose()
        {
            TerrainBitRT.Release();
        }
    }
}