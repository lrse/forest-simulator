using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Terraform/Data/Window Sampler Parallel")]
public class WindowSamplerParallel : ScriptableObject {
    [SerializeField, Range(2, 16)] int window;
    [SerializeField] int maxBladesPerTile;

    private int kernelId;
    private ComputeShader computeShader;
    private ComputeBuffer spawnPointsBuffer;
    private ComputeBuffer generatedPointsCountBuffer;

    public Vector2[] GeneratePoints(Texture2D densityTex) {
        computeShader = Resources.Load<ComputeShader>(Constants.WindowSamplerComputeShader);

        kernelId = computeShader.FindKernel("CSMain");
        int totalTiles = Mathf.CeilToInt(densityTex.height / window) * Mathf.CeilToInt(densityTex.width/ window);
        spawnPointsBuffer = new ComputeBuffer(maxBladesPerTile * totalTiles, sizeof(float) * 2, ComputeBufferType.Append);
        generatedPointsCountBuffer = new ComputeBuffer(1, sizeof(int));

        computeShader.SetInt("_Window", window);
        computeShader.SetInt("_MaxBladesPerTile", maxBladesPerTile);
        computeShader.SetBuffer(kernelId, "_SpawnPointsBuffer", spawnPointsBuffer);
        computeShader.SetBuffer(kernelId, "_GeneratedPointsCountBuffer", generatedPointsCountBuffer);
        computeShader.SetTexture(kernelId, "_DensityTex", densityTex);

        spawnPointsBuffer.SetCounterValue(0);
        generatedPointsCountBuffer.SetCounterValue(0);
        generatedPointsCountBuffer.SetData(new uint[] { 0 } );

        computeShader.GetKernelThreadGroupSizes(kernelId, out uint threadGroupX, out uint threadGroupY, out _);
        computeShader.Dispatch(
            kernelId,
            Mathf.CeilToInt(densityTex.height / (window * threadGroupX)),
            Mathf.CeilToInt(densityTex.width / (window * threadGroupY)),
            1
        );

        uint[] spawnPointCSCount = new uint[1];  // Element in index 0 holds the count
        generatedPointsCountBuffer.GetData(spawnPointCSCount);
        Vector2[] spawnPoints = new Vector2[spawnPointCSCount[0]];
        spawnPointsBuffer.GetData(spawnPoints);

        spawnPointsBuffer?.Release();
        generatedPointsCountBuffer?.Release();

        return spawnPoints;
    }
}
