using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zahnrad : MonoBehaviour
{
    private CircleCollider2D InnerRadius;
    private CircleCollider2D OuterRadius;

    public List<Zahnrad> ConnectedCogs;
    public bool IsFixedInPlace = false;
    public bool CanRotateManually = true;
    public bool IsStart = false;
    public bool IsTarget = false;
    public int Direction = 0;
    private SpriteRenderer sprite;

    void Awake()
    {
        ConnectedCogs = new List<Zahnrad>();

        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        if (colliders[0].bounds.extents[0] > colliders[1].bounds.extents[0])
        {
            InnerRadius = colliders[1];
            OuterRadius = colliders[0];
        }
        else
        {
            InnerRadius = colliders[0];
            OuterRadius = colliders[1];
        }

    }
    void Start()
    {
        Experiment.Instance.RegisterCog(this);

        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private float RotationSpeed = 0;
    public float Speed
    {
        get {return RotationSpeed;}
        set 
        { 
            _SetSpeed(value);
            rec_finished();
        }
    }
    
    public int Size
    {
        get {return (int)(InnerRadius.bounds.extents[0]*20 + 0.5);}
    }
    
    private bool CursorSelected = false;
    private Vector2 SelectionOffset;
    
    private bool CursorRotating = false;
    private Vector2 RotationAttachmentPoint;
    private float TotalRotation;
    private float AverageRotationSpeed;
    private Vector3 PreRotationAngle;
    private float WiggleAngle;
    void Update()
    {
        /*
        if(Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            switch(touch.phase)
            {
                case TouchPhase.Began:
                    CursorSelect(touch.position);
                    break;
                case TouchPhase.Moved:
                    transform.position = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane));
                    break;
                case TouchPhase.Ended:
                    CursorDeselect(touch.position);
                    break;
            }
        }*/

        if (CursorSelected)
        {
            Vector3 tpos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            Vector2 spos = SnapToGrid(tpos.x + SelectionOffset.x, tpos.y + SelectionOffset.y);
            
            if(Experiment.Instance.PositionIsValid(spos, this))
                transform.position = new Vector3(spos.x, spos.y, transform.position.z);
        }
        
        if(CursorRotating)
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            Vector2 rotator = OuterRadius.ClosestPoint(mouse);
            float rotation = Vector2.SignedAngle(RotationAttachmentPoint - (Vector2)transform.position, rotator - (Vector2)transform.position);
            if (CanRotate)
            {
                RotateAll(rotation);
                TotalRotation += rotation;
                RotationAttachmentPoint = rotator;

                AverageRotationSpeed = rotation / Time.deltaTime;//(1-5*Time.deltaTime)*AverageRotationSpeed + 5*Time.deltaTime*rotation;
            }
            else
            {
                float d = Random.Range(0.0f, Mathf.Min(Mathf.Abs(rotation) * 0.1f, 2.0f));
                d *= -Mathf.Sign(WiggleAngle);
                RotateAll(d);
                WiggleAngle += d;
            }
        }

        if (CanRotate)
            transform.RotateAround(transform.position, Vector3.forward, RotationSpeed * Time.deltaTime);
    }

    private Vector2 SnapToGrid(float x, float y)
    {
        if (Experiment.Instance.GameBoard != null)
            return Experiment.Instance.GameBoard.SnapToGrid(x, y);
        return new Vector2(x, y);
    }

    //Detect when the user clicks the GameObject
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
        if(InnerRadius.OverlapPoint(pos))
        {
            if (!IsFixedInPlace)
            {
                SelectionOffset = ((Vector2)transform.position) - pos;
                Speed = 0;
                Disconnect();
                CursorSelected = true;
                transform.localScale = Vector3.one * 1.15f;

                sprite.sortingOrder = 3;
            }
        }
        else
        {
            /*
            if (RotationSpeed != 0)
                SetSpeed(0);
            else
                SetSpeed(-30 / InnerRadius.bounds.extents[0]);
            Experiment.Instance.RotationApplied(this, RotationSpeed);
            */
            if(CanRotateManually)
            {
                Speed = 0;
                TotalRotation = 0;
                AverageRotationSpeed = 0;
                RotationAttachmentPoint = pos;
                CursorRotating = true;
                PreRotationAngle = transform.eulerAngles;
                WiggleAngle = 0;
            }
        }
    }
    
    private void CursorDeselect(Vector2 pos)
    {
        if(CursorSelected)
        {
            CursorSelected = false;
            Experiment.Instance.ConnectCog(this);
            Experiment.Instance.PlacementApplied(this, (Vector2)transform.position);

            transform.localScale = Vector3.one;
            sprite.sortingOrder = 1;
        }
        if(CursorRotating)
        {
            CursorRotating = false;
            Speed = AverageRotationSpeed;
            Experiment.Instance.RotationApplied(this, TotalRotation);
            if(!CanRotate)
                transform.eulerAngles = PreRotationAngle;
        }
    }
    
    public static float TranslationFactor(Zahnrad from, Zahnrad to)
    {
        return from.InnerRadius.bounds.extents[0] / to.InnerRadius.bounds.extents[0];
    }

    private bool rec_updated = false;
    private int _rec_dir = 0;
    private void rec_finished()
    {
        if (!rec_updated)
            return;
        rec_updated = false;
        _rec_dir = 0;
        foreach (var c in ConnectedCogs)
            c.rec_finished();
    }
    private void _SetSpeed(float speed)
    {
        if (rec_updated)
            return;
        rec_updated = true;
        RotationSpeed = speed;
        foreach(var c in ConnectedCogs)
            c._SetSpeed(-speed * TranslationFactor(this, c));
    }
    
    public void _RotateAll(float angle)
    {
        if (rec_updated)
            return;
        rec_updated = true;
        transform.RotateAround(transform.position, Vector3.forward, angle);
        foreach(var c in ConnectedCogs)
            c._RotateAll(-angle * TranslationFactor(this, c));
    }
    public void RotateAll(float angle)
    {
        _RotateAll(angle);
        rec_finished();
    }

    private bool CanRotate;
    public bool _TestRotate(int desired)
    {
        if (_rec_dir == desired)
            return true;
        if (_rec_dir != 0)
            return false;
        _rec_dir = desired;
        foreach (var c in ConnectedCogs)
            if (!c._TestRotate(-desired))
                return false;
        return true;
    }
    private void _PropagateCanRotatate(bool b)
    {
        if (rec_updated)
            return;
        rec_updated = true;
        CanRotate = b;
        foreach (var c in ConnectedCogs)
            c._PropagateCanRotatate(b);
    }
    public void TestRotate()
    {
        _PropagateCanRotatate(_TestRotate(1));
        rec_finished();
    }

    public bool Intersects(Zahnrad other)
    {
        Vector2 v = OuterRadius.ClosestPoint(other.transform.position);
        return other.OuterRadius.bounds.Contains(v);
    }
    public bool Overlaps(Zahnrad other, Vector2 pos)
    {
        return Vector2.Distance(other.transform.position, pos) < (InnerRadius.radius + other.OuterRadius.radius);
    }

    public void ConnectTo(Zahnrad other)
    {
        ConnectedCogs.Add(other);
        TestRotate();
    }
    
    public void Disconnect()
    {
        foreach(Zahnrad c in ConnectedCogs)
        {
            c.ConnectedCogs.Remove(this);
            c.TestRotate();
        }
        ConnectedCogs.Clear();
        TestRotate();
    }
}
