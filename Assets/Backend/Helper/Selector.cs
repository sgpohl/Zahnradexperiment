using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Selector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool LocallySelected = false;

    protected float ClickedDuration = 0.0f;
    protected float TargetTimeForEffect = 0.1f;
    public float Progress { get { return ClickedDuration / TargetTimeForEffect; } }

    protected abstract bool IsGloballySelected();
    protected abstract void SelectGlobally();
    protected abstract Color Coloring();

    protected float OtherProgress<T>(List<T> Selectors, T CurrentlySelected) where T : Selector
    {
        float otherProgress = 0.0f;
        foreach (T other in Selectors)
        {
            if (other == this)
                continue;
            if (other.Progress > otherProgress)
                otherProgress = other.Progress;
            if (other == CurrentlySelected)
                otherProgress = 1.0f - Progress;
        }
        return otherProgress;
    }

    void Update()
    {
        if (Clicked)
        {
            if (!LocallySelected)
                ClickedDuration += Time.deltaTime;

            if (ClickedDuration > TargetTimeForEffect)
            {
                ClickedDuration = TargetTimeForEffect;
                LocallySelected = true;
                SelectGlobally();
            }
        }

        if (LocallySelected && !IsGloballySelected())
            Deselect();

        if (!Clicked && !LocallySelected)
            ClickedDuration = 0.0f;

        var color = Coloring();
        GetComponent<SpriteRenderer>().color = color;
        foreach (Transform child in transform)
        {
            var s = child.GetComponent<SpriteRenderer>();
            if (s != null)
                s.color = color;
        }
    }
    private bool Clicked = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        Clicked = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Clicked = false;
    }
    /*
    void OnMouseDown()
    {
        Clicked = true;
    }
    void OnMouseUp()
    {
        Clicked = false;
    }
    */

    public void Deselect()
    {
        LocallySelected = false;
    }
}
