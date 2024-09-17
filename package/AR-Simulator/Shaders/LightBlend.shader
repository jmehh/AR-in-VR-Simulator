Shader "ARSimulator/LightBlend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _ARTexture;

            float4x4 _InvProjMatrix;
            float4x4 _InvViewMatrix;

            float4x4 _ARViewMatrix;
            float4x4 _ARProjMatrix;

            float _nonLinearZ;

            float _visorRelLuminance;
            float _displayRelLuminance;
            float _worldRelLuminance;

            fixed4 frag(v2f i) : SV_Target
            {
                float4 ndc = float4((i.uv.xy - float2(0.5, 0.5)) * 2.0, _nonLinearZ, 1.0);

                float4 worldPos = mul(_InvProjMatrix, ndc);
                worldPos /= worldPos.w;
                worldPos = mul(_InvViewMatrix, worldPos);

                float4 arNDC = mul(_ARViewMatrix, worldPos);
                arNDC = mul(_ARProjMatrix, arNDC);
                arNDC /= arNDC.w;
                
                float2 arUV = (arNDC + float2(1.0, 1.0)) * 0.5;

                float4 composit;

                if (all(arUV >= 0.0) && all(arUV <= 1.0)) {
                    float4 arCol = tex2D(_ARTexture, arUV);
                    composit = _worldRelLuminance * tex2D(_MainTex, i.uv) + _displayRelLuminance * arCol;
                } else {
                    composit = _visorRelLuminance * tex2D(_MainTex, i.uv);
                }
                
                return composit;
            }
            ENDCG
        }
    }
}
