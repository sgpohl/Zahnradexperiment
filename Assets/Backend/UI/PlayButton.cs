using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private float ClickedDuration = 0.0f;
    private const float TargetTimeForEffect = 0.7f;

    void Start()
    {
        Experiment.Instance.ContinueButton = this;
        Deactivate();
    }

    void Update()
    {
        if (Clicked && Usable)
        {
            ClickedDuration += Time.deltaTime;

            if (ClickedDuration > TargetTimeForEffect)
            {
                ClickedDuration = 0.0f;
                Experiment.Instance.NextTrial();
            }
        }

        float progress = Mathf.Sqrt(ClickedDuration / TargetTimeForEffect);
        //float progress = ClickedDuration / TargetTimeForEffect;
        //progress *= progress;
        Color tmp = GetComponent<SpriteRenderer>().color;
        if (Usable)
        {
            tmp.r = 1.0f - 1.0f * progress;
            tmp.b = 1.0f - 1.0f * progress;
            tmp.g = 1.0f - 0.3f * progress;
            tmp.a = 0.5f + 0.5f * progress;
        }
        else
        {
            tmp.r = 1.0f;
            tmp.b = 0.2f;
            tmp.g = 0.2f;
            tmp.a = 0.5f;
        }

        GetComponent<SpriteRenderer>().color = tmp;
    }

    private bool Clicked = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        Clicked = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Clicked = false;
        ClickedDuration = 0.0f;
    }
    /*void OnMouseDown()
    {
        Clicked = true;
    }
    void OnMouseUp()
    {
        Clicked = false;
        ClickedDuration = 0.0f;
    }*/

    bool Usable = true;
    public void Enable()
    {
        Usable = true;
    }

    public void Disable()
    {
        Usable = false;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        Enable();
    }
    public void Deactivate()
    {
        //OnMouseUp();
        OnPointerUp(null);
        gameObject.SetActive(false);
    }

}
