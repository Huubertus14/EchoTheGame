Shader "Echo/Underwater Unlit"
{
    Properties
    {
        _RockColor ("Revealed surface tint", Color) = (0.035,0.12,0.15,1)
        _HiddenColor ("Background / unhit color", Color) = (0.007843138,0.047058824,0.07450981,1)
        _Contour ("Contour strength", Float) = 0.025
        _PointSpacing ("Sonar point spacing (world units)", Range(0.08,0.6)) = 0.19
        _PointStrength ("Sonar point brightness", Range(0,2)) = 1
        _SurfaceFill ("Faint surface fill", Range(0,0.5)) = 0.18
        _SonarBrightness ("Sonar brightness", Range(0.5,3)) = 1.35
        [Toggle] _SonarObject ("Reveal solid object (off for seabed)", Float) = 1
        [HideInInspector] _EchoObjectId ("Acoustic object identifier", Float) = 0
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
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 world : TEXCOORD0; float3 normal : TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
            float4 _RockColor, _HiddenColor;
            float _Contour, _SonarObject, _EchoObjectId;
            float _PointSpacing, _PointStrength, _SurfaceFill, _SonarBrightness;
            CBUFFER_END
            #include "SonarOcclusion.hlsl"
            float2 PointHash(float2 cell)
            {
                return frac(sin(float2(dot(cell,float2(127.1,311.7)),dot(cell,float2(269.5,183.3)))) * 43758.5453);
            }
            float SonarPoints(float3 world, float spacing)
            {
                // World-anchored samples remain still while the wave moves across them.
                float2 grid = (world.xz + world.y * float2(0.21,0.13)) / spacing;
                float2 cell = floor(grid);
                float2 random = PointHash(cell);
                float2 offset = frac(grid) - (0.25 + random * 0.5);
                float radius = lerp(0.11,0.21,random.x);
                float aa = max(0.001, length(fwidth(grid)) * 0.45);
                float pointMask = 1 - smoothstep(max(0,radius-aa),radius+aa,length(offset));
                return pointMask * lerp(0.5,1,random.y) * step(0.15,random.x);
            }
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
                if (_SonarObject < 0.5) return half4(_HiddenColor.rgb, 1);
                // Graphic facet tones, independent of all scene lights and shadows.
                float facet = 0.55 + 0.45 * saturate(dot(normalize(i.normal), normalize(float3(-0.4,1,0.25))));
                // Fine bathymetric traces and irregular soundings, like the menu artwork.
                float height = (i.world.y + sin(i.world.x * 0.7 + sin(i.world.z * 0.45)) * 0.065) * 3.8;
                float depthLine = abs(frac(height) - 0.5);
                float contourAA = max(0.002, fwidth(height));
                float contourLine = 1 - smoothstep(0.04,0.04+contourAA,depthLine);
                float traceBreaks = 0.65 + 0.35 * SonarPoints(i.world,0.13);
                float points = SonarPoints(i.world,max(0.08,_PointSpacing));
                float finePoints = SonarPoints(i.world+float3(7.3,0,3.1),max(0.08,_PointSpacing)*0.58);
                float tone = _SurfaceFill * (0.5 + 0.5 * facet)
                    + _PointStrength * (points * 0.9 + finePoints * 0.22)
                    + contourLine * traceBreaks * (0.45 + _Contour * 3);
                // Lift readable coverage without clipping individual color channels.
                tone = saturate(tone * _SonarBrightness);
                float3 echo = 0;
                for (int p = 0; p < _EchoSonarCount; p++)
                {
                    float4 wave = _EchoSonarWaves[p];
                    float d = distance(i.world.xz, _EchoSonarOrigins[p].xy);
                    float behind = wave.x - d;
                    // Exactly zero in front of the wave and outside its original range.
                    float reveal = smoothstep(0, wave.y, behind)
                        * saturate((d - wave.w) / max(0.001, wave.z - wave.w))
                        * step(d, _EchoSonarOrigins[p].z);
                    if (reveal > 0) reveal *= EchoSurfaceVisible(i.world, p, _EchoObjectId);
                    // Brightness only: every sample retains the emitting player's hue.
                    echo = max(echo, _EchoSonarColors[p].rgb * tone * reveal);
                }
                // Opaque, exactly flat background color at rest, including the seabed.
                // Keep depth writes: sonar must not reveal geometry through an unhit rock.
                return half4(_HiddenColor.rgb + echo, 1);
            }
            ENDHLSL
        }
    }
}
