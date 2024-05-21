Shader "Custom/TreeUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {  "Queue" = "AlphaTest"
                "IgnoreProjector" = "True"
                "RenderType"="TransparentCutout" }
        LOD 100
        Cull[_TwoSided]
        AlphaToMask On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            //#pragma  alphatest:_Cutoff vertex:vert

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                if (col.a < 0.333)
                    discard;
                col.r = _Color.r;
                col.g = _Color.g;
                col.b = _Color.b;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                //#endif  
                return col;
            }
            ENDCG
        }
    }
}
