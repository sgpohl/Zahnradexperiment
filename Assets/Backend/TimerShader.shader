Shader "Unlit/TimerShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Progress ("Progress", float) = 0
		_Scale("BorderScale", float) = 10
		_PColor ("ProgressColor", Color) = (0,0,0,1)
		_BColor("BaseColor", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite off

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
            float4 _MainTex_ST;

			fixed4 _PColor;
			fixed4 _BColor;
			float _Progress;
			float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _BColor;
				float fac = (saturate((_Progress - i.uv.y) * _Scale));
				col = (1.f-fac)*col + fac*col*_PColor;
                return col;
            }
            ENDCG
        }
    }
}
