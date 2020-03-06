using UnityEngine;

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
    public ComputeParticleManager.SimulationParams SimulationParams { get; set; }
    public bool IsInitialized { get; set; }
    int k_kernelSize = 4;    

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
                int chunksPerRow = Mathf.CeilToInt(instancesPerRow / k_kernelSize);

                ComputeShader.SetBuffer(kernelIndex, "FixedUpdateBuffer", FixedUpdateBuffer);
                ComputeShader.SetBuffer(kernelIndex, "TransformsBuffer", PositionBuffer);
                ComputeShader.SetInt("gChunksPerRow", chunksPerRow);
                ComputeShader.SetInt("gInstancesCount", (int)Count);
                ComputeShader.SetInt("gInstancesPerRow", (int)instancesPerRow);
                ComputeShader.SetFloat("gTime", Time.realtimeSinceStartup * 0.3f);
                ComputeShader.Dispatch(kernelIndex, chunksPerRow, chunksPerRow, chunksPerRow);
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
                int chunksPerRow = Mathf.CeilToInt(instancesPerRow / k_kernelSize);

                ComputeShader.SetBuffer(kernelIndex, "FixedUpdateBuffer", FixedUpdateBuffer);
                ComputeShader.SetBuffer(kernelIndex, "TransformsBuffer", PositionBuffer);
                ComputeShader.SetInt("gChunksPerRow", chunksPerRow);
                ComputeShader.SetInt("gInstancesCount", (int)Count);
                ComputeShader.SetInt("gInstancesPerRow", (int)instancesPerRow);
                ComputeShader.SetFloat("gTime", Time.realtimeSinceStartup * 0.3f);
                ComputeShader.Dispatch(kernelIndex, chunksPerRow, chunksPerRow, chunksPerRow);
                Material.SetBuffer("transformBuffer", PositionBuffer);
            }
        }
    }

    public void FixedUpdate(DistanceFieldModel mode)
    {
        if (IsInitialized && ComputeShader != null)
        {
            int modeIndex = (int)mode;

            // Until we support all 6 simulation models.
            modeIndex = modeIndex % 6;
            //modeIndex = (int)DistanceFieldModels.PerlinNoise;

            int kernelIndex = ComputeShader.FindKernel($"CSFixedUpdate{modeIndex}");
            if (kernelIndex != -1)
            {
                int instancesPerRow = System.Convert.ToInt32(System.Math.Pow(Count, (1.0 / 3.0)));
                int chunksPerRow = Mathf.CeilToInt(instancesPerRow / k_kernelSize);

                ComputeShader.SetBuffer(kernelIndex, "FixedUpdateBuffer", FixedUpdateBuffer);
                ComputeShader.SetInt("gChunksPerRow", chunksPerRow);
                ComputeShader.SetInt("gInstancesCount", (int)Count);
                ComputeShader.SetInt("gInstancesPerRow", (int)instancesPerRow);
                ComputeShader.SetFloat("gTime", Time.realtimeSinceStartup * 0.3f);
                ComputeShader.SetFloat("gAttraction", SimulationParams.attraction);
                ComputeShader.SetFloat("gJitter", SimulationParams.jitter);
                ComputeShader.SetFloat("gInteriorColorDist", SimulationParams.interiorColorDist);
                ComputeShader.SetFloat("gExteriorColorDist", SimulationParams.exteriorColorDist);
                ComputeShader.SetFloat("gColorStiffness", SimulationParams.colorStiffness);
                ComputeShader.SetFloat("gSpeedStretch", SimulationParams.speedStretch);
                ComputeShader.SetVector("gSurfaceColor", new Vector4(SimulationParams.surfaceColor.r, SimulationParams.surfaceColor.g, SimulationParams.surfaceColor.b, 1.0f));
                ComputeShader.SetVector("gInteriorColor", new Vector4(SimulationParams.interiorColor.r, SimulationParams.interiorColor.g, SimulationParams.interiorColor.b, 1.0f));
                ComputeShader.SetVector("gExteriorColor", new Vector4(SimulationParams.exteriorColor.r, SimulationParams.exteriorColor.g, SimulationParams.exteriorColor.b, 1.0f));
                ComputeShader.Dispatch(kernelIndex, chunksPerRow, chunksPerRow, chunksPerRow);
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

        if(FixedUpdateBuffer != null)
            FixedUpdateBuffer.Release();
        FixedUpdateBuffer = null;
    }
}
