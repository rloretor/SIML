using System;
using Lemmings;
using Test;
using Unity.Mathematics;
using UnityEngine;
using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;

namespace LemmingsCPU
{
    public class LemmingSimulationCPUController : LemmingSimulationController
    {
        public bool DEBUG;
        private Texture2D Collision;
        private Texture2D SDF;
        private Color[] CollisionColors;
        private Color[] SDFColors;
        const float Gravity = 4.9f;


/*
        public void OnDrawGizmos()
        {
            Vector2 topRight = SimulationModel.TopRight;
            Vector2 botLeft = SimulationModel.BotLeft;
            Gizmos.DrawLine(topRight, botLeft);
            if (EditorApplication.isPlaying)
            {
                //Gizmos.DrawWireCube((topRight + botLeft) / 2.0f, topRight - botLeft);
                Vector2 pixelSize = (topRight - botLeft) / new Vector2(SDF.width, SDF.height);

                for (int i = 0; i < LemmingRepresentation.Count; i++)
                {
                    var lemming = LemmingRepresentation[i];
                    for (int j = -5; j < 7; j++)
                    {
                        for (int k = -5; k < 7; k++)
                        {
                            var pixelUV = computePos(computePixelUV(computeUV((Vector2) lemming.transform.position), 0.5f));
                            Vector2 pos = pixelUV + (float2) Vector2.right * pixelSize.x * j + (float2) Vector2.up * pixelSize.y * k;
                            Gizmos.DrawWireCube(pos, pixelSize);
                            pixelUV = computePixelUV(computeUV(pos), 0.5f);
                            pixelUV = math.clamp(pixelUV, 0, 1);
                            SDF.GetPixelFromUV(pixelUV);
                            if (Collision.GetPixelFromUV(pixelUV).r == 0)
                            {
                                var SDFColor = SDF.GetPixelFromUV(pixelUV);
                                float2 sdf = math.normalize(new float2(SDFColor.r, SDFColor.g)) * SDFColor.b;
                                sdf *= (float2) (topRight - botLeft);
                                // Gizmos.DrawWireCube(pos, pixelSize);
                                //  Gizmos.DrawWireCube(pos, pixelSize / 10.0f);

                                Gizmos.DrawLine(pos, pos + (Vector2) sdf);
                            }
                        }
                    }
                }
            }
        }*/
        LemmingsShaderMathUtil.Rect GetLemmingCollision(LemmingKinematicModel lemming)
        {
            timeLogger.StartRecording($"GetLemmingCollision_{Time.frameCount}");
            Vector3 transformLocalScale = RenderingModel.lemmingTemplate.transform.localScale;
            float2 s = new float2(transformLocalScale.x, transformLocalScale.y) / 2;
            float2 p = (float2) (lemming.Position + lemming.Velocity * Time.deltaTime);
            float2 c = LemmingsShaderMathUtil.pointInSquarePerimeter(lemming.Position, lemming.Velocity, (float2) lemming.Position - s, (float2) lemming.Position + s);
            float2[] corners = new[] {p + c};

            Vector2 pixelSize = (SimulationModel.TopRight - SimulationModel.BotLeft) / new Vector2(SDF.width, SDF.height);

            Vector2 max = Vector2.one * -1000f;
            Vector2 min = Vector2.one * 1000f;

            float2 puv;
            float2 psdf;
            float2 sdfPixelUV;
            float2 nsdf;
            Vector2 pixelPos;
            Color pixel;
            foreach (var t in corners)
            {
                puv = (ComputeUV(t));

                pixelPos = ComputePos(ComputePixelUV(puv, 0f));
                pixel = Color.white * GetCollisionPixelFromUV(puv).r;
                if (pixel.r != 0f) continue;

                var sdfColor = GetSDFPixelFromUV(puv);
                nsdf = math.normalize(new float2(sdfColor.r, sdfColor.g));
                sdfPixelUV = ComputePixel(ComputePixelUV(puv + nsdf * sdfColor.b, 0.5f));

                sdfPixelUV /= new float2(terrainController.TerrainBitRT.width, terrainController.TerrainBitRT.height);
                psdf = ComputePos(ComputePixelUV(sdfPixelUV, 0f));

                max = Vector2.Max(max, Vector2.Max(pixelPos, psdf));
                min = Vector2.Min(min, Vector2.Min(pixelPos, psdf));
                psdf = math.max(max, min);
                min = math.min(max, min);
                max = psdf;
                max = Vector2.Max(max, pixelPos + pixelSize * 0.5f);
                min = Vector2.Min(min, pixelPos - pixelSize * 0.5f);
            }

            LemmingsShaderMathUtil.Rect collisionPixel = new LemmingsShaderMathUtil.Rect();
            collisionPixel.Position = (max + min) / 2.0f;
            collisionPixel.Size = max - min;
            timeLogger.StopRecording($"GetLemmingCollision_{Time.frameCount}");
            return collisionPixel;
        }

        protected override void Simulate()
        {
            timeLogger.StartRecording($"Simulate_{Time.frameCount}");
            for (var index = 0; index < SimulationModel.lemmingList.Count; index++)
            {
                //LemmingRepresentation[index].transform.position = SimulationModel.lemmingList[index].Position;
                var lemming = SimulationModel.lemmingList[index];

                float2 p = lemming.Position + lemming.Velocity * Time.deltaTime;
                var pixel = GetLemmingCollision(lemming);
                bool2 collides = pixel.Size != (float2) (Vector2.one * -1000 - Vector2.one * 1000);
                if (collides.x || collides.y)
                {
                    timeLogger.StartRecording($"FixCollision_{Time.frameCount}");
                    LemmingsShaderMathUtil.FixCollision(p, lemming.Position, pixel, (float2) ((Vector2) RenderingModel.lemmingTemplate.transform.localScale), ref lemming);
                    timeLogger.StopRecording($"FixCollision_{Time.frameCount}");

                    if (DEBUG)
                        Debug.Break();
                }

                timeLogger.StartRecording($"Kinematic_{Time.frameCount}");
                lemming.Acceleration = -new float2(0, Gravity);
                lemming.Velocity += lemming.Acceleration * Time.deltaTime;
                lemming.Position += lemming.Velocity * Time.deltaTime;
                lemming.Acceleration = float2.zero;
                FixOutOfBounds(ref lemming, ComputeUV(lemming.Position));
                timeLogger.StopRecording($"Kinematic_{Time.frameCount}");

                SimulationModel.lemmingList[index] = lemming;
            }

            timeLogger.StopRecording($"Simulate_{Time.frameCount}");
            SimulationModel.SimulationRWBuffer.SetData(SimulationModel.lemmingList);
        }

        private float2 ComputeUV(float2 position)
        {
            return LemmingsShaderMathUtil.computeUV(position, SimulationModel.BotLeft, SimulationModel.TopRight);
        }

        private float2 ComputePos(float2 uv)
        {
            return LemmingsShaderMathUtil.computePos(uv, SimulationModel.BotLeft, SimulationModel.TopRight);
        }

        private float2 ComputePixelUV(float2 uv, float2 d)
        {
            return LemmingsShaderMathUtil.computePixelUV(uv, d, terrainController.TerrainBitRT.width, terrainController.TerrainBitRT.height);
        }

        private float2 ComputePixel(float2 uv)
        {
            return LemmingsShaderMathUtil.computePixel(uv, terrainController.TerrainBitRT.width, terrainController.TerrainBitRT.height);
        }


        private void FixOutOfBounds(ref LemmingKinematicModel lemming, float2 posUV)
        {
            lemming.Velocity.x = posUV.x >= 1 ? -lemming.Velocity.x : lemming.Velocity.x;
            lemming.Velocity.x = posUV.x <= 0 ? -lemming.Velocity.x : lemming.Velocity.x;
            lemming.Velocity.y = posUV.y <= 0 ? 0 : lemming.Velocity.y;
            float2 pos = lemming.Position;
            pos.x = Mathf.Clamp(lemming.Position.x, SimulationModel.BotLeft.x, SimulationModel.TopRight.x);
            pos.y = Mathf.Clamp(lemming.Position.y, SimulationModel.BotLeft.y, SimulationModel.TopRight.y);
            lemming.Position = pos;
        }

        protected override void PrepareSimulator()
        {
            Collision = terrainController.TerrainBitRT.toTexture2D();
            CollisionColors = Collision.GetPixels();
            SDF = terrainController.TerrainAnalysis.toTexture2D(terrainController.TerrainAnalysis.graphicsFormat);
            SDFColors = SDF.GetPixels();
        }


        private Color GetSDFPixelFromUV(float2 uv)
        {
            int x = Mathf.FloorToInt(uv.x * SDF.width);
            int y = Mathf.FloorToInt(uv.y * SDF.height);
            int pixesln = y * SDF.width + x;
            pixesln = Mathf.Clamp(pixesln, 0, SDF.width * SDF.height - 1);

            return SDFColors[pixesln];
        }

        private Color GetCollisionPixelFromUV(float2 uv)
        {
            int x = Mathf.FloorToInt(uv.x * Collision.width);
            int y = Mathf.FloorToInt(uv.y * Collision.height);
            int pixesln = y * Collision.width + x;
            pixesln = Mathf.Clamp(pixesln, 0, Collision.width * Collision.height - 1);
            return CollisionColors[pixesln];
        }
    }
}