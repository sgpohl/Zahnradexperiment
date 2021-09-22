using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PauseButton : PlayButton
{
    public GameObject UnpausePrefab = null;
    public Color AbgelaufeneZeitFarbe = new Color(1.0f, 1.0f, 0.35f, 0.9f);
    public float MinutenBisPause = 3.0f;

    class Timer : MonoBehaviour
    {
        private System.Diagnostics.Stopwatch t;
        private long ms;
        public delegate void Callback();
        public Callback Finished = delegate { };
        public delegate void CallbackFloat(float p);
        public CallbackFloat OnProgress = delegate { };

        public void SetTargetTime(float min)
        {
            ms = (long)(min * 60 * 1000);
        }

        public void Start()
        {
            t = new System.Diagnostics.Stopwatch();
            t.Start();
        }

        public void Update()
        {
            if (t.ElapsedMilliseconds >= ms)
            {
                Finished();
                Destroy(this);
            }
            else
            {
                OnProgress(((float)t.ElapsedMilliseconds) / ((float)ms));
            }
        }
    }

    public void StartTimer()
    {
        var timer = gameObject.AddComponent<Timer>();
        timer.SetTargetTime(MinutenBisPause);
        timer.Finished = this.TimeExpired;
        timer.OnProgress = TimerProgress;

        var rend = this.GetComponent<SpriteRenderer>();
        rend.material.SetColor("_PColor", AbgelaufeneZeitFarbe);
        rend.material.SetFloat("_Progress", 0.0f);
    }

    public void TimeExpired()
    {
        this.Grundfarbe = AbgelaufeneZeitFarbe;

        var rend = this.GetComponent<SpriteRenderer>();
        rend.material.SetFloat("_Progress", 0.0f);
    }

    public void TimerProgress(float progress)
    {
        var rend = this.GetComponent<SpriteRenderer>();
        rend.material.SetFloat("_Progress", progress);
        rend.material.SetColor("_BColor", rend.color);
    }

    public void PauseCurrentTrial()
    {
        Experiment.CurrentTrial<CogTrial>().IsPaused = true;
    }

    public void OpenUnpauseDialogue()
    {
        Instantiate(UnpausePrefab);
    }

}
