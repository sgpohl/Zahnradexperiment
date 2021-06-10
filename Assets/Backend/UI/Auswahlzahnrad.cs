using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auswahlzahnrad : MonoBehaviour
{
    public int Size;
    private bool Selected = false;

    private float ClickedDuration = 0.0f;
    private const float TargetTimeForEffect = 0.1f;


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
        //Experiment.Instance.RegisterCog(this);
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
                //Experiment.Instance.SelectCog(this);
            }
        }

        //if (Selected && Experiment.Instance.SelectedCog != this)
        //    Deselect();

        if (!Clicked && !Selected)
            ClickedDuration = 0.0f;

        float progress = ClickedDuration / TargetTimeForEffect;
        Color tmp = GetComponent<SpriteRenderer>().color;
        tmp.r = 1.0f - 1.0f * progress;
        tmp.b = 1.0f - 1.0f * progress;
        tmp.g = 1.0f + 0.0f * progress;
        tmp.a = 0.5f + 0.5f * progress;
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
