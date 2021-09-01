using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auswahlsymbol : MultiSelector<Auswahlsymbol>
{
    public ITrial.AnswerType IstDieKorrekteLoesung = ITrial.AnswerType.UNDEFINED;
    public int Nummer = -1;
    public Color Grundfarbe = Color.white;
    public Color Auswahlfarbe = Color.white;

    protected override Color Coloring()
    {
        Color diff = Auswahlfarbe - Grundfarbe;
        Color tmp = GetComponent<SpriteRenderer>().color;
        tmp = Grundfarbe + Progress * diff;
        return tmp;
    }
}
