Shader "Instanced/InstancedShader" {
		SubShader{

			Pass {

				Tags {"LightMode" = "ForwardBase"}

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma enable_d3d11_debug_symbols
				#pragma target 4.5

				#include "UnityCG.cginc"

				StructuredBuffer<float4x4> transformBuffer;

				struct v2f
				{
					float4 pos : SV_POSITION;
				};

				v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
				{
					float4x4 transform = transformBuffer[instanceID];
					float3 worldPosition = mul(transform, float4(v.vertex.xyz, 1)).xyz;

					v2f o;
					o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					return fixed4(1,0,0,1);
				}

				ENDCG
			}
	}
}
