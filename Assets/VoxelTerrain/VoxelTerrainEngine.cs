using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using System.Linq;
using PlayerPrefs = VoxelEngine.SaverVoxel;
using VoxelEngine.DoubleKeyDictionary;
namespace VoxelEngine{
public class VoxelTerrainEngine: MonoBehaviour 
{
	public Vector3 Position;
	public LayerMask mask;
	public Material m_material;
	public int distanceToload;
	[Range(0,200)]
	public float DetailDistance;
	public String GrassLayerName;
	public int MaxTrees;
	public int MaxGrass;
	public bool MakeCaves;
	public int ChunksY;
	public GameObject []Trees;
	public float []TreesWeights;
	public GameObject[] Grass;
	public float []GrassWeights;
	Vector2 Campos;
	[HideInInspector]
	public Transform Parent;
	private DoubleKeyDictionary<int, int, VoxelChunk>m_voxelChunk = new DoubleKeyDictionary<int, int, VoxelChunk>();
	public int m_surfaceSeed = 4;
	public int m_voxelWidth = 32, m_voxelHeight = 128, m_voxelLength = 32;
	public float m_surfaceLevel = 0.0f;
	public float Frequency;
	public float Amplitude;
	public int Oct;
	public float Gfrequency;
	public float Gamplitude;
	public int Goct;
	public float Cavefrequency;
	public float Caveamplitude;
	public int Caveoct;
	PerlinNoise m_surfacePerlin;
	PerlinNoise  m_cavePerlin;
	public Thread Thread;
	public Thread Thread2;
	[HideInInspector]
	public float PosX ;
	[HideInInspector]
	public float PosY ;
	[HideInInspector]
	public float PosZ;
	public static VoxelTerrainEngine Generator;
	public VoxelChunk Chunk;
	[HideInInspector]
	public Vector3 ParentPos;
	Vector3 Offset;
	Vector3 Pos;
	Vector3 PlayerposGlobal;
	bool CanContinue;
	[HideInInspector]
	public Vector3 Player;
	[HideInInspector]
	public Vector3 PlayerGlobal;
	public List<VoxelChunk>ToDestroy = new List<VoxelChunk>();
	public List<VoxelChunk>ActiveChunks = new List<VoxelChunk>();
	public List<VoxelChunk>RequestedChunks = new List<VoxelChunk>();
	public List<VoxelChunk>RequestedChunksOrdered = new List<VoxelChunk>();
	public List<VoxelChunk>NewVoxelChunk = new List<VoxelChunk>();
	public float Dist;
	bool FirstTime = true;
	[HideInInspector]
	public int xx;
	[HideInInspector]
	public int yy ;
	[HideInInspector]
	public int zz ;
	public bool cansave;
	Transform target;
	public WindZone Zone;
	[HideInInspector]
	public Camera Cam;

public void Start(){

				target = Camera.main.transform;

				Cam = Camera.main;

				Player = transform.InverseTransformPoint(target.position );

				Generator = this;

				Parent = transform;

				VoxelChunk.parent = transform;

				VoxelChunk.generator = Generator;

				MeshFactory.Frequency = Frequency;

				MeshFactory.Amplitude = Amplitude;

				MeshFactory.Oct= Oct;

				MeshFactory.GFrequency = Gfrequency;

				MeshFactory.GAmplitude = Gamplitude;

				MeshFactory.GOct = Goct;

				MeshFactory.CaveFrequency = Cavefrequency;

				MeshFactory.CaveAmplitude = Caveamplitude;

				MeshFactory.CaveOct = Caveoct;

				MeshFactory.m_surfaceLevel = m_surfaceLevel;

				m_surfacePerlin = new PerlinNoise(m_surfaceSeed);

				m_cavePerlin = new PerlinNoise(m_surfaceSeed/2);

				MeshFactory.SurfacePerlin = m_surfacePerlin;

				MeshFactory.CavePerlin = m_cavePerlin;

				MeshFactory.MarchingCubes = new MarchingCubes();

				MeshFactory.MarchingCubesVoxels = new MarchingCubes();

				MeshFactory.MakeCaves = MakeCaves;

				Offset = new Vector3(m_voxelWidth, m_voxelHeight/2, m_voxelLength);

				ParentPos = Parent.position;
	
				MarchingCubes.SetTarget(126);

				MarchingCubes.SetWindingOrder(0, 1, 2);

				CanContinue = true;

				StartCoroutine(StartThreads());


	}
		//start all threads
public IEnumerator StartThreads(){

				Thread = new Thread(GenerateTerrains);

				Thread.IsBackground=true;

				Thread.Start();

				Thread2 = new Thread(GenerateChunks);
				
				Thread2.IsBackground=true;

				Thread2.Start();

				yield return new WaitForSeconds(1);


		}
		//raycast for the voxel
		//basically just a normal raycast but with better targetting for the voxels
public static bool RaycastVoxels(Ray ray, out RaycastHit hitinfo,float distance,LayerMask mask ){

				RaycastHit hit;

			if(Physics.Raycast(ray.origin,ray.direction, out hit, distance ,mask )){
				
				hit.point = new Vector3(Mathf.RoundToInt(hit.point.x+(ray.direction.x/2))
				,Mathf.RoundToInt(hit.point.y+(ray.direction.y/2)),Mathf.RoundToInt(hit.point.z
				+(ray.direction.z/2)));

				hitinfo = hit;

				return true;
			}
		
			else {

				hitinfo = hit;

				return false;
			}

		}
		//raycast for the voxel
		//basically just a normal raycast but with better targetting for the voxels
public static bool RaycastVoxels(Ray ray, out RaycastHit hitinfo,float distance ){

				RaycastHit hit;

			if(Physics.Raycast(ray.origin,ray.direction, out hit, distance )){
				
				hit.point = new Vector3(Mathf.RoundToInt(hit.point.x+(ray.direction.x/2))
				,Mathf.RoundToInt(hit.point.y+(ray.direction.y/2)),Mathf.RoundToInt(hit.point.z
				+(ray.direction.z/2)));
				
				hitinfo = hit;

				return true;
			}
			
			else {

				hitinfo = hit;

				return false;
			}
			
		}
		//old method can be used of needed
public void RemoveVoxels(Vector3 hitpoint,byte value){

				int C = ActiveChunks.Count;

			for(int c = 0;c < C;c++){

			if( ActiveChunks[c]!=null){

				ActiveChunks[c].SetVoxels(hitpoint,value);
				}
			if( ActiveChunks[c]!=null &&ActiveChunks[c].HasVoxel){
				
				NewVoxelChunk.Add(ActiveChunks[c]);
			}
			}


	}
				//check if voxel exists and to what chunk it belongs to 
				//just VoxelTerrainEngine.raycastvoxels to raycast for a
				//voxel then check the hit.point against this
				//should return a chunk
public bool CheckVoxels(Vector3 HitPoint,out VoxelChunk chunk){

				int C = ActiveChunks.Count;

				chunk = null;

			for(int c = 0;c < C;c++){

			if(ActiveChunks[c].CheckVoxels(HitPoint)){

						chunk = ActiveChunks[c];
						
					}
			}
			if(chunk!=null)

				return true;

			else 

				return false;
			
			
		
		}
		//method for saving changes to terrain at runtime
public void SaveTerrains(){

		for(int i = 0;i < ActiveChunks.Count;i++){

		if(ActiveChunks[i].HasChanged && ActiveChunks[i].shouldrender){

			ActiveChunks[i].SaveVoxels();
		}


	
		}
	}
		//static method for setting voxel values 
		//can be used for adding or removing voxels
		//once this method is called the chunk its on will
		//be automatically located and set to re create the mesh using using new voxel data
public static void SetVoxels(Vector3 voxel,byte value){

			Generator.RemoveVoxels(voxel,value);
		
	}

	


void Update(){
			Player = transform.InverseTransformPoint( target.position);

			PlayerGlobal = target.position;

			RaycastHit hit;

			//set the wind variables on the shader comment this out if you dont use my grass shader
			Shader.SetGlobalFloat("WindAmount",Zone.windMain );
			Shader.SetGlobalFloat("WaveSize" ,Zone.windPulseMagnitude);
			Shader.SetGlobalFloat("windSpeed" , Zone.windPulseFrequency);
			Shader.SetGlobalFloat("SqrDistance", Zone.windTurbulence);
			Shader.SetGlobalFloat("WindDirectionx",Zone.transform.forward.x);
			Shader.SetGlobalFloat("WindDirectiony",Zone.transform.forward.y);
			Shader.SetGlobalFloat("WindDirectionz",Zone.transform.forward.z);

	
			//destroy meshes that are to far away 
		if(ToDestroy.Count>0){

				//if we have meshes to destroy destroy them 
				//need to implement a pooling system will save on memory
				//as well as stop the garbage collector from being called
		for(int i = 0; i < ToDestroy.Count;i++){

			
		if(ToDestroy[i]!=null && ToDestroy[i].mesh!=null && i < ToDestroy.Count){

			DestroyImmediate(ToDestroy[i].mesh);

			ToDestroy[i].mesh=null;

			UnityEngine.Object.DestroyImmediate(ToDestroy[i].m_mesh);

			ActiveChunks.Remove(ToDestroy[i]);

				}
				
		if(i < ToDestroy.Count){

			ToDestroy[i].canDraw=false;

			ToDestroy[i].canCreatemesh=false;

			ToDestroy.Remove(ToDestroy[i]);

		}
		}
		
			}

		

			//render active chunks in view
		if(ActiveChunks.Count>0){

		for(int i = 0; i < ActiveChunks.Count; i++){	

		if(i< ActiveChunks.Count&& ActiveChunks[i]!=null && ActiveChunks[i].canCreatemesh){

			//create the mesh . this is done in update as calling it from another thread is not allowed

			ActiveChunks[i].CreateMesh();

			ActiveChunks[i].canCreatemesh=false;
					
					}
			//small optimisation to only render whats infront of the player 
			//need to some how check for occlusion and not render things behind other objects
		if(i< ActiveChunks.Count && ActiveChunks[i]!=null &&  ActiveChunks[i].canDraw
		&& ActiveChunks[i].shouldrender){

			Position = ActiveChunks[i].mesh.bounds.center+ActiveChunks[i].mytransform.position;

			Vector3 direction = (Position-new Vector3(PosX,PosY,PosZ)).normalized;

			//call render on chunk
			ActiveChunks[i].Render();

		if(Vector2.Distance(new Vector2(PosX-(m_voxelWidth/2),PosZ-(m_voxelWidth/2)),
		   new Vector2(ActiveChunks[i].m_pos.x,ActiveChunks[i].m_pos.z))<DetailDistance )
							//if chunk has grass call render on that grass
			ActiveChunks[i].RenderGrass();	
						

				

						
					}
				}
			
	
			
		
		

		}
		}

		//method to destroy object 
		//had to do this as destroy can only be called from the main thread
void Destroy(int x,int z){
				//Create the voxel data
		
		
		if(m_voxelChunk[x,z].HasChanged && m_voxelChunk[x,z].shouldrender)
			m_voxelChunk[x,z].SaveVoxels();

		if(m_voxelChunk.ContainsKey(x,z))
			ToDestroy.Add(m_voxelChunk[x,z]);

		if(m_voxelChunk.ContainsKey(x,z))
			m_voxelChunk.Remove(x,z);

			}
		//threaded chunk generation with debugging if theres an error
	void GenerateChunks(){

		while (CanContinue)
		{
			try
			{	


		if(RequestedChunks.Count >0){

			//Create the voxel data
			Vector2 ppos = new Vector2(Player.x,Player.z);

			//order the chunks before creation so that oonly chunks closest to player get created first
			//had to copy to another array before ordering as it caused errors otherwise
			RequestedChunksOrdered = RequestedChunks.ToList();
			RequestedChunksOrdered = RequestedChunksOrdered.OrderBy
			(C => Vector2.Distance(ppos,new Vector2(C.m_pos.x,C.m_pos.z))).ToList();
			
			//use first chunk in the orderered list to create meshes 
			VoxelChunk chunk = RequestedChunksOrdered[0];

			//createmeshes using voxel data
			chunk.CreateMeshesAndvoxels(true);

			//add to active chunks list
			ActiveChunks.Add(chunk);

			//remove chunk from list so ass to not create again
			RequestedChunks.Remove(chunk);
									
									
								



							
					}
						
		//sleep thread if no chunks to create	
		else Thread.Sleep(5);


		//catch all exceptions and display message in console
			}
		catch (Exception e){
			Debug.LogError(e.StackTrace+e.Message);
			}
			
		}
	}


//main method for calling chunk creation some optimizations could be done here
void GenerateTerrains () 
	{	
		while (CanContinue)
	{
		try
			{

			//Vector2 Refpos =new Vector2(playerGlobal.x,playerGlobal.z);
			//basic routine for creating chunks 
		for(int i =0;i<distanceToload*m_voxelWidth;i+=m_voxelWidth){

			//set player positon relative to chunks basically rounding to chunk size
			PosX = Mathf.RoundToInt( PlayerGlobal.x/m_voxelWidth)*m_voxelWidth;

			PosY = Mathf.RoundToInt(PlayerGlobal.y/m_voxelHeight)*m_voxelHeight;

			PosZ = Mathf.RoundToInt( PlayerGlobal.z/m_voxelWidth)*m_voxelWidth;

			for(int x=(int)PosX -i;x <(int)PosX +i ;x+=m_voxelWidth){if(CanContinue){

			for(int z=(int)PosZ -i;z <(int)PosZ +i;z+=m_voxelWidth){if(CanContinue){

			//this is for the voxel editing at runtime 
			//if theres any new chunks that need to be updated they are added here
		if(NewVoxelChunk.Count >0){
											
			for(int N=  0;N < NewVoxelChunk.Count;N++){if(CanContinue){
					
			int C = NewVoxelChunk.Count-1;
			//Create the voxel data		

		if(C<NewVoxelChunk.Count&&NewVoxelChunk[C]!=null){
			//set various flags so the engine knows it needs to create a new mesh for terrain

			NewVoxelChunk[C].hascollider=false;

			NewVoxelChunk[C].HasVoxel=false;

			NewVoxelChunk[C].CreateMeshesWithVoxels();	

			NewVoxelChunk.Remove(NewVoxelChunk[C]);	
							
													}
													
												}
												
											}
										}

			//if the terrain has been edited calling saveTerrains() method
			//will save the voxel data to a file 
			//here i just set the flag of this script to cansave =true; and it saves automatically
		if(cansave){

			SaveTerrains();

			cansave = false;
										}

			//set up variables for distance checking

			Vector2 chunkpos = new Vector2((x) ,(z));

			Vector2 playerpos = new Vector2(Player.x,Player.z);

			//distance checking
			Dist = Vector2.Distance(playerpos , chunkpos);

		if(Dist < distanceToload){
			//check if the chunk already exists if not create a chunk with x *y * z of voxels

		if(!m_voxelChunk.ContainsKey(x,z)){
		    Pos = new Vector3(chunkpos.x, Offset.y  , chunkpos.y);

			//set variables for chunk creation
			Chunk = new VoxelChunk(Pos, m_voxelWidth, m_voxelHeight, m_voxelLength, m_surfaceLevel);

			//add chunk to double key dictionary
			m_voxelChunk.Add(x,z,Chunk) ;

			//set flag on chunk
			m_voxelChunk[x,z].hasproccessed=true;

			//add chunk to list of chunks that need noise added to them
			RequestedChunks.Add(m_voxelChunk[x,z]);

													}
												}
			//if the chunk already exists and its distance is greater then render distance
			//+ 50 then call destroy on the chunk
		else if(m_voxelChunk.ContainsKey(x,z) && Dist > distanceToload+64){

			Destroy(x,z);		
										}
									}
								}
							}
						}
					}

					

		
			//catch exceptions
			}catch (Exception e)
			{Debug.LogError(e.StackTrace);
			}
			
		}
	}

	
	void OnApplicationQuit(){
			//set flag to stop generation
			CanContinue=false;

			//abort thread 1
			Thread.Abort();

			//abort thread 2
			Thread2.Abort();

			Debug.Log("thread aborted");
	}

	
}
}
