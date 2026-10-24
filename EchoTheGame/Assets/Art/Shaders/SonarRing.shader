Shader "Echo/Sonar Ring"
{
    Properties
    {
        [HideInInspector] _EchoPulseSlot ("Pulse slot", Float) = 0
        _PointSpacing ("Sonar ring point spacing", Range(0.08,0.5)) = 0.22
        _TraceStrength ("Faint connecting trace", Range(0,0.3)) = 0.14
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SonarOcclusion.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _EchoPulseSlot;
            float _PointSpacing, _TraceStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; half4 color : COLOR; float3 world : TEXCOORD0; float across : TEXCOORD1; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color;
                o.world = TransformObjectToWorld(v.positionOS.xyz);
                o.across = v.uv.y * 2 - 1;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                clip(EchoVisible(i.world, (int)_EchoPulseSlot, 0) - 0.5);
                float2 delta = i.world.xz - _EchoSonarOrigins[(int)_EchoPulseSlot].xy;
                float radius = max(0.01,length(delta));
                float angle = atan2(delta.y,delta.x) / TWO_PI;
                float count = max(12,round(TWO_PI * radius / max(0.08,_PointSpacing)));
                float along = (frac(angle * count + 0.5) - 0.5) * TWO_PI * radius / count;
                float distanceToPoint = length(float2(along,i.across*0.06));
                float aa = max(0.002,fwidth(distanceToPoint));
                float pointMask = 1-smoothstep(0.04,0.04+aa,distanceToPoint);
                float glow = exp2(-abs(i.across)*abs(i.across)*5);
                half4 color = i.color;
                color.a *= saturate(pointMask + _TraceStrength * glow);
                return color;
            }
            ENDHLSL
        }
    }
}
