// Draws 3D TextMesh text (e.g. the licence plate number) as part of the scene.
// Unity's default font shader draws text on top of everything, so the plate number would show
// through walls. This one depth-tests like normal geometry (ZTest LEqual), so walls and the car hide it,
// and it fades into the scene fog like everything else. Built-in Render Pipeline.
Shader "Project/TextOnSurface"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Text Colour", Color) = (0.1, 0.1, 0.2, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        ZWrite Off
        ZTest LEqual
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        Offset -1, -1   // pull slightly towards the camera so text never flickers against the plate behind it

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; UNITY_FOG_COORDS(1) };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;   // TextMesh colour times material colour
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Font textures store the letter shapes in the alpha channel.
                fixed4 col = i.color;
                col.a *= tex2D(_MainTex, i.uv).a;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
