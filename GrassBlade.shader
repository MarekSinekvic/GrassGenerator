Shader "Custom/GrassBlade"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Blade texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _AreaSize ("Grass area size", Vector) = (0,0,0,0)

        _NoiseTex ("Noise texture", 2D) = "black" {}
        _NoiseFreq ("Noise frequency", Float) = 1

        _WavingIntens ("Waving intensity", Float) = 0
        _WavingSpeed ("Waving speed", Vector) = (0,0,0,0)
        _MaxNoiseDistortion ("Max noise distortion", Float) = 1
        _WavingPower ("Waving power", Float) = 1

        _AreaClr ("Area color", Color) = (0,0,0,0)
        _AreaClrIntens ("Area coloring intensity", Float) = 1
        _AreaClrPower ("Area coloring power", Float) = 1
        _AreaClrScale ("Area color scale", Float) = 1
        _AreaTex ("Area texture", 2D) = "black" {}

        _NormalIntens ("Normal intensity", Float) = 20

        // _DistortionTex ("Distortion texture", 2D) = "black" {}
        // _DistortionMaxOffset ("Distortion max offset", Float) = 0
        // _DistortionIntens ("Distortion intens", Float) = 0
        // _DistortionScale ("Distortion scale", Float) = 0
        
        [Toggle] _DebugView ("Turn debug view", Int) = 0
        _DebugIntens ("Debug color intensity", Float) = 0
        _DebugPower ("Debug color power", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CULL off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 vertPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float4 _AreaSize;
        float4 _AreaClr;
        float _AreaClrIntens, _AreaClrPower,_AreaClrScale;

        sampler2D _NoiseTex,_AreaTex;
        float4 _NoiseTex_TexelSize;

        float _NoiseFreq;
        float _MaxNoiseDistortion,_WavingPower;

        float _WavingIntens;
        float2 _WavingSpeed;

        sampler2D _DistortionTex;
        float4 _DistortionTex_TexelSize;
        float _DistortionIntens, _DistortionScale, _DistortionMaxOffset;

        float _NormalIntens;

        int _DebugView;
        float _DebugIntens,_DebugPower;

        const float E = 2.7182;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float getNoise(float2 uv) {
            return tex2Dlod(_NoiseTex,float4(uv+_WavingSpeed*_Time.x,0,0)*_NoiseFreq);
        }
        float getDistortion(float2 uv) {
            return tex2Dlod(_DistortionTex,float4(uv,0,0));
        }
        float smoothClamp(float v, float m1, float m2) {
            m1 = abs(m1);
            m2 = abs(m2);
            if (v > 0) {
                return m1*(-pow(2,-v/m1)+1);
            } else {
                return m2*(pow(2,v/m2)-1);
            }
        }

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            float2 uv = v.vertex.xz/_AreaSize.xz;
            float c = getNoise(uv);
            c = smoothClamp(c,-_MaxNoiseDistortion,_MaxNoiseDistortion);
            // c = pow(abs(c),_WavingPower);
            float2 offset = normalize(_WavingSpeed)*c*_WavingIntens*pow(v.vertex.y,1.5);
            v.vertex.xz += offset;

            v.normal = normalize(v.normal+float3(0,dot(float2(v.normal.xz),offset)*_NormalIntens,0));
            // v.normal = float3(0,1,0);
            
            // float cr = getNoise(uv+float2(_NoiseTex_TexelSize.x,0));
            // float cu = getNoise(uv+float2(0,_NoiseTex_TexelSize.y));
            
            // float2 delta = float2(cr-c,cu-c);
            // float d = length(delta);
            // d = smoothClamp(d*_WavingIntens,-_MaxNoiseDistortion,_MaxNoiseDistortion);

            // float2 offset = normalize(delta)*d*pow(v.vertex.y,1.5);
            // v.vertex.xz += offset;
            
            // v.normal = normalize(v.normal+float3(0,length(offset)*_NormalIntens,0));

            o.vertPos = v.vertex;

            // float d = getDistortion(uv);
            // float dr = getDistortion(uv+float2(_DistortionTex_TexelSize.x,0));
            // float du = getDistortion(uv+float2(0,_DistortionTex_TexelSize.y));
            
            // float2 distort = float2(dr-d,du-d);
            // float dst = length(distort);
            // if (dst < _DistortionMaxOffset) dst = _DistortionMaxOffset;
            // if (dst < 0.01) return;
            // v.vertex.xz += -normalize(distort)/dst * _DistortionIntens * v.vertex.y;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            fixed4 bladeColor = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            float4 areaColor = tex2D(_AreaTex, (IN.vertPos.xz / _AreaSize.xz*2+float2(1,1))/2) * _AreaClr;
            o.Albedo = ((bladeColor.rgb+1/pow(areaColor*_AreaClrScale,_AreaClrPower)*_AreaClrIntens)/2) * lerp(pow(abs(uv.x-0.5)*2,0.5)+0.7,1,pow(uv.y,0.3));
            if (_DebugView) {
                float2 uv = IN.vertPos.xz/_AreaSize.xz;
                float c = getNoise(uv);
                float cr = getNoise(uv+float2(_NoiseTex_TexelSize.x,0));
                float cu = getNoise(uv+float2(0,_NoiseTex_TexelSize.y));

                float2 delta = (float2(cr-c,cu-c));

                o.Albedo = normalize(float3(delta.x,0,delta.y))*_DebugIntens;
                o.Albedo = float3(c,c,c)*_DebugIntens;
            }
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = bladeColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
