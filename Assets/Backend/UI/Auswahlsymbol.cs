using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auswahlsymbol : MultiSelector<Auswahlsymbol>
{
    public ITrial.AnswerType IstDieKorrekteLoesung = ITrial.AnswerType.UNDEFINED;
    public int Nummer = -1;
    public Color Grundfarbe = Color.white;
    public Color Auswahlfarbe = Color.white;




    
    //public AudioSource audioData;

    //void Start()
    //{
    //    audioData = GetComponent<AudioSource>();
    //    audioData.Play(0);
    //    Debug.Log("started");
    //}

    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(10, 70, 150, 30), "Pause"))
    //    {
    //        audioData.Pause();
    //        Debug.Log("Pause: " + audioData.time);
    //    }

    //    if (GUI.Button(new Rect(10, 170, 150, 30), "Continue"))
    //    {
    //        audioData.UnPause();
    //    }
    //}
    







    protected override Color Coloring()
    {
        Color diff = Auswahlfarbe - Grundfarbe;
        Color tmp = GetComponent<SpriteRenderer>().color;
        tmp = Grundfarbe + Progress * diff;
        return tmp;
    }
}
