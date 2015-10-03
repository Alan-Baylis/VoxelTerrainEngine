using UnityEngine;
using System.Collections;

public enum foot{
	footR = 0,
	footL = 1,
	Body = 2,

}

[ExecuteInEditMode]
public class MoveObstacle : MonoBehaviour {
	public foot Foot;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Foot == foot.footL)
	Shader.SetGlobalVector("_Obstacle",transform.position);
		if(Foot == foot.footR)
			Shader.SetGlobalVector("_Obstacle2",transform.position);
		if(Foot == foot.Body)
			Shader.SetGlobalVector("_Obstacle3",transform.position);
	}
}
