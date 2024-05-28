using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Terraform/Data/Window Sampler Parallel")]
public class WindowSamplerParallel : ScriptableObject {
    [Header("Properties (in powers of 2)")]
    [SerializeField, Range(1, 16)] int windowExp;
    [SerializeField, Range(0, 11)] int maxBladesPerTileExp;

    private int kernelId;
    private ComputeShader computeShader;
    private ComputeBuffer spawnPointsBuffer;
    private ComputeBuffer generatedPointsCountBuffer;

    private void SetShaderProperties(int window, int maxBladesPerTile, ComputeBuffer spawnPointsBuffer, ComputeBuffer generatedPointsCountBuffer, Texture2D densityTex) {
        computeShader.SetInt("_Window", window);
        computeShader.SetInt("_MaxBladesPerTile", maxBladesPerTile);
        computeShader.SetBuffer(kernelId, "_SpawnPointsBuffer", spawnPointsBuffer);
        computeShader.SetBuffer(kernelId, "_GeneratedPointsCountBuffer", generatedPointsCountBuffer);
        computeShader.SetTexture(kernelId, "_DensityTex", densityTex);

        spawnPointsBuffer.SetCounterValue(0);
        generatedPointsCountBuffer.SetCounterValue(0);
        generatedPointsCountBuffer.SetData(new uint[] { 0 });
    }

    private int CalculateTotalTiles(int window, Texture2D densityTex) {
        return Mathf.Max(
            1, Mathf.CeilToInt(densityTex.height / window) * Mathf.CeilToInt(densityTex.width / window)
        );
    }

    public Vector2[] GeneratePoints(Texture2D densityTex) {
        int window = (int)Mathf.Pow(2, windowExp);
        int maxBladesPerTile = (int)Mathf.Pow(2, maxBladesPerTileExp);

        computeShader = Resources.Load<ComputeShader>(Constants.WindowSamplerComputeShader);

        kernelId = computeShader.FindKernel("CSMain");
        spawnPointsBuffer = new ComputeBuffer(
            maxBladesPerTile * CalculateTotalTiles(window, densityTex),
            sizeof(float) * 2,
            ComputeBufferType.Append
        );
        generatedPointsCountBuffer = new ComputeBuffer(1, sizeof(int));

        SetShaderProperties(window, maxBladesPerTile, spawnPointsBuffer, generatedPointsCountBuffer, densityTex);

        computeShader.GetKernelThreadGroupSizes(
            kernelId, out uint threadGroupX, out uint threadGroupY, out _
        );
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
