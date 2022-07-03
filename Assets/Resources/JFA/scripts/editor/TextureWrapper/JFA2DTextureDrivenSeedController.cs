using System;
using PlasticGui;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace JFA.editor
{
    [Serializable]
    public class JFA2DTextureDrivenSeedController : JFA2DSeedController
    {
        private Texture2D sourceTexture;

        public override void DrawProperties()
        {
            GUILayout.BeginHorizontal();
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            style.fixedWidth = 70;
            GUILayout.Label("JFA source", style);
            sourceTexture = (Texture2D) EditorGUILayout.ObjectField(sourceTexture, typeof(Texture2D), false,
                GUILayout.Width(70), GUILayout.Height(70));
            GUILayout.EndHorizontal();

            if (sourceTexture != null)
            {
                if (sourceTexture.isReadable == false)
                {
                    Debug.LogError($"Texture is not readable {sourceTexture.name}");
                    sourceTexture = null;
                    return;
                }

                width = sourceTexture.width;
                height = sourceTexture.height;
                PaintSeeds();
            }
        }

        public override void PaintSeeds(GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm)
        {
            base.PaintSeeds(format);
            RenderTexture r = new RenderTexture(width, height, 0, format);
            Material m = new Material(Shader.Find("BlitSeedsAlpha"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            Graphics.Blit(sourceTexture, r, m);
            seed = r.toTexture2D(format);
            RenderTexture.active = null;
            r.Release();
            Object.Destroy(r);

            //Color[] pixelBuffer = r.toTexture2D(format).ReadPixels();
            //float nSeeds = 0;
            //for (int i = 0; i < pixelBuffer.Length; i++)
            //{
            //    Color c = pixelBuffer[i];
            //    if (c.a == 0)
            //    {
            //        pixelBuffer[i] = new Color(0, 0, 0, 0);
            //        continue;
            //    }

            //    nSeeds++;
            //    float row = Mathf.Floor((float) i / sourceTexture.width);
            //    float column = Mathf.Floor((float) i % sourceTexture.width);
            //    pixelBuffer[i] = new Color((column) / sourceTexture.width, (row) / sourceTexture.height,
            //        nSeeds, 0);
            //}

            //for (int i = 0; i < pixelBuffer.Length; i++)
            //{
            //    pixelBuffer[i] = new Color(pixelBuffer[i].r, pixelBuffer[i].g, pixelBuffer[i].b / nSeeds, pixelBuffer[i].a);
            //}

            // seed.SetPixels(pixelBuffer);
            // seed.Apply();
        }

        public override bool IsTextureReady()
        {
            return sourceTexture != null && sourceTexture.isReadable;
        }
    }
}