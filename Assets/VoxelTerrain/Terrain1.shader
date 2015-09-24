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
		struct Input 
		{
			float3 worldPos;
			fixed3 worldNormal;
			float4 color : COLOR;
			//INTERNAL_DATA
		};
		
		fixed3 TriplanarSample(sampler2D tex, fixed3 worldPosition, fixed3 projNormal, float scale)
		{
			fixed3 cZY = tex2D(tex, worldPosition.zy * scale);
			fixed3 cXZ = tex2D(tex, worldPosition.xz * scale);
			fixed3 cXY = tex2D(tex, worldPosition.xy * scale);
			
			cXY = lerp(cXY, cXZ, projNormal.y);
			return lerp(cXY, cZY, projNormal.x);
		}
		

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float3 projNormal = saturate(pow(IN.worldNormal * 1.5, 4));
			
			fixed3 sand = TriplanarSample(_SandTex, IN.worldPos, projNormal, 1.0);
			
			fixed3 gravel = TriplanarSample(_GravelTex, IN.worldPos, projNormal, 1.0);
			
			fixed3 rock = TriplanarSample(_RockTex, IN.worldPos, projNormal, 0.1);

			fixed3 cliff = TriplanarSample(_CliffTex, IN.worldPos, projNormal, 1.0);
			
			fixed3 Iron = TriplanarSample(_IronTex, IN.worldPos, projNormal, 1.0);
			
			fixed3 Gold = TriplanarSample(_GoldTex, IN.worldPos, projNormal, 1.0);
			
			fixed3 Gunpowder = TriplanarSample(_GunPowderTex, IN.worldPos, projNormal, 0.1);

			fixed3 Tungsten = TriplanarSample(_TungstenTex, IN.worldPos, projNormal, 1.0);
			
			
			

                 
			float4 controlMap = IN.color;
			fixed3 col= 0;
			
			if(controlMap.r<0.9f&&controlMap.r>0.0f)
			col = lerp(col, rock, controlMap.r);
			
			if(controlMap.g<0.9f&&controlMap.g>0.0f)
			col = lerp(col,sand , controlMap.g);
			
			
			
			if(controlMap.b<0.9f&&controlMap.b>0.0f)
			col = lerp(col, gravel, controlMap.b);
			
		
			
			if(controlMap.a<0.9f&&controlMap.a>0.0f)
			col = lerp(col, cliff, controlMap.a);
			
			if(controlMap.r>0.5f)
			col = lerp(col, Iron, controlMap.r);
			
			
			//if(controlMap.g>0.5f)
			//col = lerp(col, Gold, controlMap.g);
			
				if(controlMap.b>0.5f)
			col = lerp(col, Gunpowder, controlMap.b);
			
			if(controlMap.a>0.5f)
			col = lerp(col, Tungsten , controlMap.a);
			

			
			
			
			
						

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
