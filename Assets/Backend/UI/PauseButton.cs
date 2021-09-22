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
            if(t.ElapsedMilliseconds >= ms)
            {
                Finished();
                Destroy(this);
            }
        }
    }

    public void StartTimer()
    {
        var timer = gameObject.AddComponent<Timer>();
        timer.SetTargetTime(MinutenBisPause);
        timer.Finished = this.TimeExpired;
    }

    public void TimeExpired()
    {
        this.Grundfarbe = AbgelaufeneZeitFarbe;
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
