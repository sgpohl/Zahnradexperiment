using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auswahlzahnrad : MultiSelector<Auswahlzahnrad>
{
    public int Size;
    public bool IstDieKorrekteLoesung = false;

    void Awake()
    {
        ClickedDuration = 0.0f;
        TargetTimeForEffect = 0.1f;


        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        CircleCollider2D InnerRadius;
        if (colliders[0].bounds.extents[0] > colliders[1].bounds.extents[0])
            InnerRadius = colliders[1];
        else
            InnerRadius = colliders[0];
        Size = (int)(InnerRadius.bounds.extents[0] * 20 + 0.5);
    }
    /*
    void Start()
    {
        Experiment.CurrentTrial<SpeedTrial>().RegisterCogSelector(this);
    }

    protected override bool IsGloballySelected()
    {
        return (Experiment.CurrentTrial<SpeedTrial>().SelectedCog == this);
    }
    protected override void SelectGlobally()
    {
        Experiment.CurrentTrial<SpeedTrial>().SelectCog(this);
    }

    protected override Color Coloring()
    {
        var trial = Experiment.CurrentTrial<SpeedTrial>();
        float h = 0.0f;
        float s = 0.0f;
        float v = 0.7f + 0.3f * Progress - OtherProgress(trial.CogSelectors, trial.SelectedCog) * 0.4f;
        return Color.HSVToRGB(h, s, v);
    }
    */
}
