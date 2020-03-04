using UnityEngine;

public class ParticleDrawCall
{
    public int SubMeshIndex { get; set; }
    public Mesh Mesh { get; set; }
    public Material Material { get; set; }
    public Bounds Bounds { get; set; }
    public ComputeBuffer ArgsBuffer { get; set; }
    public ComputeBuffer PositionBuffer { get; set; }

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
