//Script to smoothly follow object
//--------------------------------------
using UnityEngine;
using System.Collections;
//--------------------------------------
public class CameraFollow : MonoBehaviour 
{
	//Target to follow in scene
	public Transform Target = null;

	//Distance from target
	public float Distance = 10.0f;

	//Height above target
	public float Height = 5.0f;

	//Smooth params
	public float HeightDamping = 2.0f;
	public float RotationDamping = 3.0f;

	//Internal reference to cached transform
	private Transform ThisTransform = null;

	//--------------------------------------
	// Use this for initialization
	void Start () 
	{
		//Cache Transform
		ThisTransform = transform;
	}
	//--------------------------------------
	// Update is called once per frame
	void LateUpdate () 
	{
		//Exit if there's no target
		if(!Target) return;

		//Get the desired rotation and height of target
		float TargetRot = Target.eulerAngles.y;
		float TargetHeight = Target.position.y + Height;

		//Get our current rotation and height
		float CurrentRot = ThisTransform.eulerAngles.y;
		float CurrentHeight = ThisTransform.position.y;

		//Smooth rotation on Y axis
		CurrentRot = Mathf.LerpAngle(CurrentRot, TargetRot, RotationDamping * Time.deltaTime);

		//Smooth height transition
		CurrentHeight = Mathf.Lerp(CurrentHeight, TargetHeight, HeightDamping * Time.deltaTime);

		//Convert Euler rot to quaternion
		Quaternion FinalRot = Quaternion.Euler(0,CurrentRot,0);

		//Update position of camera on X-Z Plane
		ThisTransform.position = Target.position;
		ThisTransform.position -= FinalRot * Vector3.forward * Distance;

		//Set final pos and rot
		ThisTransform.position = new Vector3(ThisTransform.position.x, CurrentHeight, ThisTransform.position.z);
		ThisTransform.LookAt(Target);
	}
	//--------------------------------------
}
