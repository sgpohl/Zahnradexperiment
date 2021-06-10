using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auswahlzahnrad : MonoBehaviour
{
    public int Size;
    private bool Selected = false;

    private float ClickedDuration = 0.0f;
    private const float TargetTimeForEffect = 0.2f;

    public float Progress { get { return ClickedDuration / TargetTimeForEffect;  } }

    void Awake()
    {
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        CircleCollider2D InnerRadius;
        if (colliders[0].bounds.extents[0] > colliders[1].bounds.extents[0])
            InnerRadius = colliders[1];
        else
            InnerRadius = colliders[0];
        Size = (int)(InnerRadius.bounds.extents[0] * 20 + 0.5);
    }
    void Start()
    {
        Experiment.CurrentTrial<SpeedTrial>().RegisterCogSelector(this);
    }

    void Update()
    {
        if (Clicked)
        {
            if (!Selected)
                ClickedDuration += Time.deltaTime;

            if (ClickedDuration > TargetTimeForEffect)
            {
                ClickedDuration = TargetTimeForEffect;
                Selected = true;
                Experiment.CurrentTrial<SpeedTrial>().SelectCog(this);
            }
        }

        bool OtherSelected = (Experiment.CurrentTrial<SpeedTrial>().SelectedCog != this && Experiment.CurrentTrial<SpeedTrial>().SelectedCog != null);
        if (Selected && OtherSelected)
            Deselect();

        if (!Clicked && !Selected)
            ClickedDuration = 0.0f;


        float otherProgress = 0.0f;
        foreach(Auswahlzahnrad other in Experiment.CurrentTrial<SpeedTrial>().CogSelectors)
        {
            if (other == this)
                continue;
            if (other.Progress > otherProgress)
                otherProgress = other.Progress;
            if (other == Experiment.CurrentTrial<SpeedTrial>().SelectedCog)
                otherProgress = 1.0f-Progress;
        }

        float h = 0.0f;
        float s = 0.0f ;
        float v = 0.7f + 0.3f * Progress - otherProgress * 0.4f;
        GetComponent<SpriteRenderer>().color = Color.HSVToRGB(h, s, v);
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
