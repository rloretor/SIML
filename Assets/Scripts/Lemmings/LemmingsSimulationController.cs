using UnityEngine;

namespace Lemmings
{
    public class LemmingsSimulationController : MonoBehaviour
    {
        public LemmingSimulationModel Model;
        public ComputeShader SimulationShader;
        private int lemmingsMovementPassKernel;

        private void Start()
        {
            PrepareComputeShader();
        }

        private void Update()
        {
            Simulate();
        }

        private void PrepareComputeShader()
        {
            Model.Init();
            lemmingsMovementPassKernel = SimulationShader.FindKernel("LemmingsMovementPassKernel");
            SimulationShader.SetBuffer(lemmingsMovementPassKernel, "_Lemmings", Model.SimulationRWBuffer);

            UpdateComputeShader();
        }

        private void UpdateComputeShader()
        {
            SimulationShader.SetVector("_MaxBound", Model.Bounds.TopRight());
            SimulationShader.SetVector("_MinBound", Model.Bounds.BotLeft());
            SimulationShader.SetFloat("_DeltaTime", Time.deltaTime);
            SimulationShader.SetFloat("_Time", Time.time);
        }

        private void Simulate()
        {
            UpdateComputeShader();
            SimulationShader.Dispatch(lemmingsMovementPassKernel, Model.ThreadGroupSize, 1, 1);
            var simulation = Model.GetFrameSimulationData();
            foreach (var simulated in simulation)
            {
                Debug.DrawLine(simulated.Position, simulated.Position + simulated.Velocity);
            }
        }
    }
}