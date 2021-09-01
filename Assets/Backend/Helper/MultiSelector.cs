using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSelector<T> : Selector
    where T : Selector
{
    protected override bool IsGloballySelected()
    {
        return Experiment.CurrentTrial<ISelectorTrial<T>>().GetCurrentlySelected() == this;
    }
    protected override void SelectGlobally()
    {
        Experiment.CurrentTrial<ISelectorTrial<T>>().SelectAnswer(this as T);
    }

    private void Awake()
    {
        ClickedDuration = 0.0f;
        TargetTimeForEffect = 0.1f;
    }
    void Start()
    {
        Experiment.CurrentTrial<ISelectorTrial<T>>().Register(this as T);
    }

    protected override Color Coloring()
    {
        var trial = Experiment.CurrentTrial<ISelectorTrial<T>>();
        float h = 0.0f;
        float s = 0.0f;
        float v = 0.7f + 0.3f * Progress - OtherProgress(trial.GetSelectors(), trial.GetCurrentlySelected()) * 0.4f;
        return Color.HSVToRGB(h, s, v);
    }
}
