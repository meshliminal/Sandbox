    Shader "Custom/URP_CylinderGradientShader"
    {
        Properties
        {
            _BaseColor("Base Color", Color) = (1,1,1,1)
            _TopTransparency("Top Transparency", Range(0,1)) = 0.0
        }
        SubShader
        {
            Tags { "RenderType"="Transparent" "Queue"="Transparent" }
            LOD 200

            Pass
            {
                Name "ForwardLit"
                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off
                Cull Off

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                };

                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float3 worldPos : TEXCOORD0;
                };

                float4 _BaseColor;
                float _TopTransparency;

                Varyings vert (Attributes v)
                {
                    Varyings o;
                    o.positionCS = TransformObjectToHClip(v.positionOS);
                    o.worldPos = TransformObjectToWorld(v.positionOS);
                    return o;
                }

                half4 frag (Varyings i) : SV_Target
                {
                    // Fokozatos áttetszőség beállítása a henger teteje felé
                    float height = i.worldPos.y;
                    float alpha = lerp(_BaseColor.a, _TopTransparency, saturate(height));

                    return half4(_BaseColor.rgb, alpha);
                }
                ENDHLSL
            }
        }
    }