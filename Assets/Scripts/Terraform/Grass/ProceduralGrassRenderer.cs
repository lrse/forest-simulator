using UnityEngine;
using Unity.Mathematics;

struct SourceVertex {
    public Vector3 positionWS;
    public Vector3 normalWS;
}

struct DrawTriangle {
    public float3 lightingNormalWS;
    public float4 a;
    public float4 b;
    public float4 c;
};

public class ProceduralGrassRenderer : MonoBehaviour {
    [SerializeField] public WindowSamplerParallel windowSamplerParallel;
    [SerializeField] public GrassData grassData = default;
    [SerializeField] public bool drawEnabled = false;
    [SerializeField] public bool autoUpdate = false;

    [HideInInspector] public bool grassDataFoldout = false;
    [HideInInspector] public bool samplerParallelDataFoldout = false;

    private GameObject terrain;
    private bool initialized;

    private ComputeShader grassComputeShader;
    private Material grassMaterial;
    private Vector2[] spawnPoints;
    private SourceVertex[] spawnPointsWS;

    private ComputeBuffer sourceVertexBuffer;
    private ComputeBuffer drawBuffer;
    private ComputeBuffer argsBuffer;

    // We have to instantiate the shaders so each points to their unique compute buffers
    private ComputeShader instantiatedGrassComputeShader;
    public Material instantiatedMaterial;

    private int idGrassKernel;
    private int dispatchSize;

    // The size of one entry in the various compute buffers
    private const int SOURCE_VERT_STRIDE = sizeof(float) * 6;
    private const int DRAW_STRIDE = sizeof(float) * (3 + (3 + 1) * 3);
    private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;

    // The data to reset the args buffer with every frame
    private int[] argsBufferReset = new int[] { 0, 1, 0, 0 };

    public void GeneratePoints() {
        Texture2D densityTex = grassData.densityPipeline.GetFinalTexture();
        spawnPoints = windowSamplerParallel.GeneratePoints(densityTex);
    }

    public void TransformPointsToWS() {
        Bounds bounds = terrain.GetComponent<MeshFilter>().sharedMesh.bounds;
        RaycastHit[] hits = ParallelRaycast.PointsToSurface(spawnPoints, Constants.TerrainLayer, bounds);

        spawnPointsWS = new SourceVertex[hits.Length];
        for (int i = 0; i < hits.Length; i++) {
            if (hits[i].collider == null) {
                continue;
            }
            spawnPointsWS[i] = new SourceVertex() {
                positionWS = hits[i].point,
                normalWS = hits[i].normal
            };
        }
    }

    private void CreateBuffers() {
        // Each grass blade segment has two points. Counting those plus the tip gives us the total number of points
        int maxBladeSegments = Mathf.Max(1, grassData.maxSegments);
        int maxBladeTriangles = (maxBladeSegments - 1) * 2 + 1;

        sourceVertexBuffer = new ComputeBuffer(spawnPointsWS.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        sourceVertexBuffer.SetData(spawnPointsWS);
        drawBuffer = new ComputeBuffer(spawnPointsWS.Length * maxBladeTriangles, DRAW_STRIDE, ComputeBufferType.Append);
        drawBuffer.SetCounterValue(0);
        argsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);
    }

    private void SetBuffers() {
        instantiatedGrassComputeShader.SetBuffer(idGrassKernel, "_SourceVertices", sourceVertexBuffer);
        instantiatedGrassComputeShader.SetBuffer(idGrassKernel, "_DrawTriangles", drawBuffer);
        instantiatedGrassComputeShader.SetBuffer(idGrassKernel, "_IndirectArgsBuffer", argsBuffer);
        instantiatedMaterial.SetBuffer("_DrawTriangles", drawBuffer);
    }

    private void SetData() {
        // Update the shader with frame specific data
        instantiatedGrassComputeShader.SetInt("_NumSourceVertices", spawnPointsWS.Length);
        instantiatedGrassComputeShader.SetInt("_MaxBladeSegments", spawnPointsWS.Length);
        instantiatedGrassComputeShader.SetFloat("_JitterFactor", grassData.jitterFactor);
        instantiatedGrassComputeShader.SetFloat("_MaxBendAngle", grassData.maxBendAngle);
        instantiatedGrassComputeShader.SetFloat("_BladeCurvature", grassData.bladeCurvature);
        instantiatedGrassComputeShader.SetFloat("_BladeHeight", grassData.bladeHeight);
        instantiatedGrassComputeShader.SetFloat("_BladeHeightVariance", grassData.heightVariance);
        instantiatedGrassComputeShader.SetFloat("_BladeWidth", grassData.bladeWidth);
        instantiatedGrassComputeShader.SetFloat("_BladeWidthVariance", grassData.widthVariance);
        instantiatedGrassComputeShader.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
    }

    private void ResetBuffers() {
        // Clear the draw and indirect args buffers of last frame's data
        drawBuffer.SetCounterValue(0);
        argsBuffer.SetData(argsBufferReset);
    }

    public void Dispatch() {
        ResetBuffers();
        SetData();

        instantiatedGrassComputeShader.Dispatch(idGrassKernel, dispatchSize, 1, 1);
    }

    public Vector3[] GetGrassVertices() {
        Initialize();
        Dispatch();

        uint[] argsData = new uint[4];
        argsBuffer.GetData(argsData);
        // TODO: debug that magic number 9, there're 540 bytes in the buffer
        // unaccounted for. That means there are 9 triangles in the ether.
        DrawTriangle[] sourceVertices = new DrawTriangle[argsData[0] / 3 - 9];  // It's using the order of the struct
        drawBuffer.GetData(sourceVertices);

        Vector3[] positions = new Vector3[argsData[0]];
        for (int i = 0; i < sourceVertices.Length; i++) {
            positions[3 * i] = sourceVertices[i].a.xyz;
            positions[3 * i + 1] = sourceVertices[i].b.xyz;
            positions[3 * i + 2] = sourceVertices[i].c.xyz;
        }
        OnDisable();
        return positions;
    }

    public void Initialize() {
        if (terrain == null) {
            terrain = GameObject.Find(Constants.TerrainName);
        }
        if (grassComputeShader == null) {
            grassComputeShader = Resources.Load<ComputeShader>(Constants.GrassComputeShader);
        }
        if (grassMaterial == null) {
            grassMaterial = Resources.Load<Material>(Constants.GrassMaterial);
        }
        Debug.Assert(terrain != null, "Terrain is not defined", gameObject);
        Debug.Assert(grassComputeShader != null, "The grass compute shader is null", gameObject);
        Debug.Assert(grassMaterial != null, "The material is null", gameObject);

        // If initialized, call on disable to clean things up
        if (initialized) {
            OnDisable();
        }
        initialized = true;

        instantiatedGrassComputeShader = Instantiate(grassComputeShader);
        instantiatedMaterial = Instantiate(grassMaterial);

        SetPointsBuffersAndKernel();
    }

    private void OnDisable() {
        sourceVertexBuffer?.Release();
        drawBuffer?.Release();
        argsBuffer?.Release();
        initialized = false;
    }

    private void LateUpdate() {
        if (!initialized) {
            OnDisable();
            Initialize();
        }
        if (autoUpdate) Dispatch();
        if (drawEnabled) {
            Graphics.DrawProceduralIndirect(
                material: instantiatedMaterial,
                bounds: terrain.GetComponent<MeshFilter>().sharedMesh.bounds,
                topology: MeshTopology.Triangles,
                bufferWithArgs: argsBuffer,
                castShadows: UnityEngine.Rendering.ShadowCastingMode.Off,
                layer: gameObject.layer
            );
        }
    }

    public void SetPointsBuffersAndKernel() {
        GeneratePoints();
        TransformPointsToWS();
        CreateBuffers();
        SetBuffers();

        // Cache the kernel IDs we will be dispatching
        idGrassKernel = instantiatedGrassComputeShader.FindKernel("CSMain");
        instantiatedGrassComputeShader.GetKernelThreadGroupSizes(idGrassKernel, out uint threadGroupSize, out _, out _);
        dispatchSize = Mathf.CeilToInt((float)spawnPointsWS.Length / threadGroupSize);

    }
}