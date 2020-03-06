using System;
using UnityEngine;


public class ComputeParticleManager : MonoBehaviour
{
    [System.Serializable]
    public struct SimulationParams
    {
        public uint numberOfParticles;
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


    [SerializeField]
    public int simulationDuration = 10;

    public Mesh _mesh;
    public Material _material;
    public ComputeShader _computeShader;
    public SimulationParams simulationParams;

    private float _simulationStartTime;
    private DistanceFieldModel m_currentSimulation;
    private ParticleRenderer m_particleRenderer;

    public void Start()
    {
        m_particleRenderer = new ParticleRenderer();
        m_particleRenderer.Initialize(_computeShader, _material, _mesh, simulationParams);
    }

    private void Update()
    {
        m_particleRenderer.Draw();
    }

    private void FixedUpdate()
    {
        if (Time.realtimeSinceStartup - _simulationStartTime > simulationDuration)
        {
            int simulationIndex = ((int)m_currentSimulation + 1) % Enum.GetValues(typeof(DistanceFieldModel)).Length;
            m_currentSimulation = (DistanceFieldModel)simulationIndex;
            _simulationStartTime = Time.realtimeSinceStartup;
        }

        m_particleRenderer.FixedUpdate(m_currentSimulation);
    }

    private void OnDisable()
    {
        m_particleRenderer.Release();
    }
}

internal class serializeableAttribute : Attribute
{
}
