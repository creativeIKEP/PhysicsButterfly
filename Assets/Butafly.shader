
Shader "Custom/Butafly" {
    Properties {
         _MainColor ("MainColor", Color) = (1, 1, 1, 1) 
        _MaskTex ("MaskTex", 2D) = "white" {}
    }
    SubShader {
        Tags {"RenderType"="Queue"}
        LOD 200
        Cull off
        
        Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        float4 _MainColor;
        sampler2D _MaskTex;
        
        
        
        struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
        struct v2f
        {
            half2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };
        
        
        
        v2f vert(appdata v)
        {
            float angle_deg = 30*abs(sin(500*_Time)) + 30;
            float angle_rad = radians(angle_deg); 
            float x = v.vertex.x * cos(angle_rad);
            float y = v.vertex.x * sin(angle_rad);
            v.vertex.xyz = float3(x, abs(y), v.vertex.z);
            v.vertex.xyz += float3((float)sin(100*_Time) * 0.1f, (float)sin(10*_Time), 0);//move animation
            
            v2f o;
            o.vertex = UnityObjectToClipPos (v.vertex);
            o.uv = v.uv;
            return o;
        }
        
        
        fixed4 frag (v2f i) : COLOR
        {
            fixed4 col2 = tex2D(_MaskTex, i.uv);
            if(col2.a<0.1){discard;}
            
            fixed4 result = fixed4(_MainColor.r, _MainColor.g, _MainColor.b, col2.a);
            
            
            return result;
        }
        
        ENDCG
        }
    }
    FallBack "Diffuse"
}