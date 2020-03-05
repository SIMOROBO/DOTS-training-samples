using Unity.Collections;
using UnityEngine;
 
public static class DrawCallGenerator
{
    public static ParticleDrawCall GetComputeDrawcall(ComputeShader shader, Mesh instanceMesh, Material material, uint count)
    {
        if(shader == null)
        {
            return null;
        }

        // Indirect args
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)instanceMesh.GetIndexCount(0);
        args[1] = (uint)count;
        args[2] = (uint)instanceMesh.GetIndexStart(0);
        args[3] = (uint)instanceMesh.GetBaseVertex(0);

        ComputeBuffer argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        
        ParticleDrawCall drawcall = new ParticleDrawCall();
        drawcall.ComputeShader = shader;
        drawcall.Count = count;
        drawcall.Mesh = instanceMesh;
        drawcall.SubMeshIndex = 0;
        drawcall.Material = material;
        drawcall.Bounds = new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f));
        drawcall.ArgsBuffer = argsBuffer;
        return drawcall;

    }

    public static ParticleDrawCall GetDrawCall(Mesh instanceMesh, Material material, NativeArray<Matrix4x4> transforms)
    {
        ComputeBuffer argsBuffer;
        ComputeBuffer positionBuffer;
        GetBuffers(instanceMesh, transforms, out argsBuffer, out positionBuffer);
        material.SetBuffer("transformBuffer", positionBuffer);
        ParticleDrawCall drawcall = new ParticleDrawCall();
        drawcall.Mesh = instanceMesh;
        drawcall.SubMeshIndex = 0;
        drawcall.Material = material;
        drawcall.Bounds = new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f));
        drawcall.PositionBuffer = positionBuffer;
        drawcall.ArgsBuffer = argsBuffer;        
        return drawcall;
    }

    private static void GetBuffers(Mesh instanceMesh, NativeArray<Matrix4x4> transforms, out ComputeBuffer argsBuffer, out ComputeBuffer positionBuffer)
    {
        positionBuffer = new ComputeBuffer(transforms.Length, 16 * 4);

        positionBuffer.SetData(transforms);

        // Indirect args
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)instanceMesh.GetIndexCount(0);
        args[1] = (uint)transforms.Length;
        args[2] = (uint)instanceMesh.GetIndexStart(0);
        args[3] = (uint)instanceMesh.GetBaseVertex(0);

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }
}
