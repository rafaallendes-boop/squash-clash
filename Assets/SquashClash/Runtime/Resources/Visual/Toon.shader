Shader "SquashClash/Toon"
{
    // Art-pass shader: vertex colour x base colour with a fixed two-tone light and a soft rim.
    // No realtime lights, shadows or keywords: the cost is one unlit pass.
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}
        _ShadeColor ("Shade", Color) = (0.60,0.64,0.80,1)
        _RimColor ("Rim (a = strength)", Color) = (1,1,1,0.25)
        _LightDir ("Light direction (world, toward light)", Vector) = (-0.3,0.8,0.52,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Back
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _ShadeColor;
                half4 _RimColor;
                float4 _LightDir;
            CBUFFER_END
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; half3 normalWS : TEXCOORD1; half3 viewWS : TEXCOORD2; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.color = input.color * _BaseColor;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewWS = GetWorldSpaceViewDir(positionWS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half3 n = normalize(input.normalWS);
                half lit = smoothstep(-0.06, 0.10, dot(n, normalize(_LightDir.xyz)));
                half3 base = input.color.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                half3 c = base * lerp(_ShadeColor.rgb, half3(1,1,1), lit);
                half rim = 1 - saturate(dot(n, normalize(input.viewWS)));
                c = lerp(c, _RimColor.rgb, rim * rim * rim * _RimColor.a);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
