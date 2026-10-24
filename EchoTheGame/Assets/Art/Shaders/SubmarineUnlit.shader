Shader "Echo/Submarine Unlit"
{
    Properties
    {
        [MainColor] _BaseColor ("Player color", Color) = (0.60784316,0.9137255,0.8862745,1)
        _HullColor ("Dark hull", Color) = (0.007843138,0.047058824,0.07450981,1)
        _Fill ("Hull visibility", Range(0,0.5)) = 0.16
        _ContourStrength ("Luminous contour strength", Range(0,1)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor, _HullColor;
            float _Fill, _ContourStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 color : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 world : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.world = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.world);
                o.normal = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float3 view = GetWorldSpaceNormalizeViewDir(i.world);
                float rim = pow(1 - saturate(dot(normal,view)),2.1);
                // A quiet solid hull with a soft contour keeps the silhouette readable.
                float tone = saturate(_Fill + rim*_ContourStrength);
                return half4(_HullColor.rgb + _BaseColor.rgb*tone,1);
            }
            ENDHLSL
        }
    }
}
