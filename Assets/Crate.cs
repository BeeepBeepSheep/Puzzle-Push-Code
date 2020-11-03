using UnityEngine;
using System.Collections;
//--------------------------------------
public class Crate : MonoBehaviour
{
    //Indicates whether crate is on a destination space
    public bool bIsOnDestination = false;

    //--------------------------------------
    //When box is pushed onto destination
    void OnTriggerStay(Collider Other)
    {
        if (Other.CompareTag("End"))
            bIsOnDestination = true;
    }
    //--------------------------------------
    void OnTriggerExit(Collider Other)
    {
        if (Other.CompareTag("End"))
            bIsOnDestination = false;
    }
    //--------------------------------------
}