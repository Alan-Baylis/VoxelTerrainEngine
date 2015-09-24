using UnityEngine;
using System.Collections;
namespace VoxelEngine{
public static class MeshFactory  {
	public static float frequency;
	public static float amplitude;
	public static int oct;
	public static float Gfrequency;
	public static float Gamplitude;
	public static int Goct;
	public static float Cavefrequency;
	public static float Caveamplitude;
	public static int Caveoct;
	public static Vector3[,,] m_normals;
	public static Vector3[,,] m_normals2;
	public static PerlinNoise surfacePerlin;
	public static PerlinNoise cavePerlin;
	public static MarchingCubes marchingCubes;
	public static MarchingCubes marchingCubesVoxels;
		public static float m_surfaceLevel;
		public static bool makecaves;
	public static int[,] m_sampler = new int[,] 
	{
		{1,-1,0}, {1,-1,1}, {0,-1,1}, {-1,-1,1}, {-1,-1,0}, {-1,-1,-1}, {0,-1,-1}, {1,-1,-1}, {0,-1,0},
		{1,0,0}, {1,0,1}, {0,0,1}, {-1,0,1}, {-1,0,0}, {-1,0,-1}, {0,0,-1}, {1,0,-1}, {0,0,0},
		{1,1,0}, {1,1,1}, {0,1,1}, {-1,1,1}, {-1,1,0}, {-1,1,-1}, {0,1,-1}, {1,1,-1}, {0,1,0}
	};
	// Use this for initialization

	public static Vector3[] Createvertices(byte[,,] m_voxels,VoxelChunk chunk){

		Vector3[] vertices = marchingCubes.CreateVertices(m_voxels,chunk,2,2);
		return vertices;

	}
		public static Vector3[] CreateverticesVoxels(byte[,,] m_voxels,VoxelChunk chunk){

		Vector3[] vertices = marchingCubesVoxels.CreateVertices(m_voxels,chunk,2,2);
		return vertices;
		
	}




	// Update is called once per frame
		public static float SampleMountains(float x, float z, PerlinNoise perlin)
	{
			float w = cavePerlin.FractalNoise2D(x , z ,oct,frequency,amplitude);
		//This creates the noise used for the mountains. It used something called 
		//domain warping. Domain warping is basically offseting the position used for the noise by
		//another noise value. It tends to create a warped effect that looks nice.
		//Clamp noise to 0 so mountains only occur where there is a positive value
		//The last value (32.0f) is the amp that defines (roughly) the maximum mountaion height
		//Change this to create high/lower mountains

			return Mathf.Min(0.0f, perlin.FractalNoise2D(x +w, z +w,oct,frequency,amplitude) );
	}
	
		public static float SampleGround(float x,float z, PerlinNoise perlin)
	{
		//This creates the noise used for the ground.
		//The last value (8.0f) is the amp that defines (roughly) the maximum 
			float w = cavePerlin.FractalNoise2D(x , z ,1,Gfrequency,Gamplitude);		//and minimum vaule the ground varies from the surface level
		return perlin.FractalNoise2D(x+w, z+w,oct,Gfrequency,Gamplitude);
	}
	//not cave noise just normal noise now as it needed a noise with another seed
	public static float SampleCaves(float x, float z, PerlinNoise perlin)
	{
			float w = perlin.FractalNoise2D(x , z ,1,Gfrequency,Gamplitude);
		//larger caves (A higher frequency will also create larger caves). It is unitless, 1 != 1m
		return Mathf.Abs(perlin.FractalNoise2D(x+w, z+w,Caveoct,Cavefrequency,Caveamplitude));
		
	}
		//sample caves using simplex noise
		public static float SampleCavesreal(float x,float y, float z)
		{
			//The creates the noise used for the caves. It uses domain warping like the moiuntains
			//to creat long twisting caves.
			//The last vaule is the cave amp and defines the maximum cave diameter. A larger value will create
			float w = SimplexNoise.Noise.Generate(x /28,y/28, z/28);
			//larger caves (A higher frequency will also create larger caves). It is unitless, 1 != 1m
			return SimplexNoise.Noise.Generate(x/550+w,y/650+w, z/550+w)*32;
			
		}

		//commented out as it no longer works
	/*public float [,,] SmoothVoxels(float [,,]m_voxels)
	{
		//float startTime = Time.realtimeSinceStartup;

		//This averages a voxel with all its neighbours. Its is a optional step
		//but I think it looks nicer. You might what to do a fancier smoothing step
		//like a gaussian blur
		int w = m_voxels.GetLength(0);
		int h= m_voxels.GetLength(1);
		int l = m_voxels.GetLength(2);

		
		float[,,] smothedVoxels = new float[w,h,l];
		
		for(int x = 1; x < w-1; x++)
		{
			for(int y = 1; y < h-1; y++)
			{
				for(int z = 1; z < l-1; z++)
				{
					float ht = 0.0f;
					
					for(int i = 0; i < 27; i++)
						ht += m_voxels[x + m_sampler[i,0], y + m_sampler[i,1], z + m_sampler[i,2]];

					smothedVoxels[x,y,z] = ht/27.0f;
				}
			}
		}
		


		return smothedVoxels;
		//Debug.Log("Smooth voxels time = " + (Time.realtimeSinceStartup-startTime).ToString() );
	}*/
		//calculate normals for the mesh 
		public static Vector3[] CalculateNormals(byte[,,] m_voxels ,int size,Vector3 [] verts)
	{	

		Vector3[] normals = new Vector3[size];
		//float startTime = Time.realtimeSinceStartup;
		int w = m_voxels.GetLength(0);
		int h= m_voxels.GetLength(1);
		int l = m_voxels.GetLength(2);
		//This calculates the normal of each voxel. If you have a 3d array of data
		//the normal is the derivitive of the x, y and z axis.
		//Normally you need to flip the normal (*-1) but it is not needed in this case.
		//If you dont call this function the normals that Unity generates for a mesh are used.
		
		
		
		 m_normals = new Vector3[w,h,l];
		
		for(int x = 2; x < w-2; x++)
		{
			for(int y = 2; y < h-2; y++)
			{
				for(int z = 2; z < l-2; z++)
				{
						float dx = m_voxels[x+1,y,z] - m_voxels[x-1,y,z];
						float dy = m_voxels[x,y+1,z] - m_voxels[x,y-1,z];
						float dz =m_voxels[x,y,z+1] - m_voxels[x,y,z-1];
					
					m_normals[x,y,z] = Vector3.Normalize(new Vector3(dx,dy,dz));
				}
			}
		}
		for(int i = 0;i < size ;i++){
				normals[i] = MeshFactory.TriLinearInterpNormal(verts[i]);
		}
		//Debug.Log("Calculate normals time = " + (Time.realtimeSinceStartup-startTime).ToString() );
		//CalculateNormals().Abort();
		
		return normals;
		
	}
		//interpolate normals so normals are smoothed

	public static Vector3 TriLinearInterpNormal(Vector3 pos)
	{			

		int x = (int)pos.x;
		int y = (int)pos.y;
		int z = (int)pos.z;
		
		float fx = pos.x-x;
		float fy = pos.y-y;
		float fz = pos.z-z;
		
		Vector3 x0 = m_normals[x,y,z] * (1.0f-fx) + m_normals[x+1,y,z] * fx;
		Vector3 x1 = m_normals[x,y,z+1] * (1.0f-fx) + m_normals[x+1,y,z+1] * fx;
		
		Vector3 x2 = m_normals[x,y+1,z] * (1.0f-fx) + m_normals[x+1,y+1,z] * fx;
		Vector3 x3 = m_normals[x,y+1,z+1] * (1.0f-fx) + m_normals[x+1,y+1,z+1] * fx;
		
		Vector3 z0 = x0 * (1.0f-fz) + x1 * fz;
		Vector3 z1 = x2 * (1.0f-fz) + x3 * fz;
		
		return z0 * (1.0f-fy) + z1 * fy;
	}
		public static Vector3[] CalculateNormalsRemove(byte[,,] m_voxels ,int size,Vector3 [] verts)
	{	

		Vector3[] normals = new Vector3[size];
		//float startTime = Time.realtimeSinceStartup;
		int w = m_voxels.GetLength(0);
		int h= m_voxels.GetLength(1);
		int l = m_voxels.GetLength(2);

		//This calculates the normal of each voxel. If you have a 3d array of data
		//the normal is the derivitive of the x, y and z axis.
		//Normally you need to flip the normal (*-1) but it is not needed in this case.
		//If you dont call this function the normals that Unity generates for a mesh are used.
		
		
		
		m_normals2 = new Vector3[w,h,l];
		
		for(int x = 2; x < w-2; x++)
		{
			for(int y = 2; y < h-2; y++)
			{
				for(int z = 2; z < l-2; z++)
				{
						float dx = m_voxels[x+1,y,z] - m_voxels[x-1,y,z];
						float dy = m_voxels[x,y+1,z] - m_voxels[x,y-1,z];
						float dz = m_voxels[x,y,z+1] - m_voxels[x,y,z-1];
					
					m_normals2[x,y,z] = Vector3.Normalize(new Vector3(dx,dy,dz));
				}
			}
		}
		for(int i = 0;i < size ;i++){
			normals[i] = MeshFactory.TriLinearInterp(verts[i]);
		}
		//Debug.Log("Calculate normals time = " + (Time.realtimeSinceStartup-startTime).ToString() );
		
		return normals;
		
	}
		//interpolate normals so normals are smoothed
		//this function is aduplicate to prevent normals being confused with mesh creation normals 
		//while editing the terrain
	public static Vector3 TriLinearInterp(Vector3 pos)
	{	
		int x = (int)pos.x;
		int y = (int)pos.y;
		int z = (int)pos.z;
		
		float fx = pos.x-x;
		float fy = pos.y-y;
		float fz = pos.z-z;
		
		Vector3 x0 = m_normals2[x,y,z] * (1.0f-fx) + m_normals2[x+1,y,z] * fx;
		Vector3 x1 = m_normals2[x,y,z+1] * (1.0f-fx) + m_normals2[x+1,y,z+1] * fx;
		
		Vector3 x2 = m_normals2[x,y+1,z] * (1.0f-fx) + m_normals2[x+1,y+1,z] * fx;
		Vector3 x3 = m_normals2[x,y+1,z+1] * (1.0f-fx) + m_normals2[x+1,y+1,z+1] * fx;
		
		Vector3 z0 = x0 * (1.0f-fz) + x1 * fz;
		Vector3 z1 = x2 * (1.0f-fz) + x3 * fz;
		
		return z0 * (1.0f-fy) + z1 * fy;
	}
		//doesnt work
	public static byte[,,] SmoothVoxels(byte[,,] m_voxels)
	{
		//float startTime = Time.realtimeSinceStartup;
		
		//This averages a voxel with all its neighbours. Its is a optional step
		//but I think it looks nicer. You might what to do a fancier smoothing step
		//like a gaussian blur
		
		int w = m_voxels.GetLength(0);
		int h = m_voxels.GetLength(1);
		int l = m_voxels.GetLength(2);
		
		byte[,,] smothedVoxels = new byte[w,h,l];
		
		for(int x = 1; x < w-1; x++)
		{
			for(int y = 1; y < h-1; y++)
			{
				for(int z = 1; z < l-1; z++)
				{
					byte ht = 0;
					
					for(int i = 0; i < 27; i++)
						ht += m_voxels[x + m_sampler[i,0], y + m_sampler[i,1], z + m_sampler[i,2]];
					smothedVoxels[x,y,z] = (byte)((int)ht/27);
				}
			}
		}
		Debug.Log("donesmoothing");

		return smothedVoxels;
		
		//Debug.Log("Smooth voxels time = " + (Time.realtimeSinceStartup-startTime).ToString() );
	}


		//function to create the voxel noises and caves etc.
		public static byte[,,]CreateVoxels(byte[,,] m_voxels,Vector3 m_pos,VoxelChunk chunk)
	{
		//float startTime = Time.realtimeSinceStartup;
		
		//Creates the data the mesh is created form. Fills m_voxels with values between -1 and 1 where
		//-1 is a soild voxel and 1 is a empty voxel.
		int w = m_voxels.GetLength(0);
		int h= m_voxels.GetLength(1);
		int l = m_voxels.GetLength(2);
			float caveht =0;
			float ht =1;
			float mountainHt = 0;
			float groundHt = 0;
			float HT = 0;
			float cv =0;
			float fade = 0;
			for(int x = 0; x < w; x++)
			{
			for(int z = 0; z < l; z++)
			{
			//world pos is the voxels position plus the voxel chunks position
				float worldX = x+m_pos.x;
				float worldZ = z+m_pos.z;
				
				


					caveht =0;
					ht =1;
					mountainHt = MeshFactory.SampleMountains(worldX ,worldZ, surfacePerlin);
					groundHt = MeshFactory.SampleGround(worldX ,worldZ, surfacePerlin);

				for(int y = 0; y < h; y++)
				{

					float worldY = y+m_pos.y;
						if(y<h/4)
							caveht=SampleCaves(worldX,worldZ,cavePerlin);

						if(y > h/groundHt)ht =  mountainHt;
						else if(y > h/groundHt)ht +=  groundHt+mountainHt;
						ht+=groundHt;
						ht-=groundHt/2;


					//If we take the heigth value and add the world
					//the voxels will change from positiove to negative where the surface cuts through the voxel chunk

						HT = ht +worldY-m_surfaceLevel;

						HT-=caveht;
						fade = 1f - Mathf.Clamp01(Mathf.Max(m_surfaceLevel, worldY)/64f);
							if(makecaves &&y<h/2-50){
						cv =SampleCavesreal(worldX ,worldY,worldZ);
							//cv+=newPerlin.GetGradient3D().;
							HT+=cv*fade;
							}
							HT/=16;
					HT = Mathf.Clamp(HT , -0.5f, 0.5f);
					HT+=0.5f;
					HT*=255;

						m_voxels[x,y,z]=(byte)HT;

						m_voxels[x,y,z]=(byte)Mathf.Clamp(m_voxels[x,y,z],0,255);
						if(y>=h-5&&m_voxels[x,y,z]<=127)m_voxels[x,y,z]=255;
						if(y<=5&&m_voxels[x,y,z]>=127)m_voxels[x,y,z]=0;
					
					}
			}
		}
		
		return m_voxels;
		
		
	}
}
}