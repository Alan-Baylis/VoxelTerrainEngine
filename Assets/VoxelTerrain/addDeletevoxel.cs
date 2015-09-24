using UnityEngine;
using System.Collections;
using VoxelEngine;

public class addDeletevoxel : MonoBehaviour {
	Ray ray;
	public LayerMask mask;
	public bool islocked;
	public GameObject block;
	// Use this for initialization
	void Start () {
		block = Instantiate(block,Vector3.zero,Quaternion.Euler(0,0,0))as GameObject;
		block.SetActive(true);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.L)&&islocked==false){
			islocked = true;
			Cursor.lockState = CursorLockMode.Locked;
		}
		else if(Input.GetKey(KeyCode.L)&&islocked){
			islocked = false;
			Cursor.lockState = CursorLockMode.None;
		}


		RaycastHit hit;
		ray = Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0));

		if(VoxelTerrainEngine.raycastvoxels(ray, out hit, 100 ,mask  )){
			block.transform.position = hit.point;}
		if(Input.GetButtonDown("Fire1")){
			if(VoxelTerrainEngine.raycastvoxels(ray, out hit, 100 ,mask  )){

				VoxelTerrainEngine.SetVoxels(block.transform.position,255);}}
			if(Input.GetButtonDown("Fire2")){
			if(VoxelTerrainEngine.raycastvoxels(ray, out hit, 100 ,mask  )){
				VoxelTerrainEngine.SetVoxels(block.transform.position-ray.direction,0);
	
		}
		}
		}

	}

