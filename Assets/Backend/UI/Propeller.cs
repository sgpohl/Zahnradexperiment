using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : MonoBehaviour
{
    private Zahnrad AttachedTo;

    public CircleCollider2D InnerRadius { get; private set; }
    private SpriteRenderer sprite;

    Vector3 baseScale;
    Quaternion baseRotation;
    void Awake()
    {
        InnerRadius = GetComponent<CircleCollider2D>();

        sprite = GetComponent<SpriteRenderer>();
        baseScale = this.transform.localScale;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (CursorSelected)
        {
            Vector3 tpos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
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
    void OnMouseDown()
    {
        CursorSelect(Input.mousePosition);
    }
    void OnMouseUp()
    {
        CursorDeselect(Input.mousePosition);
    }

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
}
