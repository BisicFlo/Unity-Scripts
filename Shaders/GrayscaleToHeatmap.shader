Shader "Custom/GrayscaleToHeatmap"
{
    /// This HLSL Shader transform grayscale input texture and converts it into a false-color heatmap 
    /// the classic style you see in thermal imaging, scientific visualization, performance heatmaps, etc.
    /// By default black = Blue = cold , bright = red = hot 


    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold1 ("Threshold 1 (Blue to Green)", Range(0, 1)) = 0.33
        _Threshold2 ("Threshold 2 (Green to Yellow)", Range(0, 1)) = 0.66
        _Precision ("Precision (Number of Steps)", Range(1, 100)) = 10

        _Color1 ("Color 1 (Low)", Color) = (0, 0, 1, 1) // Blue
        _Color2 ("Color 2 (Mid-Low)", Color) = (0, 1, 0, 1) // Green
        _Color3 ("Color 3 (Mid-High)", Color) = (1, 1, 0, 1) // Yellow
        _Color4 ("Color 4 (High)", Color) = (1, 0, 0, 1) // Red
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Threshold1;
            float _Threshold2;
            float _Precision;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float gray = tex2D(_MainTex, i.uv).r;

                // Apply precision control (number of steps)
                gray = round(gray * _Precision) / _Precision;

                // Define the heatmap colors with adjustable thresholds, precision, and customizable colors
                float3 heatColor;
                if (gray < _Threshold1)
                    heatColor = lerp(_Color1.rgb, _Color2.rgb, gray / _Threshold1); // Color 1 to Color 2
                else if (gray < _Threshold2)
                    heatColor = lerp(_Color2.rgb, _Color3.rgb, (gray - _Threshold1) / (_Threshold2 - _Threshold1)); // Color 2 to Color 3
                else
                    heatColor = lerp(_Color3.rgb, _Color4.rgb, (gray - _Threshold2) / (1 - _Threshold2)); // Color 3 to Color 4

                return float4(heatColor, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
