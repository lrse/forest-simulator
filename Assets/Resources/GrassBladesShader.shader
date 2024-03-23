Shader "Grass/GrassBlades" {
    Properties {
        _BaseColor("Base color", Color) = (0, 0.5, 0, 1)
        _TipColor("Tip color", Color) = (0, 1, 0, 1)
        [Enum(No,2,Yes,0)] _TwoSided ("Two Sided", Int) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0
        _Metallic("Metallic", Range(0,1)) = 0
    }
    SubShader {
        // UniversalPipeline needed to have this render in URP
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}

        // Forward Lit pass
        Pass {

            Name "ForwardLit"
            Tags{ "LightMode" = "UniversalForward" }
            Cull [_TwoSided]

            HLSLPROGRAM
            // Register our functions
            #pragma vertex Vertex
            #pragma fragment Fragment

            // Props
            float4 _BaseColor;
            float4 _TipColor;
            float _Smoothness;
            float _Metallic;

            // Include our logic file
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // This describes a vertex on the generated mesh
            struct DrawVertex {
                float3 positionWS; // The position in world space
                float height; // The height of this vertex on the grass blade
            };

            // A triangle on the generated mesh
            struct DrawTriangle {
                float3 lightingNormalWS; // A normal, in world space, to use in the lighting alghorithm
                DrawVertex vertices[3];
            };

            // A buffer containing the generated mesh
            StructuredBuffer<DrawTriangle> _DrawTriangles;

            struct Interpolators {
                float uv : TEXCOORD0;  // The height of this vertex on the grass blade
                float3 positionWS : TEXCOORD1;  // Position in WS
                float3 normalWS : TEXCOORD2;  // Normal vector in WS
                float4 positionCS : SV_POSITION;  // Position in clip space
            };

            Interpolators Vertex(uint vertexID : SV_VertexID) {
                // Initialize the output struct
                Interpolators o = (Interpolators) 0;
    
                // Get the vertex from the buffer
                // Since the buffer is structured in triangles, we need to divide the vertexID by three
                // to get the triangle, and then modulo by 3 to get the vertex on the triangle
                DrawTriangle tri = _DrawTriangles[vertexID / 3];
                DrawVertex input = tri.vertices[vertexID % 3];
    
                o.positionWS = input.positionWS;
                o.normalWS = tri.lightingNormalWS;
                o.uv = input.height;
                o.positionCS = TransformWorldToHClip(input.positionWS);
                return o;
            }

            half4 Fragment(Interpolators input) : SV_Target {
                InputData inputData = (InputData) 0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = input.normalWS;
                inputData.viewDirectionWS = normalize(GetCameraPositionWS() - input.positionWS);
            #if SHADOWS_SCREEN
                inputData.shadowCoord = ComputeScreenPos(input.positionCS);
            #else
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
            #endif
      
                SurfaceData surfaceData = (SurfaceData) 0;
                surfaceData.albedo = lerp(_BaseColor.rgb, _TipColor.rgb, input.uv);
                surfaceData.alpha = 1;
                surfaceData.specular = 0;
                surfaceData.smoothness = _Smoothness;
                surfaceData.occlusion = 0;
                surfaceData.metallic = _Metallic;
    
                return UniversalFragmentPBR(inputData, surfaceData);

            }

            ENDHLSL
        }
    }
}
