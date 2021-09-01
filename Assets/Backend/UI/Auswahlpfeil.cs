using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auswahlpfeil : MultiSelector<Auswahlpfeil>
{
    public DirectionTrial.Direction Richtung;
    public bool IstDieKorrekteLoesung = false;

    private void Awake()
    {
        ClickedDuration = 0.0f;
        TargetTimeForEffect = 0.1f;
    }

    protected override Color Coloring()
    {
        Color tmp = GetComponent<SpriteRenderer>().color;
        tmp.r = 1.0f - 1.0f * Progress;
        tmp.b = 1.0f - 1.0f * Progress;
        tmp.g = 1.0f + 0.0f * Progress;
        tmp.a = 0.7f + 0.3f * Progress;
        return tmp;
    }
}
