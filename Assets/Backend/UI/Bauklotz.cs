using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bauklotz : MonoBehaviour
{
    public bool IsFixedInPlace = false;
    public Collider2D Collider { get; private set; }

    private Rigidbody2D PhysicsBody;
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Color defaultColor;

    public class DragNDrop : DragNDrop<Bauklotz>
    {

    }
    private Bauklotz.DragNDrop Movement;

    private StabilityTrial CurrentTrial { get { return Experiment.CurrentTrial<StabilityTrial>(); } }

    private void Awake()
    {
        Collider = GetComponent<Collider2D>();
        PhysicsBody = GetComponent<Rigidbody2D>();

        Movement = gameObject.AddComponent(typeof(Bauklotz.DragNDrop)) as Bauklotz.DragNDrop;

        Movement.Enabled = !this.IsFixedInPlace;
        Movement.IsInBounds = (Vector2 pos) => { return this.Collider.OverlapPoint(pos); };
        Movement.SelectionCallback = NotifyOfSelection;
        Movement.DeselectionCallback = NotifyOfDeselection;

        Movement.SelectionScale = 1.05f;
    }

    void Start()
    {
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
        defaultColor = GetComponent<SpriteRenderer>().color;

        CurrentTrial.Register(this);
    }

    public void UnlockMovement()
    {
        if (PhysicsBody.bodyType == RigidbodyType2D.Static)
            return;
        PhysicsBody.gravityScale = 7;
        PhysicsBody.isKinematic = false;
        PhysicsBody.freezeRotation = false;
        PhysicsBody.useAutoMass = false;
        PhysicsBody.mass = 3;

        Movement.Enabled = false;
    }

    public void LockMovement()
    {
        if (PhysicsBody.bodyType == RigidbodyType2D.Static)
            return;
        PhysicsBody.freezeRotation = true;
        PhysicsBody.gravityScale = 0;
        PhysicsBody.velocity = Vector2.zero;
        PhysicsBody.angularVelocity = 0;

        if (this.IsFixedInPlace)
            PhysicsBody.isKinematic = true;
        else
            Movement.Enabled = true;
    }

    public void Hide()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public void Show()
    {
        GetComponent<SpriteRenderer>().enabled = true;
    }

    public void ResetPosition()
    {
        PhysicsBody.position = defaultPosition;
        transform.rotation = defaultRotation;
    }
    public void NotifyOfSelection(Vector2 pos)
    {
        CurrentTrial.SolutionBlockSelected(this);
    }
    public void NotifyOfDeselection(Vector2 pos)
    {
        CurrentTrial.SolutionBlockReleased(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsFixedInPlace)
            return;

        if (!Experiment.Instance.TestMode)
            return;
        if(CurrentTrial.IsCorrectlyPlaced(this))
        {
            Color tmp = GetComponent<SpriteRenderer>().color;
            tmp.r = 0.5f;
            tmp.b = 0.5f;
            tmp.g = 1.0f;
            tmp.a = 1.0f;
            GetComponent<SpriteRenderer>().color = tmp;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = defaultColor;
        }
    }

    public bool Overlaps(Bauklotz other, Vector2 pos)
    {
        /*
        //var p = Collider.transform.position;
        //Collider.transform.position = pos - (Vector2)transform.position;
        bool colliding = Collider.IsTouching(other.Collider); ;
        //Collider.transform.position = p;
        return colliding;
        */
        return Collider.Distance(other.Collider).distance < 0;
    }

    /*
    public Vector2 ClosestPoint(Vector2 pos, Vector2 target)
    {
        //var p = Collider.transform.position;
        //Collider.transform.position = pos - (Vector2)transform.position;
        var point = Collider.ClosestPoint(target);
        //Collider.transform.position = p;
        return point;
    }*/
}
