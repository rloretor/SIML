using System.Collections.Generic;
using Lemmings.Shared;
using Test;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;

namespace Lemmings
{
    public class LemmingSimulationCPUController : MonoBehaviour
    {
        public Canvas Canvas;
        public LemmingSimulationModel SimulationModel;
        public GameObject lemmingTemplate;

        public bool DEBUG;
        private List<GameObject> LemmingRepresentation;

        [Header("Scene")] public BitMapSceneModel SceneModel;
        public TerrainSimulationView TerrainSimulationView;

        private TerrainSimulationController terrainController;
        private TerrainVectorDebugController terrainDebugController;

        private bool simulate = false;
        private Texture2D Collision;
        private Texture2D SDF;

        public void OnDrawGizmos()
        {
            Vector2 topRight = SimulationModel.TopRight;
            Vector2 botLeft = SimulationModel.BotLeft;
            Gizmos.DrawLine(topRight, botLeft);
            if (EditorApplication.isPlaying)
            {
                //Gizmos.DrawWireCube((topRight + botLeft) / 2.0f, topRight - botLeft);
                Vector2 pixelSize = (topRight - botLeft) / new Vector2(SDF.width, SDF.height);
                for (int i = 0; i < Collision.width; i++)
                {
                    for (int j = 0; j < Collision.height; j++)
                    {
                        var vector2 = new Vector2(i, j);
                        Vector2 pos = (botLeft + (vector2 + Vector2.one * 0.5f) * pixelSize);

                        var pixelUV = computePixelUV(computeUV(pos), 0.5f);
                        var SDFColor = SDF.GetPixelFromUV(pixelUV);
                        float2 sdf = math.normalize(new float2(SDFColor.r, SDFColor.g)) * SDFColor.b;
                        sdf *= (float2) (topRight - botLeft);

                        if (Collision.GetPixelFromUV(pixelUV).r == 0)
                        {
                            Gizmos.DrawWireCube(pos, pixelSize);
                            Gizmos.DrawWireCube(pos, pixelSize / 10.0f);
                            Gizmos.DrawLine(pos, pos + (Vector2) sdf);
                        }
                    }
                }
            }
        }

        private void Start()
        {
            SetCanvas();
            PrepareTerrainController();
            PrepareLemmings();
        }

        public void Update()
        {
            SimulateLemmings();
        }


        float2 computeUV(float2 position)
        {
            SimulationModel.SimulationShader.SetVector(SharedVariablesModel.MaxBound, SimulationModel.TopRight);
            float2 botLeft = SimulationModel.BotLeft;
            float2 topRight = SimulationModel.TopRight;

            return (position - botLeft) / (topRight - botLeft);
        }

        float2 computePos(float2 uv)
        {
            float2 botLeft = SimulationModel.BotLeft;
            float2 topRight = SimulationModel.TopRight;
            return ((uv * (topRight - botLeft)) + botLeft);
        }

        float2 computePixelUV(float2 uv, float2 d)
        {
            float2 puv = uv;
            float2 _texDimensions = new float2(terrainController.TerrainBitRT.width, terrainController.TerrainBitRT.height);
            puv.x = (Mathf.Floor(uv.x * _texDimensions.x) + d.x) / _texDimensions.x;
            puv.y = (Mathf.Floor(uv.y * _texDimensions.y) + d.y) / _texDimensions.y;
            return puv;
        }

        LemmingsShaderMathUtil.Rect GetLemmingCollision(float2 p)
        {
            float2 s = new float2(lemmingTemplate.transform.localScale.x, lemmingTemplate.transform.localScale.y) / 2;
            float2 d = new float2(1, 0);
            Vector2[] corners = new Vector2[]
            {
                p + s,
                p + d.xy * s.x - d.yx * s.y,
                p - d.xy * s.x + d.yx * s.y,
                p - s,
                p + d.yx * s,
                p - d.yx * s,
                p + d.xy * s,
                p - d.xy * s,
            };
            //Debug.DrawLine(new Vector3(p.x, p.y, 0), corners[0]);
            //Debug.DrawLine(new Vector3(p.x, p.y, 0), corners[1]);
            //Debug.DrawLine(new Vector3(p.x, p.y, 0), corners[2]);
            //Debug.DrawLine(new Vector3(p.x, p.y, 0), corners[3]);
            Vector2 pixelSize = (SimulationModel.TopRight - SimulationModel.BotLeft) / new Vector2(SDF.width, SDF.height);

            Vector2 max = Vector2.one * -1000f;
            Vector2 min = Vector2.one * 1000f;
            float2 puv;
            for (int i = 0; i < corners.Length; i++)
            {
                puv = (computeUV(corners[i]));
                LemmingsShaderMathUtil.Rect testPixel = new LemmingsShaderMathUtil.Rect();
                testPixel.Position = computePos(puv);
                Vector2 pixelPos = computePos(computePixelUV(puv, 0.5f));
                testPixel.Size = pixelSize * 0.1f;
                var pixel = Color.white * Collision.GetPixelFromUV(puv).r;
                if (pixel.r == 0f)
                {
                    testPixel.DebugDrawPixel(pixel);
                    max = Vector2.Max(max, pixelPos + pixelSize / 2);
                    min = Vector2.Min(min, pixelPos - pixelSize / 2);
                }
                else
                {
                    testPixel.DebugDrawPixel(pixel);
                }
            }

            LemmingsShaderMathUtil.Rect collisionPixel = new LemmingsShaderMathUtil.Rect();
            // Debug.DrawLine(min, max);
            collisionPixel.Position = (max + min) / 2.0f;
            collisionPixel.Size = (max - min);
            return collisionPixel;
        }

        LemmingsShaderMathUtil.Rect AdaptColision(LemmingsShaderMathUtil.Rect r)
        {
            float2 puv = computePixelUV(computeUV(r.Position), 0.5f);
            Vector2 pixelPos = computePos(puv);

            var SDFColor = SDF.GetPixelFromUV(puv);
            float2 sdf = math.normalize(new float2(SDFColor.r, SDFColor.g)) * SDFColor.b;
            sdf = sdf * (float2) (SimulationModel.TopRight - SimulationModel.BotLeft);
            LemmingsShaderMathUtil.Rect sdfRect;
            sdfRect.Position = r.Position;
            sdfRect.Size = sdf * 2;
            sdfRect.DebugDrawPixel(Color.cyan);
            r.Size = math.max(r.Size, sdfRect.Size);
            return r;
        }

        private void SimulateLemmings()
        {
            float Gravity = 4.9f;
            for (var index = 0; index < SimulationModel.lemmingList.Count; index++)
            {
                var lemming = SimulationModel.lemmingList[index];
                //float2 p = lemming.Position + lemming.Velocity * Time.deltaTime;
                var pixel = GetLemmingCollision(lemming.Position);
                bool2 collides = pixel.Size != (float2) (Vector2.one * -1000 - Vector2.one * 1000);
                if (collides.x || collides.y)
                {
                    // pixel = AdaptColision(pixel);
                    pixel.DebugDrawPixel(Color.blue);
                    pixel.Size += new float2(lemmingTemplate.transform.localScale.x, lemmingTemplate.transform.localScale.y) / 2.0f;
                    pixel.DebugDrawPixel(Color.yellow);
                    FixCollision(ref lemming, pixel);
                    if (DEBUG)
                        Debug.Break();
                }
                else
                {
                    lemming.Acceleration = -new float2(0, Gravity);
                    lemming.Velocity += lemming.Acceleration * Time.deltaTime;
                    lemming.Position += lemming.Velocity * Time.deltaTime;
                    lemming.Acceleration = float2.zero;
                }

                //lemming.Position = this.SimulationModel.SpawnPoints[0].transform.position;
                //FixOutOfBounds(ref lemming, uv);
                SimulationModel.lemmingList[index] = lemming;
                LemmingRepresentation[index].transform.position = SimulationModel.lemmingList[index].Position;
                // LemmingRepresentation[index].gameObject.GetComponent<RawImage>().color = pixel.r == 0f ? Color.magenta : Color.green;
            }
        }

        void FixCollision(ref LemmingKinematicModel lemming, LemmingsShaderMathUtil.Rect pixel)
        {
            LemmingsShaderMathUtil util = new LemmingsShaderMathUtil();
            float t = 0;
            float2 n = 0;
            bool c = util.Ray2Rect(pixel, lemming.Position, lemming.Velocity * Time.deltaTime, out t, out n);
            if (c)
            {
                LemmingsShaderMathUtil.Rect newPos;
                var newPosPosition = lemming.Position + lemming.Velocity * Time.deltaTime * t;
                newPos.Position = newPosPosition;
                newPos.Size = pixel.Size;
                newPos.DebugDrawPixel(Color.green);
                lemming.Position = newPosPosition;
                lemming.Velocity = float2.zero;

                lemming.Velocity = n;
                lemming.Velocity = n.yx;
                lemming.Velocity.y *= 0.2f;
                lemming.Velocity = (lemming.Velocity).normalized * Mathf.Clamp((lemming.Velocity).magnitude, 0.1f, 10f);
                lemming.Velocity += lemming.Acceleration;
            }

            lemming.Position = lemming.Position + lemming.Velocity * Time.deltaTime;
            //  Debug.DrawLine(lemming.Position, lemming.Position + lemming.Velocity, Color.magenta);
        }

        void FixCollision(ref LemmingKinematicModel lemming, float2 uv)
        {
            LemmingsShaderMathUtil util = new LemmingsShaderMathUtil();
            LemmingsShaderMathUtil.Rect pixel;
            float2 pixelUV = computePixelUV(uv, 0.5f);
            pixel.Position = computePos(pixelUV);
            // Color32 sdfcolor = SDF.GetPixel(Mathf.RoundToInt(uv.x * SDF.width), Mathf.RoundToInt(uv.y * SDF.height), 0);


            pixel.Size = (SimulationModel.TopRight - SimulationModel.BotLeft) / new Vector2(SDF.width, SDF.height);
            pixel.Size += new float2(lemmingTemplate.transform.localScale.x, lemmingTemplate.transform.localScale.y) / 2.0f;
            pixel.DebugDrawPixel(Color.cyan);
            float t = 0;
            float2 n = 0;
            //    Debug.DrawLine(lemming.Position, pixel.V3Pos());
            bool c = util.Ray2Rect(pixel, lemming.Position, lemming.Velocity * Time.deltaTime, out t, out n);
            if (c)
            {
                LemmingsShaderMathUtil.Rect newPos;
                newPos.Position = lemming.Position + lemming.Velocity * Time.deltaTime * t * 1.1f;
                newPos.Size = pixel.Size;
                newPos.DebugDrawPixel(Color.green);
                lemming.Position = lemming.Position + lemming.Velocity * Time.deltaTime * t * 1.1f;
                lemming.Velocity = float2.zero;
                n = n.yx;
                lemming.Velocity = n;
                lemming.Velocity.y *= 0.2f;
                lemming.Velocity = (lemming.Velocity).normalized * Mathf.Clamp((lemming.Velocity).magnitude, 0.1f, 10f);
                lemming.Velocity += lemming.Acceleration;
            }
        }

        void FixOutOfBounds(ref LemmingKinematicModel lemming, float2 posUV)
        {
            lemming.Velocity.x = posUV.x >= 1 ? -lemming.Velocity.x : lemming.Velocity.x;
            lemming.Velocity.x = posUV.x <= 0 ? -lemming.Velocity.x : lemming.Velocity.x;
            lemming.Velocity.y = posUV.y <= 0 ? 0 : lemming.Velocity.y;
            float2 pos = lemming.Position;
            pos.x = Mathf.Clamp(lemming.Position.x, SimulationModel.BotLeft.x, SimulationModel.TopRight.x);
            pos.y = Mathf.Clamp(lemming.Position.y, SimulationModel.BotLeft.y, SimulationModel.TopRight.y);
            lemming.Position = pos;
        }


        private void PrepareLemmings()
        {
            LemmingRepresentation = new List<GameObject>();
            for (int i = 0; i < SimulationModel.LemmingInstances; i++)
            {
                var instance = GameObject.Instantiate(lemmingTemplate);
                LemmingRepresentation.Add(instance);
                instance.transform.position = SimulationModel.lemmingList[i].Position;
            }
        }

        private void SetCanvas()
        {
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = Camera.main;
        }

        private void PrepareTerrainController()
        {
            terrainController = new TerrainSimulationController();
            terrainDebugController = new TerrainVectorDebugController();
            SceneModel sceneModel = new SDFSceneModel();
            sceneModel.Init(SceneModel.SceneBitMap.width, SceneModel.SceneBitMap.height);
            SimulationModel.Init();
            terrainController.Init(SceneModel, TerrainSimulationView, SimulationModel.Bounds);
            //terrainDebugController.Init(terrainController, RenderingModel.LemmingMesh, SimulationModel.Bounds);

            Collision = terrainController.TerrainBitRT.toTexture2D();
            SDF = terrainController.TerrainAnalysis.toTexture2D(terrainController.TerrainAnalysis.graphicsFormat);
        }
    }
}