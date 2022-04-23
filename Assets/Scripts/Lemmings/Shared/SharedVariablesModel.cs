using UnityEngine;

namespace Lemmings.Shared
{
    public static class SharedVariablesModel
    {
        public static int TexDimensions = Shader.PropertyToID("_texDimensions");
        public static int MaxBound = Shader.PropertyToID("_MaxBound");
        public static int MinBound = Shader.PropertyToID("_MinBound");
        public static int DeltaTime = Shader.PropertyToID("_DeltaTime");
        public static int Instances = Shader.PropertyToID("_Instances");
        public static int collisionBitMap = Shader.PropertyToID("_collisionBitMap");
        public static int terrainAnalysisTexture = Shader.PropertyToID("_terrainAnalysisTexture");
    }
}