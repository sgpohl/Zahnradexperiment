using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BauklotzZielgebiet : MonoBehaviour
{
    public BoxCollider2D Collider { get; private set; }

    void Awake()
    {
        Collider = GetComponent<BoxCollider2D>();
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
