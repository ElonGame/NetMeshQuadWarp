Shader "Custom/UVRef" {

    Properties{
        _MainTex("Base (RGB)", 2D) = "white" {}
    }

        SubShader{
            //Cull off

            pass {
                CGPROGRAM
                #include "UnityCG.cginc"
                uniform sampler2D _MainTex;
                sampler2D _UvQuadRefTex;
                sampler2D _FinalRenderTex;
                float4 _MainTex_ST;
                float4 uv;
                #pragma vertex vert          
                #pragma fragment frag

                struct vertexInput {
                    float4 vertex : POSITION;
                    float4 texcoord  : TEXCOORD0;
                    float4 texcoord1  : TEXCOORD1;
                };

                struct vertexOutput {
                    float4 pos : SV_POSITION;
                    float2 uv  : TEXCOORD0;
                    float2 uv2  : TEXCOORD1;
                };

                vertexOutput vert(vertexInput input)
                {
                    vertexOutput output;
                    output.pos = UnityObjectToClipPos(input.vertex);

                    output.uv = input.texcoord;
                    output.uv2 = input.texcoord1;

                    return output;
                }

                float4 frag(vertexOutput input) : COLOR
                {
                     //return  tex2D(_MainTex, float2(input.uv) * _MainTex_ST.xy + _MainTex_ST.zw);
                    //return  tex2D(_UvQuadRefTex, float2(input.uv));
                    uv = tex2D(_UvQuadRefTex, (input.uv)); //this sampling will be pixelate result

                    return  uv;// +tex2D(_FinalRenderTex, uv.xy);//float4(input.uv,0,0)+
                    return  tex2D(_MainTex, input.uv.xy);

                }

                ENDCG
            }
    }
}