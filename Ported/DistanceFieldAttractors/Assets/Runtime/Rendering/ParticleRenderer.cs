using UnityEngine;

public class ParticleRenderer
{
    private ParticleDrawCall _drawcall;

    private Material _material;
    private Mesh _mesh;

    public void Initialize(ComputeShader computeShader, Material material, Mesh mesh, ComputeParticleManager.SimulationParams simulationParams)
    {
        _material = material;
        _mesh = mesh;

        if (_drawcall == null && _mesh != null && _material != null)
        {
            _drawcall = DrawCallGenerator.GetComputeDrawcall(computeShader, _mesh, _material, 32768, simulationParams);
        }

        if (!_drawcall.IsInitialized)
        {
            _drawcall.Initialize();
        }
    }

    public void FixedUpdate(DistanceFieldModel simulation)
    {
        if(_drawcall != null)
        {
            _drawcall.FixedUpdate(simulation);
        }
    }

    public void Draw()
    {
        if (_drawcall != null)
        {
            _drawcall.Update();
            Graphics.DrawMeshInstancedIndirect(_drawcall.Mesh, _drawcall.SubMeshIndex, _drawcall.Material, _drawcall.Bounds, _drawcall.ArgsBuffer);
        }
    }

    public void Release()
    {
        if (_drawcall != null)
        {
            _drawcall.Release();
            _drawcall = null;
        }
    }
}
