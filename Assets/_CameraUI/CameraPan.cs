using UnityEngine;
using System.Collections;

public class CameraPan : MonoBehaviour {

	public GameObject focusPoint;
    
	void LateUpdate () {
		transform.LookAt (focusPoint.transform);
	}
}
