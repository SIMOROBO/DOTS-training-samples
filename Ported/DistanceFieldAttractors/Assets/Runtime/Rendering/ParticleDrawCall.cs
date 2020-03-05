using UnityEngine;

public class ParticleDrawCall
{
    public int SubMeshIndex { get; set; }
    public Mesh Mesh { get; set; }
    public Material Material { get; set; }
    public Bounds Bounds { get; set; }
    public ComputeBuffer ArgsBuffer { get; set; }
    public ComputeBuffer PositionBuffer { get; set; }

    public ComputeShader ComputeShader { get; set; }    
    public uint Count { get; set; }

    public void Update()
    {
        if(ComputeShader != null)
        {
            if(PositionBuffer == null)
            {
                PositionBuffer = new ComputeBuffer((int)Count, 16 * 4);
            }

            int kernelIndex = ComputeShader.FindKernel("CSMain");
            int instancesPerRow = System.Convert.ToInt32(System.Math.Pow(Count, (1.0 / 3.0)));

            ComputeShader.SetBuffer(kernelIndex, "TransformsBuffer", PositionBuffer);
            ComputeShader.SetInt("gInstancesCount", (int)Count);
            ComputeShader.SetInt("gInstancesPerRow", (int)instancesPerRow);
            float cosTime = Mathf.Cos(Time.realtimeSinceStartup * 0.3f) + 1;
            ComputeShader.SetFloat("gCosTime", cosTime);
            ComputeShader.Dispatch(kernelIndex, instancesPerRow/8, instancesPerRow / 8, instancesPerRow / 8);
            Material.SetBuffer("transformBuffer", PositionBuffer);
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
