﻿using UnityEngine;

public class ParticleDrawCall
{
    public int SubMeshIndex { get; set; }
    public Mesh Mesh { get; set; }
    public Material Material { get; set; }
    public Bounds Bounds { get; set; }
    public ComputeBuffer ArgsBuffer { get; set; }
    public ComputeBuffer PositionBuffer { get; set; }
    public ComputeBuffer FixedUpdateBuffer { get; set; }
    public ComputeShader ComputeShader { get; set; }    
    public uint Count { get; set; }
    public bool IsInitialized { get; set; }

    public void Initialize()
    {
        if (ComputeShader != null)
        {
            if (PositionBuffer == null)
            {
                PositionBuffer = new ComputeBuffer((int)Count, 16 * 4);
            }

            if (FixedUpdateBuffer == null)
            {
                // FixedUpdateData is made of three float3.
                int stride = 3 * sizeof(float) * 3;

                FixedUpdateBuffer = new ComputeBuffer((int)Count, stride);
            }

            int kernelIndex = ComputeShader.FindKernel("CSInitialize");
            if (kernelIndex != -1)
            {
                int instancesPerRow = System.Convert.ToInt32(System.Math.Pow(Count, (1.0 / 3.0)));
                
                ComputeShader.SetBuffer(kernelIndex, "FixedUpdateBuffer", FixedUpdateBuffer);
                ComputeShader.SetBuffer(kernelIndex, "TransformsBuffer", PositionBuffer);
                ComputeShader.SetInt("gInstancesCount", (int)Count);
                ComputeShader.SetInt("gInstancesPerRow", (int)instancesPerRow);
                ComputeShader.SetFloat("gTime", Time.realtimeSinceStartup * 0.3f);
                ComputeShader.Dispatch(kernelIndex, instancesPerRow / 8, instancesPerRow / 8, instancesPerRow / 8);
                IsInitialized = true;
            }
        }
    }

    public void Update()
    {
        if(IsInitialized && ComputeShader != null)
        {
            if(PositionBuffer == null)
            {
                PositionBuffer = new ComputeBuffer((int)Count, 16 * 4);
            }

            int kernelIndex = ComputeShader.FindKernel("CSUpdate");
            if (kernelIndex != -1 && FixedUpdateBuffer != null)
            {
                int instancesPerRow = System.Convert.ToInt32(System.Math.Pow(Count, (1.0 / 3.0)));

                ComputeShader.SetBuffer(kernelIndex, "FixedUpdateBuffer", FixedUpdateBuffer);
                ComputeShader.SetBuffer(kernelIndex, "TransformsBuffer", PositionBuffer);
                ComputeShader.SetInt("gInstancesCount", (int)Count);
                ComputeShader.SetInt("gInstancesPerRow", (int)instancesPerRow);
                ComputeShader.SetFloat("gTime", Time.realtimeSinceStartup * 0.3f);
                ComputeShader.Dispatch(kernelIndex, instancesPerRow / 8, instancesPerRow / 8, instancesPerRow / 8);
                Material.SetBuffer("transformBuffer", PositionBuffer);
            }
        }
    }

    public void FixedUpdate()
    {
        if (IsInitialized && ComputeShader != null)
        {
            int kernelIndex = ComputeShader.FindKernel("CSFixedUpdate");
            if (kernelIndex != -1)
            {
                int instancesPerRow = System.Convert.ToInt32(System.Math.Pow(Count, (1.0 / 3.0)));

                ComputeShader.SetBuffer(kernelIndex, "FixedUpdateBuffer", FixedUpdateBuffer);
                ComputeShader.SetInt("gInstancesCount", (int)Count);
                ComputeShader.SetInt("gInstancesPerRow", (int)instancesPerRow);
                ComputeShader.SetFloat("gTime", Time.realtimeSinceStartup * 0.3f);
                ComputeShader.Dispatch(kernelIndex, instancesPerRow / 8, instancesPerRow / 8, instancesPerRow / 8);
            }
        }
    }

    public void Release()
    {
        if (PositionBuffer != null)
            PositionBuffer.Release();
        PositionBuffer = null;

        if (ArgsBuffer != null)
            ArgsBuffer.Release();
        ArgsBuffer = null;
    }
}
