using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BauklotzZielgebiet : MonoBehaviour
{
    public Collider2D Collider { get; private set; }
    public int Lösungsnummer = 0;

    void Awake()
    {
        Collider = GetComponent<Collider2D>();
        //Collider.enabled = false;
    }

    private void Start()
    {
        Experiment.CurrentTrial<StabilityTrial>().Register(this);
    }

    public bool ContainsCompletely(Bauklotz target)
    {
        Bounds b = target.Collider.bounds;
        if(!Collider.OverlapPoint(b.min))
            return false;
        if (!Collider.OverlapPoint(b.max))
            return false;
        return true;
    }
}
