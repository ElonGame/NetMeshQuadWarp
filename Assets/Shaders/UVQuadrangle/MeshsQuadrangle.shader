Shader "Hidden/MeshsQuadrangle"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	sampler2D _CheckBoardTex;
	sampler2D _SpoutTex;

	float _Homography[9];
	float _InvHomography[9];
	float _InvHomographyUV[9];

	float4 _VertexValues[4];
	float4 _UVValues[4];
	float4x4 _HomographyMat;

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 local_pos : TEXCOORD1;
		float2 vertexTest : TEXCOORD2;
		float2 uv : TEXCOORD0;
	};

	v2f vert(appdata_img i)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(i.vertex);
		float3 q0 = lerp(_VertexValues[0], _VertexValues[1], i.texcoord.x);
		float3 q1 = lerp(_VertexValues[2], _VertexValues[3], i.texcoord.x);
		float3 qs = lerp(q0, q1, i.texcoord.y);
		float t = _Homography[6] * i.texcoord.x + _Homography[7] * i.texcoord.y + _Homography[8];
		float x = (_Homography[0] * i.texcoord.x + _Homography[1] * i.texcoord.y + _Homography[2]) / t;
		float y = (_Homography[3] * i.texcoord.x + _Homography[4] * i.texcoord.y + _Homography[5]) / t;
		float2 xy = float2(x, y);

		float2 p = xy;
		float s = _InvHomography[6] * p.x + _InvHomography[7] * p.y + _InvHomography[8];
		float u = (_InvHomography[0] * p.x + _InvHomography[1] * p.y + _InvHomography[2]) / s;
		float v = (_InvHomography[3] * p.x + _InvHomography[4] * p.y + _InvHomography[5]) / s;
		float2 uv = float2(u, v);
		o.local_pos = xy;		//i.vertex.yzx;//qs;// uv;//
		o.vertexTest = i.vertex.yzx;
		o.uv = i.texcoord;
		return o;
	}

	float plot(float2 st, float2 pct) {
		return smoothstep(pct - 0.02, pct, st.y) -
			smoothstep(pct, pct + 0.02, st.y);
	}
	float4 frag0(v2f i) : SV_Target
	{
		float3 q0 = lerp(_UVValues[0], _UVValues[1], i.uv.x);
		float3 q1 = lerp(_UVValues[2], _UVValues[3], i.uv.x);
		float3 qs = lerp(q0, q1, i.uv.y);

		float j = _Homography[6] * qs.x + _Homography[7] * qs.y + _Homography[8];
		float g = (_Homography[0] * qs.x + _Homography[1] * qs.y + _Homography[2]) / j;
		float h = (_Homography[3] * qs.x + _Homography[4] * qs.y + _Homography[5]) / j;
		float2 gh = float2(g, h);

		float2 p = i.local_pos;
		float s = _InvHomography[6] * p.x + _InvHomography[7] * p.y + _InvHomography[8];
		float u = (_InvHomography[0] * p.x + _InvHomography[1] * p.y + _InvHomography[2]) / 1;
		float v = (_InvHomography[3] * p.x + _InvHomography[4] * p.y + _InvHomography[5]) / 1;
		float2 uv = float2(u/s, v/s);

		float t = _InvHomographyUV[6] * i.uv.x + _InvHomographyUV[7] * i.uv.y + _InvHomographyUV[8];
		float x = (_InvHomographyUV[0] * i.uv.x + _InvHomographyUV[1] * i.uv.y + _InvHomographyUV[2]) / t;
		float y = (_InvHomographyUV[3] * i.uv.x + _InvHomographyUV[4] * i.uv.y + _InvHomographyUV[5]) / t;
		float2 xy = float2(x, y);

		float a = 1.0;
		a *= x < 0.0 ? 0.0 : 1.0;
		a *= x > 1.0 ? 0.0 : 1.0;
		a *= y < 0.0 ? 0.0 : 1.0;
		a *= y > 1.0 ? 0.0 : 1.0;

		float3 p0 = lerp(_UVValues[0], _UVValues[1], uv.x);
		float3 p1 = lerp(_UVValues[2], _UVValues[3], uv.x);
		float3 ps = lerp(p0, p1, uv.y);

		float2 pctuv = i.local_pos;//ps;// 
		float pct = plot(pctuv, pctuv.x);
		float3 color = pct * float3(0, 1, 0);

		return tex2D(_SpoutTex, uv) * 1;// +float4(color, 1);
	}

	ENDCG

	SubShader
	{
		// No culling or depth
		//Cull Off ZWrite Off ZTest Always

		// Pass 0: calculate gradient
		Pass
		{
			//Blend One One
			CGPROGRAM
			#pragma vertex   vert
			#pragma fragment frag0
			ENDCG
		}
	}
}

