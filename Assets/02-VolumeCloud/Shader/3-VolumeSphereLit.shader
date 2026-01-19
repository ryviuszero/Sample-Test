Shader "Unlit/1-VolumeSphereUnlit"
{
    Properties
    {
        [Header(Base)]
        [Space(10)]
        _Color("Color", Color) = (1,1,1,1)
        _Absorption("Absorption", Range(0, 100)) = 50  // 主视方向的体积吸收系数
        _Opacity("Opacity", Range(0, 100)) = 50         // 不透明度
        [IntRange] _Loop("Loop", Range(0, 128)) = 32    // 射线步进次数

        [Header(Light)]
        [Space(10)]
        _AbsorptionLight("AbsorptionLight", Range(0, 100)) = 50 // 光照方向的体积吸收系数
        _OpacityLight("OpacityLight", Range(0, 100)) = 50       // 光照不透明度
        [IntRange] _LoopLight("LoopLight", Range(0, 128)) = 6    // 光照射线步进次数
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
            float _Absorption;
            float _Opacity;
            int _Loop;
            float _AbsorptionLight;
            float _OpacityLight;
            int _LoopLight;
            float4 _LightColor0;

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
                float step = 1.0 / _Loop;
                float3 worldPos = i.worldPos;
                float3 worldDir = normalize(worldPos - _WorldSpaceCameraPos);

                // 模型空间 : 视角方向做rayMarch
                float3 localPos = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;
                float3 localDir = UnityWorldToObjectDir(worldDir);
                float3 localStep = localDir * step;

                // 光线方向也做 rayMarch
                float lightStep = 1.0 / _LoopLight;
                float3 localLightDir = UnityWorldToObjectDir(_WorldSpaceLightPos0.xyz);
                float3 localLightStep = localLightDir * lightStep * 0.5;  // 0.5 缩小步进

                // 初始化输出值
                float4 color = float4(_Color.rgb, 0.0);
                float transmittance = 1.0; 

                // Raymarching
                for( int i = 0; i < _Loop; i++)
                {
                    float density = densityFunction(localPos);
                    if (density > 0.0)
                    {
                        // 【主方向体积衰减（吸收模拟）】
                        float d = density * step;
                        // 通过率会被层层递减，如果遮挡过多就被完全挡住，跳出循环
                        transmittance *= 1.0 - d * _Absorption;
                        if (transmittance < 0.01)
                            break;
                        
                        // 【光照方向的体积穿透（次级RayMarching）, 制造内部阴影】
                        float transmittanceLight = 1.0;
                        float3 lightPos = localPos;

                        for(int j = 0; j < _LoopLight; ++ j){
                            float densityLight = densityFunction(lightPos);

                            if(densityLight > 0.0){
                                float dl = densityLight * lightStep;
                                // 光照通过率也会被层层递减
                                transmittanceLight *= 1.0 - dl * _AbsorptionLight;
                                if(transmittanceLight < 0.01){
                                    transmittanceLight = 0.0;
                                    break;
                                }
                            }
                        }
                        lightPos += localLightStep;
                    
                        // 当前点对整体透明度的影响
                        color.a += _Color.a * (_Opacity * d * transmittance);
                        // 光照贡献 = 场景主光颜色 * (亮度强度 * 密度 * 视线通过率 * 光线通过率)
                        color.rgb += _LightColor0 * (_OpacityLight * d *  transmittance * transmittanceLight);
                    }
                    localPos += localStep;
                    // 走出模型边界就立马终止
                    if(all(abs(localPos) >= 0.5)) break;
                }
                color.a = min(color.a, 1.0);
                return color;
            }
            ENDCG
        }
    }
}