using System;
using UnityEditor;
using UnityEngine;

namespace JFA.editor
{
    [Serializable]
    public class JFA2DTextureDrivenSeedController : JFA2DSeedController
    {
        Texture2D sourceTexture;

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
            }
        }

        public override void PaintSeeds()
        {
            base.PaintSeeds();

            Color[] pixelBuffer = sourceTexture.GetPixels();
            for (int i = 0; i < pixelBuffer.Length; i++)
            {
                Color c = pixelBuffer[i];
                if (c.a == 0)
                {
                    pixelBuffer[i] = new Color(0, 0, 0, 0);
                    continue;
                }

                float row = (float) i / sourceTexture.width;
                float column = (float) i % sourceTexture.width;
                pixelBuffer[i] = new Color((column) / sourceTexture.width, (row) / sourceTexture.height,
                    (float) i / pixelBuffer.Length, 0);
            }

            seed.SetPixels(pixelBuffer);
            seed.Apply();
        }

        public override bool IsTextureReady()
        {
            return sourceTexture != null && sourceTexture.isReadable;
        }
    }
}