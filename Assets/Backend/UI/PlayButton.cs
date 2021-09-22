using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PlayButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private float ClickedDuration = 0.0f;
    public float TargetTimeForEffect = 0.4f;
    //public delegate void ExecutionFunction();
    //public ExecutionFunction execute;
    public UnityEvent OnPress;
    public UnityEvent OnRelease;
    public UnityEvent OnStart;

    public Color Grundfarbe = new Color(1, 1, 1, 0.7f);
    public Color Auswahlfarbe = new Color(0, 0.7f, 0, 1);
    public Color InaktiveFarbe = new Color(1, 0.2f, 0.2f, 0.5f);

    void Awake()
    {
        if (OnPress == null)
            OnPress = new UnityEvent();
        if (OnStart == null)
            OnStart = new UnityEvent();
        if (OnRelease == null)
            OnRelease = new UnityEvent();
    }

    void Start()
    {
        //execute = Experiment.Instance.NextTrial;
        //Deactivate();
        OnStart.Invoke();
    }

    public void SetAsGlobal()
    {
        Experiment.Instance.ContinueButton = this;
    }

    public void ContinueToNextTrial()
    {
        Experiment.Instance.NextTrial();
    }

    public void DestroyParent()
    {
        Destroy(transform.root.gameObject);
    }

    public void EnableGlobal(bool enable)
    {
        if (enable)
            Experiment.Instance.ContinueButton.Activate();
        else
            Experiment.Instance.ContinueButton.Deactivate();
    }

    void OnValidate()
    {
        GetComponent<SpriteRenderer>().color = Grundfarbe;
    }

    void Update()
    {
        if (Clicked && Usable)
        {
            ClickedDuration += Time.deltaTime;

            if (ClickedDuration >= TargetTimeForEffect)
            {
                ClickedDuration = 0.0f;
                invoked = true;
                OnPress.Invoke();
            }
        }

        float progress = Mathf.Sqrt(ClickedDuration / TargetTimeForEffect);
        //float progress = ClickedDuration / TargetTimeForEffect;
        //progress *= progress;
        Color tmp = GetComponent<SpriteRenderer>().color;
        if (Usable)
        {
            /*
            tmp.r = 1.0f - 1.0f * progress;
            tmp.b = 1.0f - 1.0f * progress;
            tmp.g = 1.0f - 0.3f * progress;
            tmp.a = 0.7f + 0.3f * progress;
            */
            tmp = Grundfarbe * (1.0f - progress) + Auswahlfarbe * progress;
        }
        else
        {
            /*
            tmp.r = 1.0f;
            tmp.b = 0.2f;
            tmp.g = 0.2f;
            tmp.a = 0.5f;
            */
            tmp = InaktiveFarbe;
        }

        GetComponent<SpriteRenderer>().color = tmp;
    }

    private bool Clicked = false;
    private bool invoked = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        Clicked = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Clicked = false;
        ClickedDuration = 0.0f;

        if(invoked)
            OnRelease.Invoke();
        invoked = false;
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
