using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class ParticlesGenerator : MonoBehaviour
{
    [SerializeField] int m_ParticleCount;
    MeshFilter m_MeshFilter;

    void Awake()
    {
        m_MeshFilter = GetComponent<MeshFilter>();
        var mesh = m_MeshFilter.mesh;
    }

    void Start()
    {
        m_ParticleCount = Mathf.Max(m_ParticleCount, 1);

        var mesh = m_MeshFilter.mesh;
        var originalVertices = mesh.vertices;
        var originalNormals = mesh.normals;
        var originalTriangles = mesh.triangles;
        mesh.Clear();

        var originalVertexCount = originalVertices.Length;
        var newVertexCount = m_ParticleCount * originalVertexCount;
        var originalTrianglesCount = originalTriangles.Length;
        var newTrianglesCount = m_ParticleCount * originalTrianglesCount;

        var newVertices = new Vector3[newVertexCount];
        var newNormals = new Vector3[newVertexCount];
        var newColors = new Color[newVertexCount];
        var newTriangles = new int[newTrianglesCount];

        for (var i = 0; i < m_ParticleCount; i++)
        {
            var pos = Random.insideUnitSphere;

            for (var j = 0; j < originalVertexCount; j++)
            {
                var id = i * originalVertexCount + j;
                newVertices[id] = originalVertices[j];
                newNormals[id] = originalNormals[j];
                newColors[id] = new Color(pos.x, pos.y, pos.z, 0);
            }

            for (var k = 0; k < originalTrianglesCount; k++)
            {
                var id = i * originalTrianglesCount + k;
                newTriangles[id] = originalTriangles[k] + i * originalVertexCount;
            }
        }

        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.colors = newColors;
        mesh.triangles = newTriangles;
    }
}
