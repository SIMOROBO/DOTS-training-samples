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
                #include "AutoLight.cginc"
                #include "UnityLightingCommon.cginc"

				StructuredBuffer<float4x4> transformBuffer;

				struct v2f
				{
					float4 pos : SV_POSITION;
                    float4 color : COLOR0;
                    fixed3 ambient : COLOR1;
                    float3 worldNormal : TEXCOORD0;
                    
                    SHADOW_COORDS(1)
				};

				v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
				{
					float4x4 transform = transformBuffer[instanceID];
					float3 worldPosition = mul(transform, float4(v.vertex.xyz, 1)).xyz;

					v2f o;
                    half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                    o.worldNormal = worldNormal;
                    o.ambient = ShadeSH9(half4(worldNormal,1));

					o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                    float3 c = float3(
                                transform[3][0],
                                transform[3][1],
                                transform[3][2]);

                    o.color = float4(c,1);
                    TRANSFER_SHADOW(o)

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
                    
                    half NdotL = max(0, dot(i.worldNormal, _WorldSpaceLightPos0.xyz));
                    fixed shadow = SHADOW_ATTENUATION(i);

                    fixed3 lighting = NdotL * _LightColor0.rgb * shadow + i.ambient.rgb;
                    
                    fixed4 col = fixed4(i.color.xyz,1);
                    col.rgb *=  lighting;

					return col;
				}

				ENDCG
			}
	}
    FallBack "Diffuse"
}
