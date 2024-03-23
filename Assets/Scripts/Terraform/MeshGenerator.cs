using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplicationFactor, AnimationCurve heightDampening) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        // Lo centramos
        float topLeftX = (height - 1) / (-2f);
        float topLeftZ = (width - 1) / (2f);

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;
        for (int x = 0; x < height; x++) {
            for (int y = 0; y < width; y++) {
                float correctedX = x + topLeftX;
                float correctedY = y - topLeftZ;
                float correctedZ = heightDampening.Evaluate(heightMap[x, y]) * heightMultiplicationFactor;
                meshData.vertices[vertexIndex] = new Vector3(correctedX, correctedZ, correctedY);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                if (x < width - 1 && y < height - 1) {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    int triangleIdx;

    public MeshData(int meshWidth, int meshHeight) {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c) {
        triangles[triangleIdx] = a;
        triangles[triangleIdx + 1] = b;
        triangles[triangleIdx + 2] = c;
        triangleIdx += 3;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}