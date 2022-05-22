using System.Collections.Generic;
using Test;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;

namespace Lemmings
{
    public class LemmingSimulationCPUController : MonoBehaviour
    {
        public Canvas Canvas;
        public LemmingSimulationModel SimulationModel;
        public LemmingRenderingModel RenderingModel;

        public GameObject lemmingTemplate;

        public bool DEBUG;
        private List<GameObject> LemmingRepresentation;

        [Header("Scene")] public BitMapSceneModel SceneModel;
        public TerrainSimulationView TerrainSimulationView;
        private TerrainSimulationController terrainController;

        private bool simulate = false;
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

        private void Start()
        {
            SetCanvas();
            PrepareTerrainController();
            PrepareLemmings();
            RenderingModel.Init(SimulationModel, terrainController, SceneModel);
        }

        public void Update()
        {
            SimulateLemmings();
            Draw();
        }


        LemmingsShaderMathUtil.Rect GetLemmingCollision(LemmingKinematicModel lemming)
        {
            Vector3 transformLocalScale = lemmingTemplate.transform.localScale;
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
                pixel = Color.white * GetPixelFromUV(Collision, puv).r;
                if (pixel.r != 0f) continue;

                var sdfColor = GetPixelFromUV(SDF, puv);
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
            return collisionPixel;
        }

        private void SimulateLemmings()
        {
            for (var index = 0; index < SimulationModel.lemmingList.Count; index++)
            {
                //LemmingRepresentation[index].transform.position = SimulationModel.lemmingList[index].Position;
                var lemming = SimulationModel.lemmingList[index];

                float2 p = lemming.Position + lemming.Velocity * Time.deltaTime;
                var pixel = GetLemmingCollision(lemming);
                bool2 collides = pixel.Size != (float2) (Vector2.one * -1000 - Vector2.one * 1000);
                if (collides.x || collides.y)
                {
                    LemmingsShaderMathUtil.FixCollision(p, lemming.Position, pixel, (float2) ((Vector2) lemmingTemplate.transform.localScale), ref lemming);
                    if (DEBUG)
                        Debug.Break();
                }

                lemming.Acceleration = -new float2(0, Gravity);
                lemming.Velocity += lemming.Acceleration * Time.deltaTime;
                lemming.Position += lemming.Velocity * Time.deltaTime;
                lemming.Acceleration = float2.zero;


                FixOutOfBounds(ref lemming, ComputeUV(lemming.Position));
                SimulationModel.lemmingList[index] = lemming;
            }

            SimulationModel.SimulationRWBuffer.SetData(SimulationModel.lemmingList);
        }

        private void Draw()
        {
            Graphics.DrawMeshInstancedProcedural(RenderingModel.LemmingTemplateMesh, 0, RenderingModel.LemmingMaterial,
                SimulationModel.Bounds, SimulationModel.LemmingInstances, null, ShadowCastingMode.Off,
                false);
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

        private void PrepareLemmings()
        {
            //LemmingRepresentation = new List<GameObject>();
            //for (int i = 0; i < SimulationModel.LemmingInstances; i++)
            //{
            //    var instance = GameObject.Instantiate(lemmingTemplate);
            //    LemmingRepresentation.Add(instance);
            //    instance.transform.position = SimulationModel.lemmingList[i].Position;
            //}
        }

        private void SetCanvas()
        {
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = Camera.main;
        }

        private void PrepareTerrainController()
        {
            terrainController = new TerrainSimulationController();
            SceneModel sceneModel = new SDFSceneModel();
            sceneModel.Init(SceneModel.SceneBitMap.width, SceneModel.SceneBitMap.height);
            SimulationModel.Init();
            terrainController.Init(SceneModel, TerrainSimulationView, SimulationModel.Bounds);

            Collision = terrainController.TerrainBitRT.toTexture2D();
            CollisionColors = Collision.GetPixels();
            SDF = terrainController.TerrainAnalysis.toTexture2D(terrainController.TerrainAnalysis.graphicsFormat);
            SDFColors = SDF.GetPixels();
        }

        private static Color GetPixelFromUV(Texture2D rTex, float2 uv)
        {
            int x = Mathf.FloorToInt(uv.x * rTex.width);
            int y = Mathf.FloorToInt(uv.y * rTex.height);
            return rTex.GetPixel(x, y);
        }

        private void OnDestroy()
        {
            SimulationModel.Dispose();
            RenderingModel?.Dispose();
        }
    }
}