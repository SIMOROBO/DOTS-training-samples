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
        var newTriangles = new int[newTrianglesCount];

        for (var i = 0; i < m_ParticleCount; i++)
        {
            for (var j = 0; j < originalVertexCount; j++)
            {
                var id = i * originalVertexCount + j;
                newVertices[id] = originalVertices[j];
                newNormals[id] = originalNormals[j];
                //Debug.Log("Vertex "+ id +" "+ newVertices[id]);
            }

            for (var k = 0; k < originalTrianglesCount; k++)
            {
                var id = i * originalTrianglesCount + k;
                newTriangles[id] = originalTriangles[k] + i * originalVertexCount;
                //Debug.Log("Index " + id + " " + newTriangles[id]);
            }
        }

        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.triangles = newTriangles;

        /*
        var layout = new[]
                {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 2),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 3),
        };
        var vertexCount = 24 * m_ParticleCount;
        mesh.SetVertexBufferParams(vertexCount, layout);

        // set vertex data
        var verts = new NativeArray<ExampleVertex>(vertexCount, Allocator.Temp);

        // ... fill in vertex array data here...

        mesh.SetVertexBufferData(verts, 0, 0, vertexCount);
        */
    }
}
