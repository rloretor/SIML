using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Lemmings
{
    [Serializable]
    public class LemmingSimulationModel
    {
        public int ThreadGroupSize { get; private set; }
        public ComputeShader SimulationShader;
        public RectTransform Bounds;
        public Transform[] SpawnPoints;
        public int LemmingInstances;

        private const int GroupSize = 512;

        public ComputeBuffer SimulationRWBuffer { get; private set; }
        private List<LemmingKinematicModel> lemmingList = new List<LemmingKinematicModel>();
        public int ComputeKernel { get; private set; }

        public void Init()
        {
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
                var spawnPointId = UnityEngine.Random.Range(0, SpawnPoints.Length);
                var direction = UnityEngine.Random.insideUnitCircle.normalized + Vector2.one * UnityEngine.Random.Range(0, 1.0f);
                var pos = (Vector2) SpawnPoints[spawnPointId].position + direction; //  Bounds.UV(new Vector2(0.5f, 0.5f)); //Bounds.RandomPointInBounds();
                //pos += Bounds.UV(new Vector2(UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f))); // Bounds.RandomPointInBounds();
                var vel = Vector2.right + (Vector2.right * UnityEngine.Random.Range(-2, 2));
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
            lemmingList = null;
        }
    }
}