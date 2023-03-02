using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class MarchingCubesGPU : MonoBehaviour
{

    public int VoxelGridSizeX;
    public int VoxelGridSizeY;
    public int VoxelGridSizeZ;
    public float VoxelScale;

    public ComputeShader shader;
    public Texture noiseTexture;

    float[] VertexOut;
    float[] NormalOut;

    // Stores the vertices computed by the marching cubes algorithm
    List<Vector3> computedVertices = new List<Vector3>();

    // Stores the normals, same order as computedVertices
    List<Vector3> computedNormals = new List<Vector3>();

    void Start()
    {
        Profiler.BeginSample("GPUCubes");
        MarchingCubes();
        CurateLists();
        Profiler.EndSample();
        CreateMesh();
    }

    // Set up and run the marching cubes kernel
    void MarchingCubes()
    {

        VertexOut = new float[VoxelGridSizeX * VoxelGridSizeY * VoxelGridSizeZ * 16];
        NormalOut = new float[VoxelGridSizeX * VoxelGridSizeY * VoxelGridSizeZ * 16];

        // Set parameters
        shader.SetFloat("VoxelScale", VoxelScale);
        shader.SetInt("sizeX", VoxelGridSizeX);
        shader.SetInt("sizeY", VoxelGridSizeY);
        shader.SetInt("sizeZ", VoxelGridSizeZ);

        // Set up data buffers
        ComputeBuffer vertexBuffer = new ComputeBuffer(VoxelGridSizeX * VoxelGridSizeY * VoxelGridSizeZ * 16, sizeof(float) * 4);
        ComputeBuffer normalBuffer = new ComputeBuffer(VoxelGridSizeX * VoxelGridSizeY * VoxelGridSizeZ * 16, sizeof(float) * 4);
        shader.SetBuffer(shader.FindKernel("MarchingCubes"), "vertexOut", vertexBuffer);
        shader.SetBuffer(shader.FindKernel("MarchingCubes"), "normalOut", normalBuffer);

        // Get number of thread groups to dispatch
        shader.GetKernelThreadGroupSizes(shader.FindKernel("MarchingCubes"), out uint ThreadGroupSizeX, out uint ThreadGroupSizeY, out uint ThreadGroupSizeZ);

        int ThreadGroupsX = (int)(VoxelGridSizeX / ThreadGroupSizeX);
        int ThreadGroupsY = (int)(VoxelGridSizeY / ThreadGroupSizeY);
        int ThreadGroupsZ = (int)(VoxelGridSizeZ / ThreadGroupSizeZ);

        // Set the noise texture
        shader.SetTexture(shader.FindKernel("MarchingCubes"), "noiseTex", noiseTexture);

        // Dispatch the shader
        shader.Dispatch(shader.FindKernel("MarchingCubes"), ThreadGroupsX, ThreadGroupsY, ThreadGroupsZ);

        vertexBuffer.GetData(VertexOut);
        vertexBuffer.Release();
        normalBuffer.GetData(NormalOut);
        normalBuffer.Release();

    }

    // Convert the outputs from the marching cubes kernel into usable vertex and normal lists
    void CurateLists()
    {

        for (int i = 0; i < VertexOut.Length; i+=4)
        {

            // In the compute shader we mark all existing vertices/normals with a w value of 1
            if (VertexOut[i + 3] == 1.0f)
            {
                computedVertices.Add(new Vector3(VertexOut[i], VertexOut[i + 1], VertexOut[i + 2]));
                computedNormals.Add(new Vector3(NormalOut[i], NormalOut[i + 1], NormalOut[i + 2]));
            }

        }

        VertexOut = null;
        NormalOut = null;

    }

    // Send vertex and normal data to attached mesh component
    void CreateMesh()
    {

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = computedVertices.ToArray();
        mesh.normals = computedNormals.ToArray();

        int[] triangles = new int[computedVertices.Count];
        for (int i = 0; i < computedVertices.Count; i++)
        {
            triangles[i] = i;
        }
        mesh.triangles = triangles;

    }

}
