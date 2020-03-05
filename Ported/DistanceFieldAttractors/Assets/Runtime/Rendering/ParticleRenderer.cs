using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[ExecuteAlways]
public class ParticleRenderer : MonoBehaviour
{
    public Mesh _mesh;
    public Material _material;
    private ParticleDrawCall _drawcall;
    public ComputeShader _computeShader;

    void Update()
    {
        DrawComputeParticles();
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

    private void DrawComputeParticles()
    {
        if (_drawcall == null && _mesh != null && _material != null)
        {
            //_drawcall = DrawCallGenerator.GetComputeDrawcall(_computeShader, _mesh, _material, 4096);
            _drawcall = DrawCallGenerator.GetComputeDrawcall(_computeShader, _mesh, _material, 32768);

        }

        if (_drawcall != null)
        {
            _drawcall.Update();
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
