Shader "Custom/URPSimpleLit_ReflectionProbe"
{
    Properties
    {
        _Color ("Color", Color) = (1,0,0,1)
        _ReflectionCube ("Reflection Cubemap", Cube) = "" {}
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.5
        _SpecColor ("Specular Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range(1, 128)) = 32
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            float4 _Color;
            float4 _SpecColor;
            float _Shininess;
            samplerCUBE _ReflectionCube;
            float _ReflectionStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.worldNormal = normalize(TransformObjectToWorldNormal(v.normal));
                o.worldPos = TransformObjectToWorld(v.vertex);
                return o;
            }

            float3 Lambert(float3 lightColor, float3 lightDir, float3 normal)
            {
                float NdotL = saturate(dot(normal, lightDir));
                return lightColor * NdotL;
            }

            float3 BlinnPhongSpecular(float3 lightDir, float3 viewDir, float3 normal, float3 specColor, float shininess)
            {
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normal, halfDir));
                float spec = pow(NdotH, shininess);
                return specColor.rgb * spec;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 reflectDir = reflect(-viewDir, normal);

                // Sample reflection color from cubemap
                float3 reflectionColor = texCUBE(_ReflectionCube, reflectDir).rgb;

                // Main light
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float3 lightCol = Lambert(_MainLightColor.rgb * unity_LightData.z, lightDir, normal);
                float3 specCol = BlinnPhongSpecular(lightDir, viewDir, normal, _SpecColor.rgb, _Shininess);

                // Additional lights
                uint count = GetAdditionalLightsCount();
                for (uint j = 0; j < count; j++)
                {
                    Light l = GetAdditionalLight(j, i.worldPos);
                    float3 ld = l.direction;
                    float3 lambert = Lambert(l.color * (l.distanceAttenuation * l.shadowAttenuation), ld, normal);
                    float3 specular = BlinnPhongSpecular(ld, viewDir, normal, _SpecColor.rgb, _Shininess);
                    lightCol += lambert;
                    specCol += specular;
                }

                float3 baseColor = _Color.rgb + lightCol + specCol;
                float3 finalColor = lerp(baseColor, reflectionColor, _ReflectionStrength);

                return float4(finalColor, _Color.a);
            }
            ENDHLSL
        }
    }
}
