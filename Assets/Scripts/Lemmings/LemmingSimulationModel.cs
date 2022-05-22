using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Lemmings
{
    [Serializable]
    public class LemmingSimulationModel
    {
        public int ThreadGroupSize { get; private set; }
        public ComputeShader SimulationShader;
        [SerializeField] private Transform topRight;
        [SerializeField] private Transform botLeft;
        public Transform[] SpawnPoints;
        public int LemmingInstances;

        [HideInInspector] public Bounds Bounds;

        public Vector2 TopRight => topRight.position;
        public Vector2 BotLeft => botLeft.position;

        private const int GroupSize = 512;

        public ComputeBuffer SimulationRWBuffer { get; private set; }
        public List<LemmingKinematicModel> lemmingList = new List<LemmingKinematicModel>();
        public int ComputeKernel { get; private set; }


        public void Init()
        {
            Bounds = new Bounds((BotLeft + TopRight) * 0.5f, TopRight - BotLeft);
            ThreadGroupSize = Mathf.CeilToInt((float) LemmingInstances / GroupSize);
            PopulateModel();
            CreateComputeBuffer();
            PrepareComputeShader();
        }

        private void PrepareComputeShader()
        {
            ComputeKernel = SimulationShader.FindKernel("LemmingsMovementPassKernel");
            SimulationShader.SetBuffer(ComputeKernel, "_Lemmings", SimulationRWBuffer);
        }

        private void PopulateModel()
        {
            lemmingList.Capacity = LemmingInstances;
            for (var i = 0; i < LemmingInstances; i++)
            {
                var spawnPointId = (int) (i % (SpawnPoints.Length));
                var pos = ((RectTransform) SpawnPoints[spawnPointId].transform).UV(new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f))); //  Bounds.UV(new Vector2(0.5f, 0.5f)); //Bounds.RandomPointInBounds();
                var vel = UnityEngine.Random.insideUnitCircle + Vector2.one * UnityEngine.Random.Range(0, 1.0f);

                lemmingList.Add(new LemmingKinematicModel()
                {
                    Position = pos,
                    Velocity = vel,
                    Acceleration = Vector2.zero
                });
            }
        }

        private void CreateComputeBuffer()
        {
            var lemmingsByteSize = Marshal.SizeOf(typeof(LemmingKinematicModel));
            SimulationRWBuffer = new ComputeBuffer(LemmingInstances, lemmingsByteSize);
            SimulationRWBuffer.SetData(lemmingList);
        }


        public LemmingKinematicModel[] GetFrameSimulationData()
        {
            LemmingKinematicModel[] lemmingFrameData = new LemmingKinematicModel[LemmingInstances];
            SimulationRWBuffer.GetData(lemmingFrameData);
            return lemmingFrameData;
        }

        public void Dispose()
        {
            SimulationShader = null;
            SimulationRWBuffer?.Release();
            SimulationRWBuffer?.Dispose();
            lemmingList = null;
        }
    }
}