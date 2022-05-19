using System;
using System.Collections.Generic;
using System.Numerics;
using Lemmings;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Test.VisualProofs
{
    [Serializable]
    public class LemmingBoxResolutionVisualProofData
    {
        public Transform lookat;
        public Transform box;

        public Vector3 Velocity => lookat.transform.position - box.transform.position;
    }

    public class LemmingBoxResolutionVisualProofs : MonoBehaviour
    {
        public int width;
        public int height;
        public Transform botLeft;
        public Transform topRight;
        public LemmingBoxResolutionVisualProofData lemmingData;
        public LemmingBoxResolutionVisualProofData boxData;

        private void OnDrawGizmos()
        {
            Vector2 PixelSize = (topRight.position - botLeft.position) / (Vector2) new float2(width, height);
            DrawPixels(PixelSize);
            LemmingKinematicModel lemming = new LemmingKinematicModel()
            {
                Position = lemmingData.box.position,
                Acceleration = Vector3.zero,
                Velocity = lemmingData.Velocity
            };
            DrawLemming(lemming, new Color(0.7f, 0, 0, 0.4f));
            LemmingsShaderMathUtil.Rect r = new LemmingsShaderMathUtil.Rect();

            r.Position = (Vector2) boxData.box.position;
            r.Size = (Vector2) boxData.Velocity;
            r.DebugDrawPixel(new Color(1, 0, 1, 0.5f));
            float2 p = lemming.Position + lemming.Velocity;
            DrawLemming(lemming, Color.yellow);
            LemmingsShaderMathUtil.FixCollision(p, lemming.Position, r, (Vector2) lemmingData.box.transform.localScale, ref lemming);
            DrawLemming(lemming, Color.green);
        }


        private void DrawLemming(LemmingKinematicModel lemming, Color c)
        {
            Gizmos.color = c;

            Gizmos.DrawWireCube(lemming.Position, lemmingData.box.transform.localScale);
            Gizmos.DrawLine(lemming.Position + Vector2.right * 0.1f, lemming.Position + Vector2.right * 0.1f + lemming.Velocity);
        }

        private void DrawPixels(Vector2 PixelSize)
        {
            Gizmos.color = Color.gray;
            for (int i = 0; i <= height; i++)
            {
                Gizmos.DrawLine(botLeft.transform.position + Vector3.up * PixelSize.y * i, topRight.transform.position - Vector3.up * PixelSize.y * (height - i));
                for (int j = 0; j <= width; j++)
                {
                    Gizmos.DrawLine(botLeft.transform.position + Vector3.right * PixelSize.x * j, topRight.transform.position - Vector3.right * PixelSize.x * (width - j));
                }
            }
        }
    }
}