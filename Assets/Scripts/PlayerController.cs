using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
   public enum MOVETYPE {IDLE=0, WALK=1, BOXPUSH=2, CHEER=3};

   public MOVETYPE PlayerState
   {
       get{return State};
   }
   set
   {   
       State = value;

       AnimComp.SetInteger("iSatet",(int)State);
   }
}
// time in seconds to move 
public float MoveTime = 1.0f;

//movement speed (units per seccond)
public float MoveDistance = 2.0f;

//chached transform
private Transform ThisTransform = null;

//cplayer state
private MOVETYPE State = MOVETYPE.IDLE;

//reference to animator
private Animator AnimComp = null;

void Start ()
{
    //Get transform component
    ThisTransform = GetComponent<Transform>();

    //Get Anim Comp
    AnimComp = GetComponent<Animator>();

    //start input handeling loop for player controller
    StartCoroutine(HandleInput());
}
public IEnumerator HandleInput()
{
    //loop forever, reading player input
    while(true)
    {
        while(Mathf.CeilToInt(Input.GetAxis("Vertical")) > 0)
        {
            //set walk
            PlayerState = MOVETYPE.WALK;

            //move player 1 increment
            yield return StartCoroutine(Move(MoveDistance));
        }

        //set to idle
        PlayerState = MOVETYPE.IDLE; //set to idle state - default

        yield return null;
    }
}
public IEnumerator Move(float Increment = 0)
{
    //start position
    Vector3 StartPos = ThisTransform.position;

    //Dest Position
    Vector3 StartPos = ThisTransform.position;

    //Dest position
    Vector3 DestPos = ThisTransform.postition + ThisTransform.forward * Increment;

    //elapsed time
    float ElapsedTime = 0.0f;

    while(ElapsedTime < MoveTime)
    {
        //Calculate interpolated angle
        Vector3 FinalPos = Vector3.Lerp(StartPos, DestPos, ElapsedTime/MoveTime);

        //update pos
        ThisTransform.position = FinalPos;


        //wait until next frame
        yield return null;
        //update time
        ElapsedTime += Time.deltaTime;
    }
    
    //complete move
    ThisTransform.position = DestPos;

    yield break;

}