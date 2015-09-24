using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using System.Linq;
using PlayerPrefs = PreviewLabs.SaverVoxel;
using Util.DoubleKeyDictionary;
namespace VoxelEngine{
public class VoxelTerrainEngine: MonoBehaviour 
{
		public Vector3 Position;
		[Range(0.001f,4.0f)]
		public float number;
		[Range(0.001f,90.0f)]
		public float number2;
	public LayerMask mask;
	public Material m_material;
	public int distanceToload;
		[Range(0,200)]
	public float DetailDistance;
	public String grassLayerName;
	public int maxTrees;
	public int maxGrass;
	GameObject m_mesh;
	public bool makecaves;
		public int ChunksY;
	public GameObject []trees;
		public float []treesWeights;
	public GameObject[] grass;
		public float []grassWeights;
	Vector2 Campos;
	[HideInInspector]
	public Transform Parent;
	private DoubleKeyDictionary<int, int, VoxelChunk>m_voxelChunk = new DoubleKeyDictionary<int, int, VoxelChunk>();
	public int m_surfaceSeed = 4;
	public int m_voxelWidth = 32, m_voxelHeight = 128, m_voxelLength = 32;
	public float m_surfaceLevel = 0.0f;
	public float frequency;
	public float amplitude;
	public int oct;
	public float Gfrequency;
	public float Gamplitude;
	public int Goct;
	public float Cavefrequency;
	public float Caveamplitude;
	public int Caveoct;
	PerlinNoise m_surfacePerlin;
	PerlinNoise  m_cavePerlin;
	public Thread thread;
	public Thread thread2;
		public float posx ;
		public float posy ;
		public float posz;
	public static VoxelTerrainEngine generator;
	
	public VoxelChunk chunk;
	[HideInInspector]
	public Vector3 parentpos;
	Vector3 offset;
	Vector3 pos;
	Vector3 playerposGlobal;
	bool canContinue;
	bool cancollect;

	[HideInInspector]
	public Vector3 player;
	public Vector3 playerGlobal;
	public List<VoxelChunk>Todestroy = new List<VoxelChunk>();
	public List<VoxelChunk>ActiveChunks = new List<VoxelChunk>();
	public List<VoxelChunk>requestedChunks = new List<VoxelChunk>();
	public List<VoxelChunk>requestedChunksOrdered = new List<VoxelChunk>();
		public List<VoxelChunk>newvoxelChunk = new List<VoxelChunk>();
	public float dist;
	bool firstTime = true;
	[HideInInspector]
	public int xx;
	[HideInInspector]
	public int yy ;
	[HideInInspector]
	public int zz ;
		public int offsety ;
	public bool cansave;
	Transform target;
		public WindZone zone;
		public Camera cam;
		public bool check;
	public void Start(){

			target = Camera.main.transform;
			cam = Camera.main;

			player = transform.InverseTransformPoint(target.position );
		generator = this;
		Parent = transform;
		VoxelChunk.parent = transform;
		VoxelChunk.generator = generator;
		MeshFactory.frequency = frequency;
		MeshFactory.amplitude = amplitude;
		MeshFactory.oct= oct;
		MeshFactory.Gfrequency = Gfrequency;
		MeshFactory.Gamplitude = Gamplitude;
		MeshFactory.Goct = Goct;
		MeshFactory.Cavefrequency = Cavefrequency;
		MeshFactory.Caveamplitude = Caveamplitude;
		MeshFactory.Caveoct = Caveoct;
			MeshFactory.m_surfaceLevel = m_surfaceLevel;
		m_surfacePerlin = new PerlinNoise(m_surfaceSeed);
		m_cavePerlin = new PerlinNoise(m_surfaceSeed/2);
		MeshFactory.surfacePerlin = m_surfacePerlin;
		MeshFactory.cavePerlin = m_cavePerlin;
		MeshFactory.marchingCubes = new MarchingCubes();
		MeshFactory.marchingCubesVoxels = new MarchingCubes();
			MeshFactory.makecaves = makecaves;
		offset = new Vector3(m_voxelWidth, offsety*m_voxelHeight/2, m_voxelLength);
		parentpos = Parent.position;


		MarchingCubes.SetTarget(126);
		MarchingCubes.SetWindingOrder(0, 1, 2);
		canContinue = true;
		StartCoroutine(startthreads());
			//Wind = new Vector4(zone.windMain,zone.windPulseFrequency,zone.windPulseMagnitude,zone.windTurbulence);
				//Shader.SetGlobalColor("_Wind", Wind);







	}
		//start all threads
public IEnumerator startthreads(){

			thread = new Thread(generateterrains);
			thread.IsBackground=true;
			thread.Start();
			thread2 = new Thread(generateChunks);
			thread2.IsBackground=true;
			thread2.Start();
			yield return new WaitForSeconds(1);


		}
		//raycast for the voxel
		//basically just a normal raycast but with better targetting for the voxels
		public static bool raycastvoxels(Ray ray, out RaycastHit hitinfo,float distance,LayerMask mask ){
			RaycastHit hit;
			if(Physics.Raycast(ray.origin,ray.direction, out hit, distance ,mask )){
				
				hit.point = new Vector3(Mathf.RoundToInt(hit.point.x+(ray.direction.x/2)),Mathf.RoundToInt(hit.point.y+(ray.direction.y/2)),Mathf.RoundToInt(hit.point.z+(ray.direction.z/2)));

				hitinfo = hit;
				return true;
			}
		
			else {	hitinfo = hit;
				return false;}

		}
		//raycast for the voxel
		//basically just a normal raycast but with better targetting for the voxels
		public static bool raycastvoxels(Ray ray, out RaycastHit hitinfo,float distance ){
			RaycastHit hit;
			if(Physics.Raycast(ray.origin,ray.direction, out hit, distance )){
				
				hit.point = new Vector3(Mathf.RoundToInt(hit.point.x+(ray.direction.x/2)),Mathf.RoundToInt(hit.point.y+(ray.direction.y/2)),Mathf.RoundToInt(hit.point.z+(ray.direction.z/2)));
				
				hitinfo = hit;
				return true;
			}
			
			else {	hitinfo = hit;
				return false;}
			
		}
		//old method can be used of needed
	public void removeVoxels(Vector3 hitpoint,byte value){
			int C = ActiveChunks.Count;
			for(int c = 0;c < C;c++){
			if( ActiveChunks[c]!=null){
				ActiveChunks[c].removeVoxels(hitpoint,value);}
			if( ActiveChunks[c]!=null &&ActiveChunks[c].hasvoxel){
				
					newvoxelChunk.Add(ActiveChunks[c]);
			}
			}


	}
		//check if voxel exists and to what chunk it belongs to 
		//just VoxelTerrainEngine.raycastvoxels to raycast for a voxel then check the hit.point against this
		//should return a chunk
		public bool CheckVoxels(Vector3 hitpoint,out VoxelChunk chunk){
			int C = ActiveChunks.Count;
			chunk = null;
			for(int c = 0;c < C;c++){
					if(ActiveChunks[c].CheckVoxels(hitpoint)){
						chunk = ActiveChunks[c];
						
					}



			}
			if(chunk!=null)return true;

			else return false;
			
			
		
		}
		//method for saving changes to terrain at runtime
		public void saveTerrains(){
		for(int i = 0;i < ActiveChunks.Count;i++){

				if(ActiveChunks[i].haschanged && ActiveChunks[i].shouldrender){
			ActiveChunks[i].saveVoxels();
		}


	
		}
	}
		//static method for setting voxel values 
		//can be used for adding or removing voxels
		//once this method is called the chunk its on will be automatically located and set to re create the mesh using using new voxel data
	public static void SetVoxels(Vector3 voxel,byte value){
			generator.removeVoxels(voxel,value);
		
	}

	


	void Update(){
			player = transform.InverseTransformPoint( target.position);
			playerGlobal = target.position;
			RaycastHit hit;
			//set the wind variables on the shader
			Shader.SetGlobalFloat("WindAmount",zone.windMain );
			Shader.SetGlobalFloat("WaveSize" ,zone.windPulseMagnitude);
			Shader.SetGlobalFloat("windSpeed" , zone.windPulseFrequency);
			Shader.SetGlobalFloat("SqrDistance", zone.windTurbulence);
			Shader.SetGlobalFloat("WindDirectionx",zone.transform.forward.x);
			Shader.SetGlobalFloat("WindDirectiony",zone.transform.forward.y);
			Shader.SetGlobalFloat("WindDirectionz",zone.transform.forward.z);

	
			//destroy meshes that are to far away 
		if(Todestroy.Count>0){

				//if we have meshes to destroy destroy them 
				//need to implement a pooling system will save on memory as well as stop the garbage collector from being called
			for(int i = 0; i < Todestroy.Count;i++){
				if(Todestroy[i]!=null){
			
						if(Todestroy[i].mesh!=null && i < Todestroy.Count){
				DestroyImmediate(Todestroy[i].mesh);
				Todestroy[i].mesh=null;
				UnityEngine.Object.DestroyImmediate(Todestroy[i].m_mesh);
				ActiveChunks.Remove(Todestroy[i]);
				}
				
						if(i < Todestroy.Count){
				Todestroy[i].canDraw=false;
				Todestroy[i].canCreatemesh=false;
								Todestroy.Remove(Todestroy[i]);
		}
		}
		}
			}

		

			//render active chunks in view
			if(ActiveChunks.Count>0){

				for(int i = 0; i < ActiveChunks.Count; i++)
				{	

					if(i< ActiveChunks.Count&& ActiveChunks[i]!=null && ActiveChunks[i].canCreatemesh){
						//create the mesh . this is done in update as calling it from another thread is not allowed
						ActiveChunks[i].CreateMesh();
						ActiveChunks[i].canCreatemesh=false;
					
					}
					//small optimisation to only render whats infront of the player 
					//need to some how check for occlusion and not render things behind other objects
					if(i< ActiveChunks.Count && ActiveChunks[i]!=null &&  ActiveChunks[i].canDraw && ActiveChunks[i].shouldrender){
						Position = ActiveChunks[i].mesh.bounds.center+ActiveChunks[i].mytransform.position;

						Vector3 direction = (Position-new Vector3(posx,posy,posz)).normalized;



						//call render on chunk
							ActiveChunks[i].render();
						if(Vector2.Distance(new Vector2(posx-(m_voxelWidth/2),posz-(m_voxelWidth/2)),new Vector2(ActiveChunks[i].m_pos.x,ActiveChunks[i].m_pos.z))<DetailDistance )
							//if chunk has grass call render on that grass
								ActiveChunks[i].renderGrass();	
						

				

						
					}
				}
			
	
			
		
		

		}
		}

		//method to destroy object 
		//had to do this as destroy can only be called from the main thread
	void destroy(int x,int z){
				//Create the voxel data
		
		
		if(m_voxelChunk[x,z].haschanged && m_voxelChunk[x,z].shouldrender)
		m_voxelChunk[x,z].saveVoxels();
			//if(m_voxelChunk.ContainsKey(x,z))
			//requestedChunks.Remove(m_voxelChunk[x,z]);
			if(m_voxelChunk.ContainsKey(x,z))
			Todestroy.Add(m_voxelChunk[x,z]);
			if(m_voxelChunk.ContainsKey(x,z))
			m_voxelChunk.Remove(x,z);
			}
		//threaded chunk generation with debugging if theres an error
	void generateChunks(){
		while (canContinue)
		{
			try
			{	


				if(requestedChunks.Count >0){





						//Create the voxel data
						Vector2 ppos = new Vector2(player.x,player.z);
						//order the chunks before creation so that oonly chunks closest to player get created first
						//had to copy to another array before ordering as it caused errors otherwise
						requestedChunksOrdered = requestedChunks.ToList();
						requestedChunksOrdered = requestedChunksOrdered.OrderBy(C => Vector2.Distance(ppos,new Vector2(C.m_pos.x,C.m_pos.z))).ToList();
						//use first chunk in the orderered list to create meshes 
						VoxelChunk chunk = requestedChunksOrdered[0];
						//createmeshes using voxel data
									chunk.CreateMeshesAndvoxels(true);
						//add to active chunks list
									ActiveChunks.Add(chunk);
						//remove chunk from list so ass to not create again
									requestedChunks.Remove(chunk);
									
									
								



							
					}
						
				//sleep thread if no chunks to create	
else Thread.Sleep(5);


					//catch all exceptions and display message in console
			}catch (Exception e)
				{Debug.LogError(e.StackTrace+e.Message);
			}
			
		}
	}


		//main method for calling chunk creation some optimizations could be done here
void generateterrains () 
	{	
		while (canContinue)
	{
		try
			{

					//Vector2 Refpos =new Vector2(playerGlobal.x,playerGlobal.z);

					//basic routine for creating chunks 
					for(int i =0;i<distanceToload*m_voxelWidth;i+=m_voxelWidth){
					posx = Mathf.RoundToInt( playerGlobal.x/m_voxelWidth)*m_voxelWidth;
						posy = Mathf.RoundToInt( playerGlobal.y/m_voxelHeight)*m_voxelHeight;
					posz = Mathf.RoundToInt( playerGlobal.z/m_voxelWidth)*m_voxelWidth;
						for(int x=(int)posx -i;x <(int)posx +i ;x+=m_voxelWidth){if(canContinue){
								for(int z=(int)posz -i;z <(int)posz +i;z+=m_voxelWidth){if(canContinue){
										//this is for the voxel editing at runtime 
										//if theres any new chunks that need to be updated they are added here
										if(newvoxelChunk.Count >0){
											
											for(int N=  0;N < newvoxelChunk.Count;N++){if(canContinue){
													//Create the voxel data
													int C = newvoxelChunk.Count-1;
													
													if(C<newvoxelChunk.Count&&newvoxelChunk[C]!=null){
														//set various flags so the engine knows it needs to create a new mesh for terrain
														newvoxelChunk[C].hascollider=false;
														newvoxelChunk[C].hasvoxel=false;
														newvoxelChunk[C].CreateMeshesWithVoxels();	
														newvoxelChunk.Remove(newvoxelChunk[C]);	
													}
													
												}
												
											}
										}
										//if the terrain has been edited calling saveTerrains() method will save the voxel data to a file 
										//here i just set the flag of this script to cansave =true; and it saves automatically
										if(cansave){
											saveTerrains();
											cansave = false;
										}
										//set up variables for distance checking
											Vector2 chunkpos = new Vector2((x) ,(z));

											Vector2 playerpos = new Vector2(player.x,player.z);
										//distance checking
										dist = Vector2.Distance(playerpos , chunkpos);
												if(dist < distanceToload){
											//check if the chunk already exists if not create a chunk with x *y * z of voxels
										if(!m_voxelChunk.ContainsKey(x,z)){
												pos = new Vector3(chunkpos.x, offset.y  , chunkpos.y);
												//set variables for chunk creation
													chunk = new VoxelChunk(pos, m_voxelWidth, m_voxelHeight, m_voxelLength, m_surfaceLevel);
													//add chunk to double key dictionary
													m_voxelChunk.Add(x,z,chunk) ;
												//set flag on chunk
													m_voxelChunk[x,z].hasproccessed=true;
												//add chunk to list of chunks that need noise added to them
													requestedChunks.Add(m_voxelChunk[x,z]);

													}
												}
											//if the chunk already exists and its distance is greater then render distance + 50 then call destroy on the chunk
										else if(m_voxelChunk.ContainsKey(x,z) && dist > distanceToload+64){
													destroy(x,z);
													
												}
									}}}}
					}










				
				//Creates the mesh form voxel data using the marching cubes plugin and creates the mesh collider
					

		
					//catch exceptions
			}catch (Exception e)
			{Debug.LogError(e.StackTrace);
			}
			
		}
	}

	/*void processChunks(){
		while (canContinue)
		{
			try
			{

					
				if(requestedChunks.Count>0){
					for(int i = 0;i < requestedChunks.Count;i++){
						if(canContinue){
							if(i<requestedChunks.Count){

						
						requestedChunks.RemoveAt(i);
						
						
					}
					
				} 
				}
				}
				
			}catch (Exception e)
			{	Debug.Log("ErrorProccessChunks");
				StreamWriter writer = new StreamWriter("Error Log.txt");
				writer.WriteLine(e.Message + "\r\n" + e.StackTrace);
				writer.Close();
			}
		}
	}*/
	
	void OnApplicationQuit(){
			//set flag to stop generation
		canContinue=false;
			//abort thread 1
		thread.Abort();
			//abort thread 2
		thread2.Abort();
		Debug.Log("thread aborted");
	}

	
}
}
