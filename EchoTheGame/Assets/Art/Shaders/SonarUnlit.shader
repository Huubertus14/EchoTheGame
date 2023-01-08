Shader "Unlit/SonarUnlit"
{
	Properties {
		_SonarBaseColor("Base Color",  Color) = (00, 0.0, 0.0, 0)
		_SonarWaveColor("Wave Color",  Color) = (1.0, 0.0, 0.0, 0)
		_SonarWaveParams("Wave Params", Vector) = (1, 20, 20, 10)
		_SonarWaveVector("Wave Vector", Vector) = (0, 0, 1, 0)
		_SonarAddColor("Add Color",   Color) = (0, 0, 0, 0)
		_SonarStep("Step", Float) = 0.0
	}

	SubShader {
		Pass {
			CGPROGRAM

	#pragma vertex vert  
	#pragma fragment frag 

	#include "UnityCG.cginc" 
			uniform float4 _SonarWaveColor;
			uniform float4 _SonarWaveParams; // Amp, Exp, Interval, Speed
			uniform float3 _SonarWaveVector;
			uniform float4 _SonarAddColor;
			uniform float _SonarStep;

			struct vertexInput {
				float4 vertex : POSITION;
			};

			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD0;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				output.pos = UnityObjectToClipPos(input.vertex);
				output.worldPos = mul(unity_ObjectToWorld, input.vertex);

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				float w = length(input.worldPos - _SonarWaveVector) - 0.001;
				w -= (_Time.y *_SonarWaveParams.w);
				float p = _SonarWaveParams.y;
				w = (pow(w, p) - pow(-w, p * 4)) * 0.5;
				return w * _SonarWaveColor;
			}

			ENDCG
		}
	}
}
