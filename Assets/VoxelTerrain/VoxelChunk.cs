using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using VoxelEngine;
namespace VoxelEngine{

public class VoxelChunk {
	public enum VoxelType{
		Dirt = 0,
		SandStone = 1,
		Stone = 2,
		Grass = 3,
		Iron = 4,
		Gold = 5,
		GunPowder = 6,
		Tungsten = 7,

		}
	public List<Vector3>Pos= new List<Vector3>();
	public List<float>Rot= new List<float>();
	public List<Mesh>GrassMesh = new List<Mesh>();
	public List<Material>GrassMat = new List<Material>();
	public byte[,,]Voxels;
	public Vector3 m_pos;
	public GameObject m_mesh;
	float m_surfaceLevel;
	public Vector3[] verts;
	public int[]tris;
	Vector3[] normals;
	public Mesh mesh;
	public bool HasVoxel;
	public Transform mytransform;
	public bool canCreatemesh;
	public bool canDraw;
	public bool shouldrender;
	public static Transform parent;
	public static VoxelTerrainEngine generator;
	public bool hascollider;
	public MeshCollider col;
	public bool hasproccessed;
	public string FileName;
	public string SaveName;
	public SaverVoxel VoxelSaver;
	public bool HasChanged;
	public bool HasGrass;
	public Camera cam;
	int layer=0;
	public int G = 0;
	MeshFilter meshfilter;
	MeshRenderer meshrender;
	public int size;
	public List<Vector3>treelist = new List<Vector3>();
	public List<int>treeindex = new List<int>();
	Color[] control;

public VoxelChunk(Vector3 pos, int width, int height, int length, float surfaceLevel)
	{			


			m_surfaceLevel = surfaceLevel;
			//As we need some extra data to smooth the voxels and create the normals we need a extra 5 voxels
			//+1 one to create a seamless mesh. +2 to create smoothed normals and +2 to smooth the voxels
			//This is a little unoptimsed as it means some data is being generated that has alread been generated in other voxel chunks
			//but it is simpler as we dont need to access the data in the other voxels. You could try and copy the data
			//needed in for the other voxel chunks as a optimisation step

			//set voxel size and data
			Voxels = new byte[width+5, height+5, length+5];

			//set position so that voxel position matches world position if translated
			m_pos = pos - new Vector3(2f,2f,2f);

			//set file name for voxel saver

			FileName = "VoxelChunk "+m_pos;

			SaveName = "Saved Chunks";

			VoxelSaver = new SaverVoxel(FileName+".txt",SaveName);

			VoxelSaver.fileName = SaveName + "/" + FileName+".txt";


	}
		///method for rendering terrain
public void Render()
		{

			G = GrassMesh.Count;

		if(cam==null){

			layer = LayerMask.NameToLayer(generator.GrassLayerName);
			cam=Camera.main;
			}
			//i use graphics.drawmesh to draw my meshes wasnt sure if this was the right move or not
			//maybe someone knows better?

			Graphics.DrawMesh(mesh,m_pos,Quaternion.identity ,
			generator.m_material,parent.gameObject.layer
			, cam , 0 ,null, UnityEngine.Rendering.ShadowCastingMode.TwoSided);

		}
//some optimisations could be done like combining meshes
///method for rendering grass 
public void RenderGrass()
		{
		//if we have grass render render it otherwise ignore
		if(G>0)
		for(int i = 0;i < G;i++){
		   Graphics.DrawMesh(GrassMesh[i],Pos[i],Quaternion.Euler(0,Rot[i],0 )
		   ,GrassMat[i],layer , cam  , 0 ,null , UnityEngine.Rendering.ShadowCastingMode.Off );
			}
		}

		//this basically allows you set the value where 0 is full and 255 is empty
		public void SetVoxels(Vector3 voxelpos ,byte value){

		if(mytransform!=null)
			voxelpos = mytransform.InverseTransformPoint(voxelpos);

			//rounds off the vector3s to nearest integer as voxels are integer values
			int voxelpositionX = Mathf.RoundToInt(voxelpos.x);

			int voxelpositionY = Mathf.RoundToInt(voxelpos.y);

			int voxelpositionZ = Mathf.RoundToInt(voxelpos.z);

			//neccesary checks to ensure we are not checking out of bounds voxels
		if(voxelpositionX<Voxels.GetLength(0) && voxelpositionY<Voxels.GetLength(1)-5
		   &&voxelpositionZ<Voxels.GetLength(2) && voxelpositionX>=0 && voxelpositionY>=5
		   && voxelpositionZ>=0){	

		if(Voxels[voxelpositionX,voxelpositionY,voxelpositionZ]!=value){

			Voxels[voxelpositionX,voxelpositionY,voxelpositionZ] =value;

			//tell the engine to regenerate the terrain based on new voxel data
			HasChanged=true;

			//send back to tell the engine its got a voxel at this coordinate
			HasVoxel=true;
				}


			}
			//send back to tell the engine it hasnt got voxel at this coordinate
		else HasVoxel=false;
		
	}

		//cant remember what this does but it looks like it basically checks if theres a voxel there?
public bool CheckVoxels(Vector3 VoxelPos ){
		if(mytransform!=null)

			VoxelPos = mytransform.InverseTransformPoint(VoxelPos);
			
			int voxelpositionX = Mathf.RoundToInt(VoxelPos.x);
			int voxelpositionY = Mathf.RoundToInt(VoxelPos.y);
			int voxelpositionZ = Mathf.RoundToInt(VoxelPos.z);
			
			
		if(voxelpositionX<Voxels.GetLength(0) && voxelpositionY<Voxels.GetLength(1)-5&&
		   voxelpositionZ<Voxels.GetLength(2) && voxelpositionX>=0 && voxelpositionY>=5
		   && voxelpositionZ>=0){

			return true;}
				
				

			
		else return false;
			
		}

///this is to find the type of voxel thats there eg: iron , oil or gold
///can have up to 8 types which will be in shader
///still needs work id like to have separate array that holds the types in it but it uses to much memory
///still dont quite understand why it uses so much memory
public byte FindVoxelType(Vector3 VoxelPos,byte Type ){
		if(mytransform!=null)
			VoxelPos = mytransform.InverseTransformPoint(VoxelPos);
			
			int voxelpositionX = Mathf.RoundToInt(VoxelPos.x);
			int voxelpositionY = Mathf.RoundToInt(VoxelPos.y);
			int voxelpositionZ = Mathf.RoundToInt(VoxelPos.z);
			
			
		if(voxelpositionX<Voxels.GetLength(0) && voxelpositionY<Voxels.GetLength(1)-5
		   &&voxelpositionZ<Voxels.GetLength(2) && voxelpositionX>=0 && voxelpositionY>=5
		   && voxelpositionZ>=0){

			float t = (float)Voxels[voxelpositionX,voxelpositionY,voxelpositionZ]/255;

			Type = (byte)(t*8);

			return Type ;
				}
			else return 9;
			
		}
	
//threaded mesh creation 
// basically just creates the triangles and vertices on a thread then adds them to a mesh
//in the main thread
public void CreateMeshesAndvoxels(bool MakeNewVoxels){

			canCreatemesh = false;
		
		if(MakeNewVoxels && VoxelSaver.GetBool("hasSavedChunk")==false){

			Voxels = MeshFactory.CreateVoxels(Voxels,m_pos,this);
			
		}

		else {
			LoadVoxels();
			}

			//create the verts
			verts = MeshFactory.MarchingCubes.CreateVertices(Voxels,this,2,2);

			//store the size so as to avoid garbage creation
			size = verts.Length;

			//create normals
			normals = MeshFactory.CalculateNormals(Voxels,size,verts);

			//create colors
			control = new Color[size];

			float R = 0;
			float G = 0;
			float B = 0;
			float A = 0;

			for(int i = 0; i < size; i++)
			{
			R=0;
			G=0;
			B=0;
			A = 0;

			int x = Mathf.RoundToInt(verts[i].x-normals[i].x);
			int y = Mathf.RoundToInt(verts[i].y-normals[i].y);
			int z = Mathf.RoundToInt(verts[i].z-normals[i].z);

			//conversion from voxel value to voxel type
				//seems to work well
			byte vox = (byte)((float)Voxels[x,y,z]/255*8);
			//basically each value gets assigned a color of 0.5 to 1.0 
				//in theory each decimal could be a voxel type
		if(vox==(int)VoxelType.Dirt)
			R = 0.5f;
		if(vox==(int)VoxelType.Stone )
			G = 0.5f;
		if(vox==(int)VoxelType.SandStone)
			B = 0.5f;
		if(vox==(int)VoxelType.Grass)
			A = 0.5f;
				
		if(vox==(int)VoxelType.Iron)
			R = 1;
		if(vox==(int)VoxelType.Gold)
			G = 1;
		if(vox==(int)VoxelType.GunPowder)
			B = 1;
		if(vox==(int)VoxelType.Tungsten)
			A = 1;
			control[i] = new Color(R,G,B,A);
			}
			canCreatemesh = true;

	}
	//this is called if the voxel has changed and needs to be reloaded 
		//had to do it this way or else we end up with strange terrain errors

public void CreateMeshesWithVoxels(){
			//set flag so this mesh wont get created twice
			canCreatemesh = false;

			verts = MeshFactory.CreateverticesVoxels(Voxels,this);
			//cache vertices size to avoid gc allocation

			size = verts.Length;

			normals = MeshFactory.CalculateNormalsRemove (Voxels,size,verts);

			control = new Color[size];
			
			float R = 0;
			float G = 0;
			float B = 0;
			float A = 0;

			for(int i = 0; i < size; i++)
			{
			R=0;
			G=0;
			B=0;
			A = 0;
			int x = Mathf.RoundToInt(verts[i].x-normals[i].x);

			int y = Mathf.RoundToInt(verts[i].y-normals[i].y);

			int z = Mathf.RoundToInt(verts[i].z-normals[i].z);
			
			byte vox = (byte)((float)Voxels[x,y,z]/255*8);
				
		if(vox==(int)VoxelType.Dirt)
			R = 0.5f;

		if(vox==(int)VoxelType.Stone )
			G = 0.5f;

		if(vox==(int)VoxelType.SandStone)
			B = 0.5f;

		if(vox==(int)VoxelType.Grass)
			A = 0.5f;
				
		if(vox==(int)VoxelType.Iron)
			R = 1;

		if(vox==(int)VoxelType.Gold)
			G = 1;

		if(vox==(int)VoxelType.GunPowder)
			B = 1;

		if(vox==(int)VoxelType.Tungsten)
			A = 1;

			control[i] = new Color(R,G,B,A);
			}
		canCreatemesh = true;

	}

		//save voxels this all works perfectly i dont think it needs much explaining only thing that could 
		//be done is some sort of compression although i couldnt find any compression methods in unity
	public void SaveVoxels(){
			//flatten array of 3d voxels

			byte []voxel = new byte[Voxels.GetLength(0)*Voxels.GetLength(1)*Voxels.GetLength(2)];

		for(int x =0; x < Voxels.GetLength(0);x++){

		for(int y =0; y < Voxels.GetLength(1);y++){

		for(int z =0; z < Voxels.GetLength(2);z++){

			voxel[x + y*Voxels.GetLength(0)+z*Voxels.GetLength(0)*Voxels.GetLength(1)] = Voxels[x,y,z];

					}
				}
			}	
			//if no directory exists create one
		if(!Directory.Exists(SaveName + "/"+FileName))

			Directory.CreateDirectory(SaveName + "/"+FileName);

			File.WriteAllBytes(SaveName + "/"+FileName + "/" +FileName+".dat", voxel );

			VoxelSaver.SetBool("hasSavedChunk",true);

			VoxelSaver.SetFloat("TreeCount",treelist.Count);

		for(int i = 0;i < treelist.Count;i++){

			VoxelSaver.SetFloat(" index "+i,treeindex[i]);

			VoxelSaver.SetFloat("x"+i,treelist[i].x);

			VoxelSaver.SetFloat("y"+i,treelist[i].y);

			VoxelSaver.SetFloat("z"+i,treelist[i].z);

			}

			VoxelSaver.Flush();

			VoxelSaver.DeleteAll();
	}
			//load the voxels if directory exists

	public void LoadVoxels(){
			byte [] values = File.ReadAllBytes(SaveName + "/"+FileName + "/" +FileName+".dat");

			int i = values.Length;


		for(int x =0; x < Voxels.GetLength(0);x++){

		for(int y =0; y < Voxels.GetLength(1);y++){

		for(int z =0; z < Voxels.GetLength(2);z++){

			Voxels[x,y,z] = values[x + y*Voxels.GetLength(0)+z*Voxels.GetLength(0)*Voxels.GetLength(1)];

					}
				}
			}

	
		
	}
		//main thread mesh assigning 
		//assigns meshes vertices , triangles and colors to mesh 
		//as well as adds the plant placer script which plants the trees and grass and rocks on the mesh
	public void CreateMesh( )
	{	
		
		if(mesh==null)
			mesh = new Mesh();
		else if (mesh!=null)
			mesh.Clear();

			mesh.vertices = verts;
			mesh.triangles = tris;
			mesh.normals = normals;


			size = 0;
		if(verts!=null)
			size = verts.Length;

		if(size>0)
			shouldrender=true;

		else 
			shouldrender=false;

		if(size>0){
			//store in colors 
			mesh.colors = control;

			//mark mesh as dynamic not sure how well this works?
			mesh.MarkDynamic();

		if(m_mesh==null)
			m_mesh = new GameObject("Voxel Mesh " + m_pos.x.ToString() + " " + m_pos.y.ToString()
			+ " " + m_pos.z.ToString());
			m_mesh.tag = parent.tag;
			m_mesh.layer = parent.gameObject.layer;
			m_mesh.isStatic = parent.gameObject.isStatic;
			m_mesh.transform.parent = parent;
			m_mesh.transform.localPosition = m_pos;
			mytransform = m_mesh.transform;

		if(col!=null ){
			UnityEngine.Object.DestroyImmediate(col);}
		if(size>0){
			col = m_mesh.AddComponent<MeshCollider>();
			col.sharedMesh = mesh;}

		if(HasGrass ==false){
			//add plant placer component to mesh object 
			plantPlacer placer = m_mesh.AddComponent<plantPlacer>();
			//assign vertices for plant place to use to know where to place the plants and trees
			placer.Vertices = verts;
			//assign colors to plant placer supposed to be used to only place grass on grass area but
			//i couldnt get it to work
			placer.control = control;
			placer.chunk = this;
			HasGrass = true;}
			//set flag so engine knows it can draw mesh now
			canDraw=true;
			//nullify all vertices and tris and normals so as to not hold onto unnecassary information
			tris = null;
			verts=null;
			normals=null;


			}
		}
	}
}
