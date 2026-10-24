Shader "Echo/Submarine Unlit"
{
    Properties
    {
        [MainColor] _BaseColor ("Player color", Color) = (0.60784316,0.9137255,0.8862745,1)
        _HullColor ("Dark hull", Color) = (0.007843138,0.047058824,0.07450981,1)
        _Fill ("Hull visibility", Range(0,0.5)) = 0.12
        _ContourStrength ("Luminous contour strength", Range(0,1)) = 0.85
        _PointStrength ("Sonar point brightness", Range(0,1)) = 0.55
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
            float _Fill, _ContourStrength, _PointStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 color : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 world : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 local : TEXCOORD2;
                float accent : TEXCOORD3;
            };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.world = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.world);
                o.normal = TransformObjectToWorldNormal(v.normalOS);
                o.local = v.positionOS.xyz;
                o.accent = v.color.r;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float3 view = GetWorldSpaceNormalizeViewDir(i.world);
                float rim = pow(1 - saturate(dot(normal,view)),2.1);
                // Object-space soundings travel with the vessel, without UVs or crawling.
                float2 grid = i.local.xy / 0.065;
                float2 cell = floor(grid);
                float2 random = frac(sin(float2(dot(cell,float2(127.1,311.7)),dot(cell,float2(269.5,183.3))))*43758.5453);
                float dist = length(frac(grid) - (0.25 + random*0.5));
                float aa = max(0.003,length(fwidth(grid))*0.4);
                float dots = (1-smoothstep(0.1,0.1+aa,dist))*step(0.3,random.x);
                // Thin transverse hull seams and a clean oval around the raised sail.
                float station = abs(frac(i.local.y*4+0.5)-0.5);
                float seams = 1-smoothstep(0.012,0.012+max(0.002,fwidth(i.local.y*4)),station);
                float hatch = length((i.local.xy-float2(0,-0.1))/float2(0.115,0.32));
                float hatchLine = (1-smoothstep(0.045,0.045+max(0.01,fwidth(hatch)),abs(hatch-1)))
                    * smoothstep(0.32,0.43,i.local.z);
                float tone = saturate(_Fill + rim*_ContourStrength + seams*0.12
                    + dots*_PointStrength*lerp(0.5,1,random.y) + hatchLine*0.65 + i.accent*0.1);
                return half4(_HullColor.rgb + _BaseColor.rgb*tone,1);
            }
            ENDHLSL
        }
    }
}
