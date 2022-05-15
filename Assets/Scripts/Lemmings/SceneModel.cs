using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Lemmings
{
    [Serializable]
    public class BitMapSceneModel : SceneModel
    {
        [SerializeField] protected Texture sceneBitMap;

        public override Texture SceneBitMap => sceneBitMap;

        public override void Init(int width, int height)
        {
        }
    }

    [Serializable]
    public class SDFSceneModel : SceneModel
    {
        public Material Material;

        private RenderTexture sceneBitMap;

        public override Texture SceneBitMap => sceneBitMap;
        public RenderTexture SceneBitMapRT => sceneBitMap;


        public override void Init(int width, int height)
        {
            sceneBitMap = new RenderTexture(width, height, 0, GraphicsFormat.R8G8B8A8_SRGB, 0)
            {
                autoGenerateMips = false,
                antiAliasing = 1
            };

            Material = new Material(Shader.Find("AnalyticalSDFs"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            Graphics.Blit((RenderTexture) sceneBitMap, (RenderTexture) sceneBitMap, Material);
        }
    }


    [Serializable]
    public abstract class SceneModel
    {
        public abstract Texture SceneBitMap { get; }
        public RectTransform bounds;

        public abstract void Init(int width, int height);
    }
}