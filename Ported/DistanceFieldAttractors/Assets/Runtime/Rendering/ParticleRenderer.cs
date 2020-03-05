using System;
using Unity.Collections;
using UnityEngine;


[ExecuteAlways]
public class ParticleRenderer : MonoBehaviour
{
    [System.Serializable]
    public struct SimulationParams
    {
        public float attraction;
        public float jitter;
        public Color surfaceColor;
        public Color interiorColor;
        public Color exteriorColor;
        public float interiorColorDist;
        public float exteriorColorDist;
        public float colorStiffness;
        public float speedStretch;
    }

    public Mesh _mesh;
    public Material _material;
    private ParticleDrawCall _drawcall;
    public ComputeShader _computeShader;
    public SimulationParams simulationParams;

    public void Restart()
    {
        if (_drawcall == null && _mesh != null && _material != null)
        {
            //_drawcall = DrawCallGenerator.GetComputeDrawcall(_computeShader, _mesh, _material, 4096);
            _drawcall = DrawCallGenerator.GetComputeDrawcall(_computeShader, _mesh, _material, 32768, simulationParams);
        }
        if(!_drawcall.IsInitialized)
        {
            _drawcall.Initialize();
        }
    }

    private void Update()
    {
        if (_drawcall != null)
        {
            _drawcall.Update();
            Graphics.DrawMeshInstancedIndirect(_drawcall.Mesh, _drawcall.SubMeshIndex, _drawcall.Material, _drawcall.Bounds, _drawcall.ArgsBuffer);
        }
    }

    private void FixedUpdate()
    {
        if (_drawcall != null)
        {
            _drawcall.FixedUpdate();
        }
    }

    private void DrawParticles()
    {
        if (_drawcall == null && _mesh != null && _material != null)
        {
            NativeArray<Matrix4x4> transforms = GetTransforms(10000);
            _drawcall = DrawCallGenerator.GetDrawCall(_mesh, _material, transforms);
        }

        if (_drawcall != null)
        {
            Graphics.DrawMeshInstancedIndirect(_drawcall.Mesh, _drawcall.SubMeshIndex, _drawcall.Material, _drawcall.Bounds, _drawcall.ArgsBuffer);
        }
    }

    private NativeArray<Matrix4x4> GetTransforms(int instanceCount)
    {
        int instancesPerRow = System.Convert.ToInt32(System.Math.Pow(instanceCount, (1.0 / 3.0)));
        instanceCount = instancesPerRow * instancesPerRow * instancesPerRow;
        Matrix4x4[] transformsArray = new Matrix4x4[instanceCount];
        int spacing = 3;
        for (int x = 0; x < instancesPerRow; x++)
        {
            for (int y = 0; y < instancesPerRow; y++)
            {
                for (int z = 0; z < instancesPerRow; z++)
                {
                    int i = (x * (instancesPerRow * instancesPerRow)) + y * instancesPerRow + z;
                    transformsArray[i] = Matrix4x4.Translate(new Vector3(x, y, z) * spacing);
                }
            }
        }

        var transforms = new NativeArray<Matrix4x4>(transformsArray, Allocator.Temp);
        return transforms;
    }

    void OnDisable()
    {
        if(_drawcall != null)
        {
            _drawcall.Release();
            _drawcall = null;
        }
    }
}

internal class serializeableAttribute : Attribute
{
}