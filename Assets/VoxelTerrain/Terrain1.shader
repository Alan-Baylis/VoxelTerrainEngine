Shader "Custom/Terrain" 
{
	Properties 
	{	_CliffTex("Grass", 2D) = "white" {}
		_SandTex("CLiff", 2D) = "white" {}
		_GravelTex("GravelTex", 2D) = "white" {}
		_RockTex("RockTex", 2D) = "white" {}
		
		_IronTex("IronTex", 2D) = "white" {}
		_GoldTex("GoldTex", 2D) = "white" {}
		_GunPowderTex("GunPowderTex", 2D) = "white" {}
		_TungstenTex("TungstenTex", 2D) = "white" {}
		_Blending ("Blending" , Range( 0.0001 ,0.5 ))= 0.5
		_FBlending ("Fuzzy Blending" , Range( 0.0001 ,0.25 ))= 0.5
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		//Cull off
				

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _SandTex, _GravelTex, _RockTex,_CliffTex,_IronTex,_GoldTex,_GunPowderTex,_TungstenTex;
		float _Blending;
		float _FBlending;
		struct Input 
		{
			float3 worldPos;
			fixed3 worldNormal;
			float4 color : COLOR;
			//INTERNAL_DATA
		};
		
	
		fixed4 TriplanarSample(sampler2D tex, fixed3 worldPosition, fixed3 projNormal, float scale)
		{
			fixed4 cZY = tex2D(tex, worldPosition.zy * scale);
			fixed4 cXZ = tex2D(tex, worldPosition.xz * scale);
			fixed4 cXY = tex2D(tex, worldPosition.xy * scale);
			
			cXY = lerp(cXY, cXZ, projNormal.y);
			return lerp(cXY, cZY, projNormal.x);
		}
		
		
		
		float4 blend(float4 texture1, float a1, float4 texture2, float a2)
		{
	
    	float depth = _Blending;
    	float ma = max(texture1.a * a1, texture2.a * a2) - depth;

    	float b1 = max(texture1.a * a1 - ma, 0);
    	float b2 = max(texture2.a * a2 - ma, 0);

    	return ((texture1.rgba * b1) + (texture2.rgba * b2)) / (b1 + b2);
		}
		
		
		
		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float3 projNormal = saturate(pow(IN.worldNormal * 1.5, 4));
			
			float4 sand = TriplanarSample(_SandTex, IN.worldPos, projNormal, 1.0);
			
			float4 gravel = TriplanarSample(_GravelTex, IN.worldPos, projNormal, 1.0);
			
			float4 rock = TriplanarSample(_RockTex, IN.worldPos, projNormal, 0.1);
			
			float4 cliff = TriplanarSample(_CliffTex, IN.worldPos, projNormal, 1.0);
			
			float4 Iron = TriplanarSample(_IronTex, IN.worldPos, projNormal, 1.0);
			
			float4 Gold = TriplanarSample(_GoldTex, IN.worldPos, projNormal, 1.0);
			
			float4 Gunpowder = TriplanarSample(_GunPowderTex, IN.worldPos, projNormal, 0.1);

			float4 Tungsten = TriplanarSample(_TungstenTex, IN.worldPos, projNormal, 1.0);
		
			float4 controlMap = IN.color;
			float4 col= 0;
			
			
			
			if(controlMap.g<0.71f&&controlMap.g>_FBlending)
			col =	lerp(col,blend(sand,controlMap.g*2,sand , controlMap.g*2),controlMap.g*2);
			
			if(controlMap.r<0.71f&&controlMap.r>_FBlending)
			col = lerp(col,blend(col,controlMap.r*2, rock,controlMap.r*2 ),controlMap.r*2);
			
			if(controlMap.b<0.71f&&controlMap.b>_FBlending)
			col = lerp(col,blend(col,controlMap.b*2, gravel, controlMap.b*2),controlMap.b*2);
			
		
			
			if(controlMap.a<0.71f&&controlMap.a>_FBlending)
			col = lerp(col,blend(col,controlMap.a*2, cliff,controlMap.a*2), controlMap.a*2);
			
			if(controlMap.r>0.5f)
			col = lerp(col,blend(col,controlMap.r, Iron, controlMap.r),controlMap.a);
			
			
			if(controlMap.g>0.5f)
			col = lerp(col,blend(col,controlMap.g, Gold, controlMap.g),controlMap.g);
			
				if(controlMap.b>0.5f)
			col = lerp(col,blend(col,controlMap.b, Gunpowder, controlMap.b),controlMap.b);
			
			if(controlMap.a>0.5f)
			col = lerp(col,blend(col,controlMap.a ,Tungsten , controlMap.a),controlMap.a);
			

			
			
			
			
						

			o.Metallic = 0;
            o.Smoothness = 0;
			//o.Albedo = controlMap;  
			o.Albedo = col;
			//o.Alpha = 1.0;
			//o.Normal = normalize(coln *0.1);
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
