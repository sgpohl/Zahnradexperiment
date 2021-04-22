using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ablage : MonoBehaviour
{
    public Texture2D icon;
    
    void OnGUI () 
    {
        if (GUI.Button (new Rect (Screen.width-200,0,200,Screen.height), icon)) 
        {
            print ("clicked");
        }
    }
}
