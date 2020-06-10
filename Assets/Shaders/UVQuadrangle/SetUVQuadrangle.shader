Shader "Hidden/SetUVQuadrangle"
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
	sampler2D _ParticleTex;

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
		float2 xy = float2(x, y);	// float2(2 * (x - 0.5), 2 * (0.5 - y));

		o.local_pos = xy;			//xy;// i.vertex; //qs
		o.vertexTest = i.vertex;
		o.uv = i.texcoord;
		return o;
	}
	float plot(float2 st, float2 pct) {
		return smoothstep(pct - 0.02, pct, st.y) -
			smoothstep(pct, pct + 0.02, st.y);
	}
	float4 frag0(v2f i) : SV_Target
	{
		float3 q0 = lerp(_VertexValues[0], _VertexValues[1], i.uv.x);
		float3 q1 = lerp(_VertexValues[2], _VertexValues[3], i.uv.x);
		float3 qs = lerp(q0, q1, i.uv.y);//float3(i.uv.xy, 0);// 
		/*float t = _Homography[6] * qs.x + _Homography[7] * qs.y + _Homography[8];
		float x = (_Homography[0] * qs.x + _Homography[1] * qs.y + _Homography[2]) / t;
		float y = (_Homography[3] * qs.x + _Homography[4] * qs.y + _Homography[5]) / t;
		float2 xy = float2(x, y);*/

		float2 p = i.local_pos;//qs.xy;//i.vertexTest;//
		float s  = _InvHomography[6] * p.x + _InvHomography[7] * p.y + _InvHomography[8];
		float u  = (_InvHomography[0] * p.x + _InvHomography[1] * p.y + _InvHomography[2]) / s;
		float v  = (_InvHomography[3] * p.x + _InvHomography[4] * p.y + _InvHomography[5]) / s;
		float2 uv = float2(u, v);

		float2 pt = i.uv;
		float t = _InvHomographyUV[6] * pt.x + _InvHomographyUV[7] * pt.y + _InvHomographyUV[8];
		float x = (_InvHomographyUV[0] * pt.x + _InvHomographyUV[1] * pt.y + _InvHomographyUV[2]) / t;
		float y = (_InvHomographyUV[3] * pt.x + _InvHomographyUV[4] * pt.y + _InvHomographyUV[5]) / t;
		float2 xy = float2(x, y);

		float a = 1.0;
		a *= x < 0.0 ? 0.0 : 1.0;
		a *= x > 1.0 ? 0.0 : 1.0;
		a *= y < 0.0 ? 0.0 : 1.0;
		a *= y > 1.0 ? 0.0 : 1.0;

		fixed3 p3 = fixed3(uv, 0);
		fixed3 a3 = fixed3(_VertexValues[0].xy, 0);
		fixed3 b3 = fixed3(_VertexValues[1].xy, 0);
		fixed3 c3 = fixed3(_VertexValues[3].xy, 0);
		fixed3 d3 = fixed3(_VertexValues[2].xy, 0);
		int a1 = cross(p3 - a3, b3 - a3) * cross(p3 - d3, c3 - d3);
		int b1 = cross(p3 - a3, d3 - a3) * cross(p3 - b3, c3 - b3);
		//uv *= a1 < 0 && b1 < 0 ? 1 : 0;
		//uv *= a1 < 0 && b1 < 0 ? 1 : 0;

		float3 p0 = lerp(_UVValues[0], _UVValues[1], xy.x);
		float3 p1 = lerp(_UVValues[2], _UVValues[3], xy.x);
		float3 ps = lerp(p0, p1, xy.y);

		float2 pctuv = uv;
		float pct = plot(pctuv, pctuv.x);
		float3 color = pct * float3(0, 1, 0);
		//return fixed4(pctuv, 0, 0) * a + fixed4(color.xyz, 0);

		//return tex2D(_CheckBoardTex, i.vertexTest) * a;
		//return float4(i.vertexTest , 0.0, 0.0);
		//return float4(i.uv , 0.0, 0.0)*a;
		//return tex2D(_CheckBoardTex, i.uv.xy) * a;
		//return tex2D(_CheckBoardTex, xy) * a;
		//return float4(xy , 0.0, 0.0);
		//return float4(qs.xy , 0.0, 0.0)*1;
		//return tex2D(_CheckBoardTex, i.local_pos) * 1;
		//return float4(i.local_pos , 0.0, 0.0)*1;
		//return tex2D(_CheckBoardTex, ps) * 1;
		//return float4(uv , 0.0, 0.0) * a;
		//return tex2D(_CheckBoardTex, ps.xy) * a + fixed4(color.xyz, 0);
		//return float4(ps.xyz, 0.0) * a;
		//return float4(uv.xy, 0, 0)* a;
		//return tex2D(_CheckBoardTex, ps.xy) * a;
		return tex2D(_CheckBoardTex, uv.xy) * a;
		//return tex2D(_ParticleTex, ps.xy * a) + fixed4(ps.xy,0,0) * a;
		//return float4(qs.xy, 0, 0);
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
			#pragma vertex   vert//vert_img
			#pragma fragment frag0
			ENDCG
		}
	}
}

