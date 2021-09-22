using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UnpauseButton : PlayButton
{
    void Start()
    {
        Activate();
        //OnPress.AddListener(UnpauseCurrentTrial);
    }

    public void UnpauseCurrentTrial()
    {
        Experiment.CurrentTrial<CogTrial>().IsPaused = false;
    }

    /*
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (invoked)
        {
            Experiment.Instance.ContinueButton.Activate();
            Destroy(transform.root.gameObject);
        }
    }
    */
}