// Procedural night-sky skybox for the Built-in Render Pipeline.
// Draws a dark colour gradient, twinkling stars and a moon, all calculated per pixel
// from the view direction, so it needs no textures. Use it on a material, then set that
// material as the Skybox in Window > Rendering > Lighting > Environment.
Shader "Project/NightSky"
{
    Properties
    {
        [Header(Sky colours)]
        _ZenithColor ("Top of sky colour", Color) = (0.008, 0.012, 0.035, 1)
        _HorizonColor ("Horizon colour (match the fog colour)", Color) = (0.045, 0.055, 0.09, 1)
        _GroundColor ("Below horizon colour", Color) = (0.015, 0.015, 0.02, 1)
        _GradientCurve ("Gradient curve", Range(0.1, 3)) = 0.6

        [Header(Stars)]
        _StarDensity ("Star grid size (higher = more, smaller stars)", Range(20, 400)) = 140
        _StarAmount ("Chance of a star per grid cell", Range(0, 0.5)) = 0.12
        _StarSize ("Star size", Range(0.05, 0.3)) = 0.2
        _StarBrightness ("Star brightness", Range(0, 4)) = 1.6
        _TwinkleSpeed ("Twinkle speed", Range(0, 10)) = 2.5
        _TwinkleAmount ("Twinkle amount", Range(0, 1)) = 0.6
        _StarHorizonFade ("Fade stars near horizon", Range(0.01, 0.6)) = 0.25

        [Header(Moon)]
        _MoonDirection ("Moon direction", Vector) = (0.35, 0.5, -0.8, 0)
        _MoonColor ("Moon colour", Color) = (0.9, 0.93, 1, 1)
        _MoonSize ("Moon size (radians)", Range(0.005, 0.1)) = 0.035
        _MoonGlow ("Moon glow strength", Range(0, 1)) = 0.25
        _MoonGlowSize ("Moon glow size", Range(1, 15)) = 6
    }

    SubShader
    {
        // "Background" queue + no depth writing: the sky is drawn behind everything else.
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _ZenithColor, _HorizonColor, _GroundColor;
            float _GradientCurve;
            float _StarDensity, _StarAmount, _StarSize, _StarBrightness;
            float _TwinkleSpeed, _TwinkleAmount, _StarHorizonFade;
            float4 _MoonDirection;
            fixed4 _MoonColor;
            float _MoonSize, _MoonGlow, _MoonGlowSize;

            struct appdata { float4 vertex : POSITION; };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;   // direction from the camera into the sky
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // The skybox is a big sphere/cube around the camera, so each vertex's
                // position is also the direction we're looking in.
                o.dir = v.vertex.xyz;
                return o;
            }

            // Cheap pseudo-random numbers from a 3D position (same input = same output).
            // "Hash without sine" by Dave Hoskins (shadertoy.com/view/4djSRW, MIT).
            float hash13(float3 p3)
            {
                p3 = frac(p3 * 0.1031);
                p3 += dot(p3, p3.zyx + 31.32);
                return frac((p3.x + p3.y) * p3.z);
            }
            float3 hash33(float3 p3)
            {
                p3 = frac(p3 * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yxz + 33.33);
                return frac((p3.xxy + p3.yxx) * p3.zyx);
            }

            // Stars: split the sky into a 3D grid of cells. Some cells (chosen at random)
            // contain one star at a random spot; we draw a small dot if this pixel is near it.
            float3 Stars(float3 dir)
            {
                float3 p = dir * _StarDensity;
                float3 cell = floor(p);
                float3 local = frac(p);          // position inside the cell, 0..1

                float h = hash13(cell);
                if (h > _StarAmount) return 0;   // no star in this cell

                float3 starPos = hash33(cell) * 0.6 + 0.2;   // keep star away from cell edges
                float d = length(local - starPos);
                float dot_ = 1 - smoothstep(_StarSize * 0.5, _StarSize, d);

                // Each star twinkles at its own timing, and has its own brightness and tint.
                float twinkle = 1 - _TwinkleAmount * (0.5 + 0.5 * sin(_Time.y * _TwinkleSpeed + h * 300));
                float brightness = lerp(0.35, 1.0, frac(h * 97.31));
                float3 tint = lerp(float3(0.75, 0.82, 1.0), float3(1.0, 0.92, 0.8), frac(h * 13.7));

                return tint * dot_ * twinkle * brightness * _StarBrightness;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float up = dir.y;   // 1 = straight up, 0 = horizon, -1 = straight down

                // 1. Gradient: horizon colour blending into the dark top of the sky.
                float3 col = (up >= 0)
                    ? lerp(_HorizonColor.rgb, _ZenithColor.rgb, pow(saturate(up), _GradientCurve))
                    : lerp(_HorizonColor.rgb, _GroundColor.rgb, saturate(-up * 4));

                // 2. Moon: a disc around the moon direction, plus a soft glow.
                float3 moonDir = normalize(_MoonDirection.xyz);
                float angle = acos(clamp(dot(dir, moonDir), -1, 1));   // angle from the moon, in radians
                float disc = 1 - smoothstep(_MoonSize * 0.85, _MoonSize, angle);
                float glow = _MoonGlow * pow(saturate(1 - angle / (_MoonSize * _MoonGlowSize)), 2);

                // 3. Stars: faded out near the horizon and hidden behind the moon.
                float starFade = smoothstep(0, _StarHorizonFade, up) * (1 - disc);
                col += Stars(dir) * starFade;

                col += _MoonColor.rgb * glow;
                col = lerp(col, _MoonColor.rgb, disc);

                return fixed4(col, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
