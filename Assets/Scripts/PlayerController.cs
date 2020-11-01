//--------------------------------------
using UnityEngine;
using System.Collections;
//--------------------------------------
public class PlayerController : MonoBehaviour 
{
	//--------------------------------------
	
	//enum for movement
	public enum MOVETYPE {IDLE=0, WALK=1, BOXPUSH=2, CHEER=3};
	
	//enum for space state
	public enum SPACESTATE {EMPTY=0, CRATE=1, COLLIDER=2};
	
	//Property for State
	public MOVETYPE PlayerState
	{
		get{return State;}
		
		set
		{
			State = value;
			
			//Set animation state
			AnimComp.SetInteger("iState",(int)State);
		}
	}
	
	//Movement Speed (Units per Second)
	public float MoveDistance = 2.0f;
	
	//Time in seconds to move
	public float MoveTime = 1.0f;
	
	//Rotate Time (In Seconds)
	public float RotTime = 1.0f;
	
	//Rotate Increments (total amount of rotation in one turn)
	public float RotIncrement = 90.0f;
	
	//Cached Transform
	private Transform ThisTransform = null;
	
	//Reference to animator
	private Animator AnimComp = null;
	
	//Level Colliders
	private Collider[] Colliders = null;
	
	//Player State
	private MOVETYPE State = MOVETYPE.IDLE;

	//Reference to last tested crate
	private Transform LastBox = null;

	//Hand position for push state
	public Transform LeftHandDest = null;
	public Transform RightHandDest = null;
	
	//--------------------------------------
	// Use this for initialization
	void Start () 
	{
		//Get Cached Transform
		ThisTransform = transform;
		
		//Get Anim Comp
		AnimComp = GetComponent<Animator>();
		
		//Get all colliders in scene
		Colliders = Object.FindObjectsOfType<Collider>();
		
		//Set starting state to idle
		PlayerState = MOVETYPE.IDLE;
		
		//Start input handling loop for player controller
		StartCoroutine(HandleInput());
	}
	//--------------------------------------
	//Rotates root transform one increment (90 degrees)
	public IEnumerator Rotate(float Increment = 0)
	{
		//Get Original Y Rotation
		float StartRot = ThisTransform.rotation.eulerAngles.y;
		
		//Get Destination Rot
		float DestRot = StartRot + Increment;
		
		//Elapsed Time
		float ElapsedTime = 0.0f;
		
		while(ElapsedTime < RotTime)
		{
			//Calculate interpolated angle
			float Angle = Mathf.LerpAngle(StartRot, DestRot, ElapsedTime/RotTime);
			
			ThisTransform.eulerAngles = new Vector3(0, Angle, 0);
			
			//Wait until next frame
			yield return null;
			
			//Update time
			ElapsedTime += Time.deltaTime;
		}
		
		//Final rotation
		ThisTransform.eulerAngles = new Vector3(0, Mathf.FloorToInt(DestRot), 0);
	}
	//--------------------------------------
	//Moves root transform one increment (2 units)
	public IEnumerator Move(float Increment = 0)
	{
		//Start position
		Vector3 StartPos = ThisTransform.position;
		
		//Dest Position
		Vector3 DestPos = ThisTransform.position + ThisTransform.forward * Increment;
		
		//Elapsed Time
		float ElapsedTime = 0.0f;
		
		while(ElapsedTime < MoveTime)
		{
			//Calculate interpolated angle
			Vector3 FinalPos = Vector3.Lerp(StartPos, DestPos, ElapsedTime/MoveTime);
			
			//Update pos
			ThisTransform.position = FinalPos;
			
			//If we are pushing then update box pos
			if(PlayerState == MOVETYPE.BOXPUSH)
				LastBox.position = new Vector3(ThisTransform.position.x, LastBox.position.y, ThisTransform.position.z)  + ThisTransform.forward * Increment;
			
			//Wait until next frame
			yield return null;
			
			//Update time
			ElapsedTime += Time.deltaTime;
		}
		
		//Complete move
		ThisTransform.position = DestPos;
		
		if(PlayerState == MOVETYPE.BOXPUSH)
			LastBox.position = new Vector3(ThisTransform.position.x, LastBox.position.y, ThisTransform.position.z)  + ThisTransform.forward * Increment;
		
		yield break;
	}
	//--------------------------------------
	//Coroutine for handling input
	public IEnumerator HandleInput()
	{
		//Loop forever, reading player input
		while(true)
		{
			if(Mathf.CeilToInt(Input.GetAxis("Vertical")) > 0)
			{
				//Validate movement - should we remain idle, walk or push?
				PlayerState = ValidateWalk();
				
				//If we are not idle, then move
				if(PlayerState != MOVETYPE.IDLE)
				{
					//Move player 1 increment
					yield return StartCoroutine(Move(MoveDistance));
				}
			}
			else
				PlayerState = MOVETYPE.IDLE; //Set to idle state - default
			
			
			if(Input.GetAxis("Horizontal") < 0.0f)
				yield return StartCoroutine(Rotate(-RotIncrement));
			
			if(Input.GetAxis("Horizontal") > 0.0f)
				yield return StartCoroutine(Rotate(RotIncrement));
	
		yield return null;
		}
	}
	//--------------------------------------
	//Tests a point in the scene to determine whether it intersects a collider
	public SPACESTATE PointState(Vector3 Point)
	{
		//Cycle through colliders and test for collision
		foreach(Collider C in Colliders)
		{
			//Point intersects a collider - determine type
			if(C.bounds.Contains(Point) && !C.gameObject.CompareTag("End"))
			{
				if(C.gameObject.CompareTag("Crate"))
				{
					//Get reference to crate
					LastBox = C.gameObject.transform;
					
					return SPACESTATE.CRATE; //Point is in crate
				}
				else
					return SPACESTATE.COLLIDER; //Else point is in collider
			}
		}
		
		//Point not in collider - space is empty
		return SPACESTATE.EMPTY;
	}
	//--------------------------------------
	//Based on surrounding objects, determine move type allowed in direction
	public MOVETYPE ValidateWalk()
	{
		//Update next box
		LastBox = null;
		
		//Get dest point in next tile
		Vector3 DestPos = ThisTransform.position + ThisTransform.forward * MoveDistance;
		
		//Get double dest point (two tiles away). For checking move destination if box is on next tile
		Vector3 DblDestPos = ThisTransform.position + ThisTransform.forward * MoveDistance * 2.0f;
		
		//Status of next space
		SPACESTATE NextStatus = PointState(DestPos);
		
		//Get last tested box
		Transform NextBox = LastBox;
		
		//Status of two spaces away
		SPACESTATE DoubelSpaceStatus = PointState(DblDestPos);
		
		//Update last box
		LastBox = NextBox;
		
		//If next space is empty then walk
		if(NextStatus == SPACESTATE.EMPTY) return MOVETYPE.WALK;
		
		//If next space has crate and two spaces is empty, then push
		if(NextStatus == SPACESTATE.CRATE && DoubelSpaceStatus == SPACESTATE.EMPTY) return MOVETYPE.BOXPUSH;
		
		//Else cannot move
		return MOVETYPE.IDLE;
	}
	//--------------------------------------
	//Update hand state for IK pass
	void OnAnimatorIK(int layerIndex)
	{
		//If box pushing
		if(PlayerState == MOVETYPE.BOXPUSH)
		{
			AnimComp.SetIKPositionWeight(AvatarIKGoal.RightHand,0.5f);
			AnimComp.SetIKRotationWeight(AvatarIKGoal.RightHand,0.5f);
			AnimComp.SetIKPositionWeight(AvatarIKGoal.LeftHand,0.5f);
			AnimComp.SetIKRotationWeight(AvatarIKGoal.LeftHand,0.5f);
			AnimComp.SetIKPosition(AvatarIKGoal.RightHand,RightHandDest.position);
			AnimComp.SetIKRotation(AvatarIKGoal.RightHand,RightHandDest.rotation);
			AnimComp.SetIKPosition(AvatarIKGoal.LeftHand,LeftHandDest.position);
			AnimComp.SetIKRotation(AvatarIKGoal.LeftHand,LeftHandDest.rotation);
		}
		else // Hands in default state
		{
			AnimComp.SetIKPositionWeight(AvatarIKGoal.RightHand,0.0f);
			AnimComp.SetIKRotationWeight(AvatarIKGoal.RightHand,0.0f);
			AnimComp.SetIKPositionWeight(AvatarIKGoal.LeftHand,0.0f);
			AnimComp.SetIKRotationWeight(AvatarIKGoal.LeftHand,0.0f);
		}
	}
	//--------------------------------------
}
