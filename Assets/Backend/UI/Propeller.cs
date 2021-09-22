using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Propeller : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Zahnrad AttachedTo;

    public CircleCollider2D InnerRadius { get; private set; }
    public float OuterRadius { get; private set; }
    private SpriteRenderer sprite;

    Vector3 baseScale;
    Quaternion baseRotation;
    void Awake()
    {
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        CircleCollider2D OuterCollider;
        if (colliders[0].bounds.extents[0] > colliders[1].bounds.extents[0])
        {
            InnerRadius = colliders[1];
            OuterCollider = colliders[0];
        }
        else
        {
            InnerRadius = colliders[0];
            OuterCollider = colliders[1];
        }
        OuterRadius = OuterCollider.radius * transform.localScale.x;
        Destroy(OuterCollider);

        sprite = GetComponent<SpriteRenderer>();
        baseScale = this.transform.localScale;
    }

    void Start()
    {
        Experiment.CurrentTrial<PropellerTrial>().RegisterPropeller(this);
    }

    // Update is called once per frame
    void Update()
    {
        var trial = Experiment.CurrentTrial<CogTrial>();
        if (trial.IsPaused)
            return;

        if (CursorSelected)
        {
            Vector3 tpos = Camera.main.ScreenToWorldPoint(new Vector3(Experiment.Input.mousePosition.x, Experiment.Input.mousePosition.y, Camera.main.nearClipPlane));
            Vector2 spos = SnapToCog(tpos.x + SelectionOffset.x, tpos.y + SelectionOffset.y);

            transform.position = new Vector3(spos.x, spos.y, transform.position.z);
        }
        else if (AttachedTo != null)
        {
            Vector3 p = AttachedTo.transform.position;
            this.transform.position = new Vector3(p.x, p.y, this.transform.position.z);
            this.transform.rotation = AttachedTo.transform.rotation * baseRotation;


            sprite.sortingOrder = AttachedTo.sprite.sortingOrder + 1;
        }
    }

    private Vector2 SnapToCog(float x, float y)
    {
        Zahnrad below = Experiment.CurrentTrial<CogTrial>().CogAt(new Vector2(x,y));
        if(below == null || below.IsStart)
            return new Vector2(x, y);
        return below.transform.position;
    }

    private Zahnrad CogBelow()
    {
        Zahnrad cog = Experiment.CurrentTrial<CogTrial>().CogAt(this.transform.position);
        if (cog == null || cog.IsStart)
            return null;
        return cog;
    }


    private bool CursorSelected = false;
    private Vector2 SelectionOffset;
    public void OnPointerDown(PointerEventData eventData)
    {
        var trial = Experiment.CurrentTrial<CogTrial>();
        if (trial.IsPaused)
            return;
        CursorSelect(eventData.position);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        CursorDeselect(eventData.position);
    }
    /*
    void OnMouseDown()
    {
        CursorSelect(Experiment.Input.mousePosition);
    }
    void OnMouseUp()
    {
        CursorDeselect(Experiment.Input.mousePosition);
    }
    */

    private void CursorSelect(Vector2 pos)
    {
        pos = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, Camera.main.nearClipPlane));
        if (InnerRadius.OverlapPoint(pos))
        {
            SelectionOffset = ((Vector2)transform.position) - pos;
            Disconnect();
            CursorSelected = true;
            transform.localScale = baseScale * 1.15f;

            sprite.sortingOrder = 4;
        }
    }

    private void CursorDeselect(Vector2 pos)
    {
        if (CursorSelected)
        {
            CursorSelected = false;
            transform.localScale = baseScale;
            sprite.sortingOrder = 2;
            AttachedTo = CogBelow();
            baseRotation = this.transform.rotation;
            if (AttachedTo != null)
                baseRotation *= Quaternion.Inverse(AttachedTo.transform.rotation);

            Experiment.CurrentTrial<PropellerTrial>().AttachPropeller(AttachedTo);
        }
    }

    private void Disconnect()
    {
        Experiment.CurrentTrial<PropellerTrial>().DetachPropeller(AttachedTo);
        AttachedTo = null;
    }


    public bool Intersects(Propeller other)
    {
        return Vector2.Distance( other.transform.position, transform.position) < (OuterRadius + other.OuterRadius);
    }
}
