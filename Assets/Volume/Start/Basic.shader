Shader "CustomRenderTexture/Basic"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _StepSize ("Step Size", Range(0, 1)) = 0.1
        _MinDistanceToSurface ( "Min Distance To Surface", Float) = 0.01
     }

     SubShader
     {
        Tags { "RenderType"="Opaque" }

        Pass
        {

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex: POSITION;
            };

            struct v2f
            {
                float4 vertex: SV_POSITION;
                float3 worldPos: TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float _StepSize;
            float _MinDistanceToSurface;
            fixed4 _Color;

            float rayBox(float boundsMin, float3 boundsMax, float3 camPos, float3 viewDir)
            {
                float3 invViewDir = 1 / viewDir;
                float3 t0 = (boundsMin - camPos) * invViewDir;
                float3 t1 = (boundsMax - camPos) * invViewDir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                float dst0 = max(max(tmin.x, tmin.y), tmin.z);
                float dst1 = min(min(tmax.x, tmax.y), tmax.z);
                float dstToBound = max(0, dst0);
                float dstInsideBox = max(0, dst1 - dstToBound);
                return float2(dstToBound, dstInsideBox);
            }

            float GetSphere(float3 p)
            {
                float d = length(p) - 0.5;
                return d;
            }

            float GetRing(float3 p)
            {
                float d = length(float2(length(p.xz) - 0.3, p.y)) - 0.1;
                return d;
            }


            fixed4 frag(v2f i): SV_Target
            {
                float3 camPos = _WorldSpaceCameraPos;
                float3 viewDir = normalize(i.worldPos - camPos);
                float3 center = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
                float3 size = float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].z);
                float3 boundsMin = center - size * 0.5f;
                float3 boundsMax = center + size * 0.5f;
                float2 rayToBox=rayBox(boundsMin, boundsMax, camPos, viewDir);
                if (rayToBox.x < _ProjectionParams.y || rayToBox.x > _ProjectionParams.z)
                    discard;
                float3 startPos = camPos + viewDir * rayToBox.x;
                for (float stepLength = 0; stepLength < rayToBox.y; stepLength += _StepSize)
                {
                    float3 currentPos = startPos + viewDir * stepLength;
                    float3 localPos = currentPos - center;
                    if(GetRing(localPos) < _MinDistanceToSurface)
                    {
                        return _Color;
                    }
                }
                discard;

                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
