Shader "GraphView/Grid"
{
    Properties
    {
        _GridColor ("Grid Color", Color) = (0.25, 0.25, 0.25, 1)
        _BackgroundColor ("Background Color", Color) = (0.1, 0.1, 0.1, 1)
        _GridSize ("Grid Size", Float) = 16.0
        _ScaleLimit ("Upscale Limit", Int) = 4
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
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
                float scaleFactor : TEXCOORD1;
            };

            float4 _GridColor;
            float _ScaleLimit;
            float4 _BackgroundColor;
            float4 _RtTransform;
            float _Scale;
            float _GridSize;

            fixed4 grid(float2 uv, float scaleFactor)
            {
                float2 d = fwidth(uv);
                float2 d2 = smoothstep(0.5 - d, 0.5, frac(uv + 0.5)) - smoothstep(0.5, 0.5 + d, frac(uv + 0.5));
                return max(d2.x, d2.y) * smoothstep(0.1, 0.9, scaleFactor / _ScaleLimit);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                v.uv.y = 1 - v.uv.y;
                float scale = pow(_ScaleLimit, frac(log(_Scale) / log(_ScaleLimit)));
                o.uv = (v.uv * _RtTransform.xy - _RtTransform.zw) / _GridSize / scale;
                o.scaleFactor = scale;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float scaleFactor = i.scaleFactor;
                float weight = grid(uv, scaleFactor)
                + grid(uv / _ScaleLimit, scaleFactor * _ScaleLimit);
                fixed4 col = lerp(_BackgroundColor, _GridColor, weight);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
