Shader "Hidden/StageDataMap/SetQuadrangleData"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	sampler2D _CheckBoardTex;

	float _Homography[9];
	float _InvHomography[9];

	float4 _VertexValues[4];

	float4 frag0(v2f_img i) : SV_Target
	{
		float2 p = i.uv.xy;
		float s = _InvHomography[6] * p.x + _InvHomography[7] * p.y + _InvHomography[8];
		float u = (_InvHomography[0] * p.x + _InvHomography[1] * p.y + _InvHomography[2]) / s;
		float v = (_InvHomography[3] * p.x + _InvHomography[4] * p.y + _InvHomography[5]) / s;
		float2 uv = float2(u, v);

		float a = 1.0;
		a *= u < 0.0 ? 0.0 : 1.0;
		a *= u > 1.0 ? 0.0 : 1.0;
		a *= v < 0.0 ? 0.0 : 1.0;
		a *= v > 1.0 ? 0.0 : 1.0;

		float3 p0 = lerp(_VertexValues[0], _VertexValues[1], u);
		float3 p1 = lerp(_VertexValues[2], _VertexValues[3], u);
		float3 ps = lerp(p0, p1, v);

		float4 col = float4(ps.xyz, 0.0) * a;
		//float4 col = tex2D(_CheckBoardTex, uv.xy) *a;
		return col;
	}

	ENDCG

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

		// Pass 0: calculate gradient
        Pass
        {
			Blend One One
            CGPROGRAM
            #pragma vertex   vert_img
            #pragma fragment frag0
            ENDCG
        }
    }
}

