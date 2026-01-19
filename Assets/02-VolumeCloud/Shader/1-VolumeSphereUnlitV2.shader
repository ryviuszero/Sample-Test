Shader "Unlit/1-VolumeSphereUnlit"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Intensity("Intensity", Range(0, 1)) = 0.1
        _Loop("Loop", Range(0, 128)) = 32
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }

        Pass
        {
            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Lighting Off    //不参与光照

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos: TEXCOORD0;
            };

            float4 _Color;
            float _Intensity;
            int _Loop;

            inline float densityFunction(float3 p)
            {
                return 0.5 - length(p);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldPos = i.worldPos;
                float3 worldDir = normalize(worldPos - _WorldSpaceCameraPos);

                // 射线在物体局部坐标中走动
                float3 localPos = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;
                float3 localDir = UnityWorldToObjectDir(worldDir);

                // 射线步进
                float step = 1.0 / _Loop;
                float3 localStep = localDir * step;

                // 通过光线步进计算透明度
                float alpha = 0.0;
                for (int i = 0; i < _Loop; i++)
                {
                    // localPos是模型空间与模型中心的距离
                    float density = densityFunction(localPos);
                    if (density > 0.001)
                    {
                        alpha += (1.0 - alpha) * density * _Intensity;
                    }
                    // 每一步累加该点的密度贡献，形成体积感
                    localPos += localStep;
                    // 走出模型边界就立马终止
                    if(all(abs(localPos) >= 0.5)) break;
                }
                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}
