using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auswahlpfeil : MonoBehaviour
{
    public DirectionTrial.Direction Richtung;
    public bool IstDieKorrekteLoesung = false;
    private bool Selected = false;

    private float ClickedDuration = 0.0f;
    private const float TargetTimeForEffect = 0.1f;


    void Update()
    {
        if (Clicked)
        {
            if(!Selected)
                ClickedDuration += Time.deltaTime;

            if (ClickedDuration > TargetTimeForEffect)
            {
                ClickedDuration = TargetTimeForEffect;
                Selected = true;
                Experiment.CurrentTrial<DirectionTrial>().SelectDirection(Richtung, IstDieKorrekteLoesung);
            }
        }

        if (Selected && Experiment.CurrentTrial<DirectionTrial>().SelectedDirection != Richtung)
            Deselect();

        if(!Clicked && !Selected)
            ClickedDuration = 0.0f;

        float progress = ClickedDuration / TargetTimeForEffect;
        Color tmp = GetComponent<SpriteRenderer>().color;
        tmp.r = 1.0f - 1.0f * progress;
        tmp.b = 1.0f - 1.0f * progress;
        tmp.g = 1.0f + 0.0f * progress;
        tmp.a = 0.7f + 0.3f * progress;
        GetComponent<SpriteRenderer>().color = tmp;
    }
    private bool Clicked = false;
    void OnMouseDown()
    {
        Clicked = true;
    }
    void OnMouseUp()
    {
        Clicked = false;
    }

    public void Deselect()
    {
        Selected = false;
    }
}
