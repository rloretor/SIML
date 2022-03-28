using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lemmings
{
    [Serializable]
    public class LemmingSimulationModel : IDisposable
    {
        public int ThreadGroupSize { get; private set; }

        public RectTransform Bounds;
        public int LemmingInstances;

        private const int GroupSize = 512;

        public ComputeBuffer SimulationRWBuffer { get; private set; }
        private List<LemmingModel> lemmingList = new List<LemmingModel>();

        public void Init()
        {
            ThreadGroupSize = Mathf.CeilToInt((float) LemmingInstances / GroupSize);
            PopulateModel();
            CreateComputeBuffer();
        }


        private void PopulateModel()
        {
            lemmingList.Capacity = LemmingInstances;
            for (var i = 0; i < LemmingInstances; i++)
            {
                var pos = Bounds.RandomPointInBounds();
                var vel = Vector2.right;
                lemmingList.Add(new LemmingModel()
                {
                    Position = pos,
                    Velocity = vel,
                });
            }
        }

        private void CreateComputeBuffer()
        {
            var lemmingsByteSize = Marshal.SizeOf(typeof(LemmingModel));
            SimulationRWBuffer = new ComputeBuffer(LemmingInstances, lemmingsByteSize);
            SimulationRWBuffer.SetData(lemmingList);
        }

        public void Dispose()
        {
            SimulationRWBuffer?.Dispose();
            lemmingList = null;
        }

        public LemmingModel[] GetFrameSimulationData()
        {
            LemmingModel[] lemmingFrameData = new LemmingModel[LemmingInstances];
            SimulationRWBuffer.GetData(lemmingFrameData);
            return lemmingFrameData;
        }
    }
}