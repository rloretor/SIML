using System;
using UnityEngine;

namespace Lemmings
{
    [Serializable]
    public class SceneModel
    {
        [SerializeField] private Texture2D terrainBitmap;

        public Texture2D TerrainBitmap
        {
            get => terrainBitmap;
            private set => terrainBitmap = value;
        }
    }
}