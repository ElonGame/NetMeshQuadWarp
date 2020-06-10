Shader "Hidden/StageDataMap/SetTriangleData"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	float2 _UV0;
	float2 _UV1;
	float2 _UV2;

	float4 _Color0;
	float4 _Color1;
	float4 _Color2;

	float4 barycentric(float3 uv, float3 p1, float3 p2, float3 p3, float4 c1, float4 c2, float4 c3)
	{
		float3 a = p2 - p3;
		float3 b = p1 - p3;
		float3 c = uv - p3;
		float ab = a.x * b.x + a.y * b.y + a.z * b.z;
		float ac = a.x * c.x + a.y * c.y + a.z * c.z;
		float bc = b.x * c.x + b.y * c.y + b.z * c.z;
		float m = a.x * a.x + a.y * a.y + a.z * a.z;
		float n = b.x * b.x + b.y * b.y + b.z * b.z;
		float d = m * n - ab * ab;
		float u = (m * bc - ab * ac) / d;
		float v = (n * ac - ab * bc) / d;
		float w = 1.0 - u - v;
		float3 p = float3(u, v, w);
		float4 col = c1 * u + c2 * v + c3 * w;
		return ((p.x >= 0.0) && (p.x <= 1.0) && (p.y >= 0.0) && (p.y <= 1.0) && (p.z >= 0.0) && (p.z <= 1.0)) ? col : float4(0.0, 0.0, 0.0, 0.0);
	}

	float4 frag(v2f_img i) : SV_Target
	{
		float4 col = float4(tex2D(_MainTex, i.uv.xy).rgb, 0.0);

		col = barycentric(
			float3(i.uv.xy, 0.0),
			float3(_UV0.xy, 0.0),
			float3(_UV1.xy, 0.0),
			float3(_UV2.xy, 0.0),
			_Color0,
			_Color1,
			_Color2
		);

		return col;
	}
	ENDCG

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Blend One One

			CGPROGRAM
			#pragma vertex   vert_img
			#pragma fragment frag
			ENDCG
		}
	}
}

