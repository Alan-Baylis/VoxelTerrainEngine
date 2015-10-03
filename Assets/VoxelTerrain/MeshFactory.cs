using UnityEngine;
using System.Collections;
namespace VoxelEngine{
public static class MeshFactory  {
	public static VoxelTerrainEngine generator;
	public static Vector3[,,] m_normals;
	public static Vector3[,,] m_normals2;
	public static PerlinNoise SurfacePerlin;
	public static PerlinNoise CavePerlin;
	public static MarchingCubes MarchingCubes;
	public static MarchingCubes MarchingCubesVoxels;
	public static float m_surfaceLevel;
	public static bool MakeCaves;

	public static Vector3[] Createvertices(byte[,,] m_voxels,VoxelChunk chunk){

		Vector3[] vertices = MarchingCubes.CreateVertices(m_voxels,chunk,2,2);
		return vertices;

	}
		public static Vector3[] CreateverticesVoxels(byte[,,] m_voxels,VoxelChunk chunk){

		Vector3[] vertices = MarchingCubesVoxels.CreateVertices(m_voxels,chunk,2,2);
		return vertices;
		
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

	


		//function to create the voxel noises and caves etc.
	public static byte[,,]CreateVoxels(byte[,,] m_voxels,Vector3 m_pos,VoxelChunk chunk,NoiseModule noiseModule)
	{
		//float startTime = Time.realtimeSinceStartup;
		
		//Creates the data the mesh is created form. Fills m_voxels with values between -1 and 1 where
		//-1 is a soild voxel and 1 is a empty voxel.

		int w = m_voxels.GetLength(0);
		int h= m_voxels.GetLength(1);
		int l = m_voxels.GetLength(2);
		float worldX;
		float worldZ;
		float worldY;
			float ht;
	for(int x = 0; x < w; x++)
			{
	for(int z = 0; z < l; z++)
			{
		
			//world pos is the voxels position plus the voxel chunks position
		worldX = x+m_pos.x;
		worldZ = z+m_pos.z;
	ht = generator.noise.FillVoxel2d(worldX,worldZ,m_pos,SurfacePerlin,CavePerlin);

	for(int y = 0; y < h; y++)
				{
		worldY = y+m_pos.y;
		float HT=ht+worldY-h/2;

	if(MakeCaves&&y<h/2-50)
		HT -= generator.noise.FillVoxel3d(worldX,worldY,worldZ,m_pos);

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