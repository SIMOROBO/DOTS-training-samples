Shader "Instanced/InstancedShaderAnimals" {
    SubShader{

        Pass {

            Tags {"LightMode" = "ForwardBase"}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            //#pragma enable_d3d11_debug_symbols
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
                float3x3 rotate = (float3x3)transform;
                float3 scale = float3(0.5, 0.5, 0.5 + 0.25f * transform._44);
                transform._11_21_31 *= scale.x;
                transform._12_22_32 *= scale.y;
                transform._13_23_33 *= scale.z;
                transform._44 = 1.0;

                float3 worldPosition = mul(transform, float4(v.vertex.xyz, 1.0f)).xyz;
                //v.vertex.xyz = worldPosition;

                v2f o;
                // half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half3 worldNormal = mul(rotate, v.normal);
                o.worldNormal = worldNormal;

                //o.pos = UnityObjectToClipPos(v.vertex);
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.ambient = ShadeSH9(half4(worldNormal, 1.0));

                float3 c = float3(
                            transform[3][0],
                            transform[3][1],
                            transform[3][2]);

                o.color = float4(v.color.xyz * c, 1.0f);
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
        } // End pass

        // shadow caster rendering pass, implemented manually
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

            #include "UnityCG.cginc"

            StructuredBuffer<float4x4> transformBuffer;

            struct v2f {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v, uint instanceID : SV_InstanceID)
            {
                float4x4 transform = transformBuffer[instanceID];
                float3x3 rotate = (float3x3)transform;
                float3 scale = float3(0.1, 0.01, transform._44);
                transform._11_21_31 *= scale.x;
                transform._12_22_32 *= scale.y;
                transform._13_23_33 *= scale.z;
                transform._44 = 1.0;

                float3 worldPosition = mul(transform, float4(v.vertex.xyz, 1.0f)).xyz;
                v.vertex.xyz = worldPosition;

                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}
