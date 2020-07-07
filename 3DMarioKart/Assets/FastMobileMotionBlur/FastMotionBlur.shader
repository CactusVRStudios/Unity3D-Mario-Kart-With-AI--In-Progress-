Shader "SupGames/FastMotionBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
    	Cull Off ZWrite Off ZTest Always	
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			
            sampler2D _MainTex;
			float _Distance;
			float4x4 _CurrentToPreviousViewProjectionMatrix;


            struct v2f
            {
                half4 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
				half4 uv1 : TEXCOORD1;
				half4 uv2 : TEXCOORD2;
            };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				half4 projPos=half4(v.texcoord.xy * 2.0h - 1.0h,_Distance,1.0h);
				half4 previous = mul(_CurrentToPreviousViewProjectionMatrix, projPos);
				previous /= previous.w;
				half2 vel = (previous.xy - projPos.xy)*0.166667h;
				o.uv.xy= v.texcoord.xy;
				o.uv.zw=v.texcoord.xy+vel;
				o.uv1.xy=v.texcoord.xy+vel*2.0h;
				o.uv1.zw=v.texcoord.xy+vel*3.0h;
				o.uv2.xy=v.texcoord.xy+vel*4.0h;
				o.uv2.zw=v.texcoord.xy+vel*5.0h;
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col=tex2D(_MainTex, i.uv.xy);
				col += tex2D(_MainTex, i.uv.zw);
				col += tex2D(_MainTex, i.uv1.xy);
				col += tex2D(_MainTex, i.uv1.zw);
				col += tex2D(_MainTex, i.uv2.xy);
				col += tex2D(_MainTex, i.uv2.zw);
				return col*0.166667h;
            }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			
            sampler2D _MainTex;
            sampler2D _BlurTex;
            sampler2D _MaskTex;

            struct v2f
            {
                half2 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
            };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv= v.texcoord.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col=tex2D(_MainTex, i.uv);
				fixed4 blur=tex2D(_BlurTex, i.uv);
				fixed mask =tex2D(_MaskTex,i.uv).r;
				return lerp(col,blur,mask);
            }
            ENDCG
        }
    }
}
