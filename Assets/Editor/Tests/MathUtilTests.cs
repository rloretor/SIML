using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Lemmings;
using NUnit.Framework;
using Test;
using Unity.Mathematics;
using UnityEngine;

namespace Editor.Tests
{
    public class MathTests
    {
        LemmingsShaderMathUtil utils;

        struct RayBox
        {
            public Vector2 o;
            public Vector2 D;
            public Vector2 c;
            public Vector2 s;
            public int result;
            public Vector3 resultData;
        }

        [SetUp]
        public void InitTest()
        {
            utils = new LemmingsShaderMathUtil();
        }

        [Test]
        [TestCaseSource(nameof(RayVsRayTestSource))]
        public float Ray2Ray(float2 r0O, float2 r0D, float2 r1O, float2 r1D)
        {
            float t = utils.Ray2Ray(r0O, r0D, r1O, r1D);
            return t;
        }

        [Test]
        [TestCaseSource(nameof(RayVsRayToleranceTestSource))]
        public void Ray2RayTolerance(float2 r0O, float2 r0D, float2 r1O, float2 r1D, float result)
        {
            float t = utils.Ray2Ray(r0O, r0D, r1O, r1D);

            Assert.IsTrue(Math.Abs(result - t) < 0.00001f);
        }

        [Test]
        [TestCaseSource(nameof(RayVsAABB))]
        public bool RayToAABB(float2 r0O, float2 r0D, float2 center, float2 size, float result, float2 normal)
        {
            var r = new LemmingsShaderMathUtil.Rect();
            r.Position = center;
            r.Size = size;
            float t = 0;
            float2 n = float2.zero;
            bool collides = utils.Ray2Rect(r, r0O, r0D, out t, out n);
            if (collides)
            {
                Debug.Log(t);
                Debug.Log(n);
                Assert.IsTrue(Math.Abs(result - t) < 0.00001f);
                Assert.IsTrue(Math.Abs(n.x - normal.x) < 0.00001f);
                Assert.IsTrue(Math.Abs(n.y - normal.y) < 0.00001f);
            }

            return collides;
        }

        [Test]
        [TestCaseSource(nameof(RayVsAABB))]
        public bool RayToAABBGPU(float2 r0O, float2 r0D, float2 center, float2 size, float result, float2 normal)
        {
            ComputeShader shader = (ComputeShader) Resources.Load("TestComputeShader");
            int kernel = shader.FindKernel("Ray2BoxKernel");
            List<RayBox> testData = new List<RayBox>();
            testData.Add(new RayBox()
            {
                o = r0O,
                D = r0D,
                c = center,
                s = size,
                result = 0,
                resultData = Vector3.zero
            });
            int dataByteSize = Marshal.SizeOf(typeof(RayBox));
            ComputeBuffer databuffer = new ComputeBuffer(1, dataByteSize);
            databuffer.SetData(testData);
            shader.SetBuffer(kernel, "_RayBoxBuffer", databuffer);
            shader.Dispatch(kernel, 1, 1, 1);
            var finalData = new RayBox[1];
            databuffer.GetData(finalData);
            databuffer.Dispose();

            var gpuResult = finalData[0];
            Debug.Log(gpuResult);

            var r = new LemmingsShaderMathUtil.Rect();
            r.Position = center;
            r.Size = size;
            float t = 0;
            float2 n = float2.zero;
            bool collides = utils.Ray2Rect(r, r0O, r0D, out t, out n);

            Assert.IsTrue(gpuResult.resultData.x == t);
            Assert.IsTrue(gpuResult.resultData.y == n.x);
            Assert.IsTrue(gpuResult.resultData.z == n.y);

            Assert.IsTrue(collides == (gpuResult.result == 1));

            return gpuResult.result == 1;
        }


        static TestCaseData[] RayVsRayTestSource =
        {
            //same origin
            new TestCaseData(float2.zero, new float2(1, 0), float2.zero, new float2(0, 1)).Returns(0.0f),
            new TestCaseData(float2.zero, new float2(1, 0), float2.zero, new float2(-1, 1)).Returns(0.0f),
            new TestCaseData(float2.zero, new float2(0, 1), float2.zero, new float2(11, 0)).Returns(0.0f),

            //paralel
            new TestCaseData(float2.zero, new float2(0, 0), float2.zero, new float2(2, 2)).Returns(float.NaN),
            new TestCaseData(float2.zero, new float2(0, 1), float2.zero, new float2(0, 1)).Returns(float.NaN),
            new TestCaseData(new float2(0, 1), new float2(1, 1), float2.zero, new float2(1, 1)).Returns(float.NegativeInfinity),
            new TestCaseData(new float2(0, 1), new float2(1, 1), float2.zero, new float2(-1, -1)).Returns(float.PositiveInfinity),

            //orthogonal

            //same origin
            new TestCaseData(float2.zero, new float2(1, 0), new float2(1, 0), new float2(0, 1)).Returns(1f),
            new TestCaseData(float2.zero, new float2(1, 0), new float2(1, 0), new float2(-1, 1)).Returns(1f),
            new TestCaseData(float2.zero, new float2(0, 1), new float2(1, 0), new float2(11, 0)).Returns(0),
            new TestCaseData(new float2(1, 0), new float2(11, 0), float2.zero, new float2(0, 1)).Returns(-1 / 11.0f),
            new TestCaseData(float2.zero, new float2(0, 1), new float2(0, -1), new float2(1, 0)).Returns(-1f),
            new TestCaseData(new float2(0, 1), new float2(1, 0), new float2(1, 0), new float2(0, 1)).Returns(1f),
            new TestCaseData(new float2(0, 1), new float2(2, 0), new float2(1, 0), new float2(0, 1)).Returns(0.5),
        };


        static TestCaseData[] RayVsRayToleranceTestSource =
        {
            new TestCaseData(new float2(0, 1), new float2(2, 0), new float2(1, 0), new float2(0, 1), 0.5f),
            new TestCaseData(new float2(0, 1), new float2(2, -2), new float2(1, 0), new float2(0, 1), 0.5f),
            new TestCaseData(new float2(0, 1), new float2(2, -2), new float2(1, 0), new float2(0, 1), 0.5f),
        };


        static TestCaseData[] RayVsAABB =
        {
            new TestCaseData(new float2(0, 0), new float2(1, 0), new float2(1, 0), new float2(0.5f, 0), 0.75f, new float2(-1, 0)).Returns(true),
            new TestCaseData(new float2(0, 0), new float2(1f, 0), new float2(1.5f, 0), new float2(0.4f, 0), null, null).Returns(false),
            new TestCaseData(new float2(0, -1), new float2(1f, 1f), new float2(2, -1), new float2(2f, 2.0f), 1, new float2(0, 1)).Returns(true),
        };
    }
}