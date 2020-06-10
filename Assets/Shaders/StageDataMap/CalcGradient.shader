Shader "Hidden/StageDataMap/CalcGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	// -------------------------------------------------------------------------------
	// 勾配を計算
	// -------------------------------------------------------------------------------
	float4 frag_calculateGradient(v2f_img i) : SV_Target
	{
		float pN = tex2D(_MainTex, i.uv.xy + _MainTex_TexelSize.xy * float2( 0,  1)).y;
		float pS = tex2D(_MainTex, i.uv.xy + _MainTex_TexelSize.xy * float2( 0, -1)).y;
		float pE = tex2D(_MainTex, i.uv.xy + _MainTex_TexelSize.xy * float2( 1,  0)).y;
		float pW = tex2D(_MainTex, i.uv.xy + _MainTex_TexelSize.xy * float2(-1,  0)).y;
		float p  = tex2D(_MainTex, i.uv.xy + _MainTex_TexelSize.xy * float2( 0,  0)).y;
		
		//pN = pN == 0.0 ? p : pN;
		//pS = pS == 0.0 ? p : pS;
		//pE = pE == 0.0 ? p : pE;
		//pW = pW == 0.0 ? p : pW;

		float2 grad = 0.5 * float2(pE - pW, pN - pS);

		grad = clamp(grad, -0.1, 0.1);

		return float4(grad.xy, 0.0, 1.0);
	}

	ENDCG

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

		// Pass 0: calculate gradient
        Pass
        {
            CGPROGRAM
            #pragma vertex   vert_img
            #pragma fragment frag_calculateGradient
            ENDCG
        }
    }
}

