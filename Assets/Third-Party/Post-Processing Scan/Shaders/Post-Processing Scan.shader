// URP 17 (Render Graph) port of the Post-Processing Scan effect.
// Same shader properties as the original pack, so existing materials keep working.

Shader "Mirza Beig/Post-Processing Scan"
{
    Properties
    {
        [HDR] _Colour ("Colour", Color) = (1,1,1,1)
        _Origin ("Origin", Vector) = (0,0,0,0)
        _Power ("Power", Float) = 10
        _Tiling ("Tiling", Float) = 1
        _Speed ("Speed", Float) = 1
        _MaskRadius ("Mask Radius", Float) = 5
        _MaskHardness ("Mask Hardness", Range(0, 1)) = 1
        _MaskPower ("Mask Power", Float) = 1
        _MultiplyBlend ("Multiply Blend", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        ZTest Always
        Cull Off
        ZWrite Off

        Pass
        {
            Name "Scan"

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float3 _Origin;
            float4 _Colour;
            float _Power;
            float _Tiling;
            float _Speed;
            float _ScanTime;
            float _MaskRadius;
            float _MaskHardness;
            float _MaskPower;
            float _MultiplyBlend;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // Rebuild the world position of this pixel from the depth buffer.
                float rawDepth = SampleSceneDepth(uv);
                float3 worldPosition = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                float distanceToOrigin = distance(worldPosition, _Origin);

                // Expanding rings (time only advances while a pulse is active).
                float rings = frac(distanceToOrigin * _Tiling - _ScanTime * _Speed);
                float pulse = pow(rings, _Power);

                // Spherical mask around the origin (inverse smoothstep, same as the original).
                float maskInner = lerp(0.0, _MaskRadius + 0.999, _MaskHardness);
                float mask = pow(smoothstep(_MaskRadius + 1.0, maskInner, distanceToOrigin), _MaskPower);

                float scanAmount = saturate(pulse * mask * _Colour.a);

                float4 screenColour = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                float3 scanColour = lerp(_Colour.rgb, _Colour.rgb * screenColour.rgb, _MultiplyBlend);

                return half4(lerp(screenColour.rgb, scanColour, scanAmount), 1.0h);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
