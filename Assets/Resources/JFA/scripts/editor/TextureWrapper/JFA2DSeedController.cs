using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace JFA.editor
{
    public abstract class JFA2DSeedController
    {
        protected Texture2D seed;
        protected int width;
        protected int height;
        public Texture2D Seed => seed;

        public abstract void DrawProperties();

        public virtual void PaintSeeds(GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm)
        {
            seed = new Texture2D(width, height, format, 0, TextureCreationFlags.None)
            {
                name = "Seed",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp
            };
            seed.filterMode = FilterMode.Point;
        }

        public abstract bool IsTextureReady();

        public int GetMinPasses()
        {
            return Mathf.CeilToInt(Mathf.Log(Mathf.Max(width, height), 2f));
        }
    }
}