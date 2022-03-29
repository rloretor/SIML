using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Lemmings
{
    [Serializable]
    public class LemmingSimulationModel
    {
        public ComputeShader SimulationShader;
        public int ThreadGroupSize { get; private set; }

        public RectTransform Bounds;
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
                var pos = Bounds.RandomPointInBounds();
                var vel = Vector2.right;
                lemmingList.Add(new LemmingKinematicModel()
                {
                    Position = pos,
                    Velocity = vel,
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