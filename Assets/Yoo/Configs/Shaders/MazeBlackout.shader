Shader "Hidden/MazeBlackout"
{
    Properties
    {
        _Threshold ("Brightness Threshold", Range(0, 1)) = 0.02
        _Softness ("Brightness Softness", Range(0.001, 1)) = 0.04
        _Boost ("Brightness Boost", Range(1, 8)) = 2.5
        _Tint ("Light Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            half _Threshold;
            half _Softness;
            half _Boost;
            half4 _Tint;

            half4 Frag(Varyings input) : SV_Target
            {
                half3 scene = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord).rgb;

                // 只保留烘焙光照产生的亮部，其余压成黑色。
                // 这样黑场中传送点上的 Point Light 照明仍然可见。
                half luminance = dot(scene, half3(0.2126h, 0.7152h, 0.0722h));
                half mask = smoothstep(_Threshold, _Threshold + _Softness, luminance);

                return half4(scene * _Boost * mask * _Tint.rgb, 1);
            }
            ENDHLSL
        }
    }
}
