using UnityEngine;
using System.Collections;
using VoxelEngine;
using System.Collections.Generic;

public class plantPlacer : MonoBehaviour {
	public Vector3[] vertices;
	public static GameObject[] trees;
	public static GameObject[] grass;
	public static float[] treesWeights;
	public static float[] grassWeights;
	public int maxTrees;
	public int maxGrass;
	public static VoxelTerrainEngine terain;
	public static int chance;
	public VoxelChunk chunk;
	public int G = 0;
	static Mesh []mesh;
	static Material []material;
	GameObject gameo;
	Transform player;
	public Camera cam;
	int layer=0;
	Vector3 thispos;
	public Color[] control;
	float weight;
	float totalweights;
	bool foundgrass;
	// Use this for initialization
	void Start () {
	
		cam =Camera.main;
		player = Camera.main.transform;
		if(terain==null){
			terain = GameObject.FindObjectOfType<VoxelTerrainEngine>();
		}
		thispos = new Vector3(transform.position.x+(terain.m_voxelWidth/2),transform.position.y+(terain.m_voxelHeight/2),transform.position.z+(terain.m_voxelWidth/2));

		UnityEngine.Random.seed =terain.m_surfaceSeed;
		if(maxTrees==0){
		maxTrees = terain.maxTrees;
		maxGrass = terain.maxGrass;
		grass = terain.grass;
			trees = terain.trees;
		grassWeights = terain.grassWeights;
		treesWeights= terain.treesWeights;}
		int g = grass.Length;
		for(int t=0;t<grassWeights.Length;t++)
			totalweights+=grassWeights[t];
		if(mesh==null){
		mesh = new Mesh[g];
			material = new Material[g];
		for(int i =0;i < g;i++){
			mesh[i]=grass[i].GetComponent<MeshFilter>().mesh;
			material[i]=grass[i].GetComponent<MeshRenderer>().material;
			}}
	StartCoroutine(spawnstuff());}
	IEnumerator spawnstuff(){
		yield return new WaitForSeconds(1);
		chance = UnityEngine.Random.Range(1,10);
		int T = trees.Length;
		int g = grass.Length;
		int V = vertices.Length;
		layer = LayerMask.NameToLayer(terain.grassLayerName);

			chance = UnityEngine.Random.Range(1,10);
			G=0;
		RaycastHit hit;
			for(int i = 0;i < V;i++){
				if(G<maxGrass ){
				int v = Random.Range(0,V);
				if(Physics.Raycast(vertices[v]+transform.position,Vector3.up,out hit,500,terain.mask )==false){

					int R =0;
					float TWeights = 0;
					//basic weighting of grass which can be set up on Terrain script for each grass values range is 0.0f to 10.0f
					weight = Random.Range(0,totalweights);
					for(int t=0;t<grassWeights.Length;t++){
						if(TWeights<=weight){
						TWeights+=grassWeights[t];

						if(TWeights>weight){
							R=t;}

					}
					}
					
					//supposed to check if alpha value is greater then x place grass here but couldnt get it to work
					//maybe someone else knows how to do it
					if(control[v].a<0.9f&&control[i].a>0.001f){
					G++;
				//int R =UnityEngine.Random.Range(0,g);
				chunk.grassmesh.Add(mesh[R]);
				chunk.grassmat.Add(material[R] );
					chunk.rot.Add(Random.Range(0.0f,360.0f));
					if(Physics.Raycast(vertices[v]+transform.position+Vector3.up+(Vector3.right*Random.Range(-1.0f,1.0f)),Vector3.down,out hit,10,terain.mask )){
				chunk.pos.Add(hit.point);
					}
					else chunk.pos.Add(vertices[v]+transform.position);
				}
				}

	}
		}
	//if the voxels were changed load the changes im going to implement this with the grass as well
		// so that we dont get grass in tunnels 
			if(chunk.VoxelSaver.GetFloat("TreeCount")!=0){

				for(int i = 0;i < chunk.VoxelSaver.GetFloat("TreeCount");i++){
					gameo = Instantiate (trees[(int)chunk.VoxelSaver.GetFloat(" index "+i)],Vector3.zero,Quaternion.identity )as GameObject;
					gameo.transform.position = new Vector3(
					chunk.VoxelSaver.GetFloat("x"+i),
					chunk.VoxelSaver.GetFloat("y"+i),
						chunk.VoxelSaver.GetFloat("z"+i));
					chunk.treelist.Add(gameo.transform.position);
					chunk.treeindex.Add((int)chunk.VoxelSaver.GetFloat(" index "+i));
			}
				chunk.VoxelSaver.DeleteAll();
				chunk.VoxelSaver.Flush();
			}
			else{
				if(chance==2||chance ==7||chance==9)
		for(int i = 0;i < maxTrees;i++){
				int t =UnityEngine.Random.Range(0,V);
				int num = UnityEngine.Random.Range(0,T);
				if(Physics.Raycast(vertices[t]+transform.position,Vector3.up,out hit,500,terain.mask )==false&&trees.Length>0){

					gameo = Instantiate (trees[num],Vector3.zero,Quaternion.identity )as GameObject;
				gameo.transform.parent = transform;
				gameo.transform.position = vertices[t]+transform.position+Vector3.down;
				gameo.isStatic = gameObject.isStatic;
				chunk.treelist.Add(gameo.transform.position);
				chunk.treeindex.Add(num);
			}
			}
		}
			chance = UnityEngine.Random.Range(1,10);
		vertices=null;
			Destroy(this);
	}
	// Update is called once per frame

}


