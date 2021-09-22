using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;


public class Zahnrad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public CircleCollider2D InnerCollider { get; private set; }
    public CircleCollider2D OuterCollider { get; private set; }
    public float InnerRadius { get => InnerCollider.radius * transform.lossyScale.x; }
    public float OuterRadius { get => OuterCollider.radius * transform.lossyScale.x; }

    public List<Zahnrad> ConnectedCogs;
    public ConnectedComponent System;
    public bool IsFixedInPlace = false;
    public bool CanRotateManually = true;
    public bool IsStart = false;
    public bool IsTarget = false;
    public string Beschreibung = "";

    public DirectionTrial.Direction Direction = DirectionTrial.Direction.INVALID;
    public SpriteRenderer sprite;

    private Zahnrad.DragNDrop Movement;

    private CogTrial CurrentTrial { get { return Experiment.CurrentTrial<CogTrial>(); } }


    public class DragNDrop : DragNDrop<Zahnrad>
    {
        protected override Vector2 PositionConstraint(float x, float y)
        {
            return SnapToGrid(x, y);
        }

        private Vector2 SnapToGrid(float x, float y)
        {
            var trial = CurrentTrial as CogTrial;
            if (trial.GameBoard != null)
                return trial.GameBoard.SnapToGrid(x, y);
            return new Vector2(x, y);
        }
    }
    public class ConnectedComponent
    {
        private bool _CanRotate;
        public bool CanRotate {
            get
            {
                if (Dirty)
                    CleanUp();
                return _CanRotate;
            }
            private set
            {
                foreach (var cog in Set)
                    cog.System._CanRotate = value;
            }
        }
        Zahnrad parent;
        public ConnectedComponent(Zahnrad p)
        {
            parent = p;
        }

        public Zahnrad Start
        {
            get { return Set.FindLast(cog => cog.IsStart); }
        }
        public List<Zahnrad> Targets
        {
            get { return Set.FindAll(cog => cog.IsTarget); }
        }
        public bool Contains(Zahnrad other)
        {
            return Set.Contains(other);
        }

        private List<Zahnrad> _Path(Zahnrad to)
        {
            if (parent == to)
                return new List<Zahnrad> { parent };
            if (rec_updated)
                return new List<Zahnrad> { };
            rec_updated = true;
            List<Zahnrad> path = new List<Zahnrad> { };
            foreach (var c in parent.ConnectedCogs)
            {
                var path_partial = c.System._Path(to);
                if ((path_partial.Count < path.Count && path_partial.Count > 0) || path.Count == 0)
                {
                    path = path_partial;
                }
            }
            path.Insert(0, parent);
            return path;
        }
        public static int Distance(Zahnrad from, Zahnrad to)
        {
            int d = from.System._Path(to).Count;
            from.System.rec_finished();
            return d;
        }

        bool _set_selector = false;
        private IEnumerable<Zahnrad> RecalculateSet()
        {
            if (_set_selector)
                return new List<Zahnrad> { };
            _set_selector = true;
            IEnumerable<Zahnrad> set = new List<Zahnrad> { };
            foreach (var c in parent.ConnectedCogs)
                set = set.Concat(c.System.RecalculateSet());
            set = set.Append(parent);
            return set;
        }

        private List<Zahnrad> _set;
        private List<Zahnrad> Set
        {
            get
            {
                if (Dirty)
                    CleanUp();
                return _set;
            }
        }

        public int Size
        {
            get { return Set.Count; }
        }


        private bool _dirty = true;
        private bool Dirty
        {
            set
            {
                _dirty = value;
                //dont use accessor! Set is recalculated dependent on dirtyness, so this would result in an endless recursion.
                if (_set == null)
                    return;
                foreach (var c in _set)
                    c.System._dirty = value;
            }
            get => _dirty;
        }
        public void Merge(ConnectedComponent other)
        {
            Dirty = true;
            other.Dirty = true;
        }

        public void Disconnect()
        {
            Dirty = true;
            foreach (Zahnrad c in parent.ConnectedCogs)
                c.ConnectedCogs.Remove(parent);
            parent.ConnectedCogs.Clear();
        }



        private bool rec_updated = false;
        private int _rec_dir = 0;
        private void CleanUp()
        {
            if (!Dirty)
                return;

            _set = RecalculateSet().ToList();
            foreach (var c in _set)
            {
                c.System._set_selector = false;
                c.System._set = _set;
            }
            Dirty = false;

            CanRotate = _TestRotate(1);
            rec_finished();
        }

        private void rec_finished()
        {
            foreach (var cog in Set)
            {
                cog.System._rec_dir = 0;
                cog.System.rec_updated = false;
            }
        }
        private void _SetSpeed(float speed)
        {
            if (rec_updated)
                return;
            rec_updated = true;
            parent.RotationSpeed = speed;
            foreach (var c in parent.ConnectedCogs)
                c.System._SetSpeed(-speed * TranslationFactor(parent, c));
        }
        public void SetSpeed(float speed)
        {
            _SetSpeed(speed);
            rec_finished();
        }

        private void _RotateAll(float angle)
        {
            if (rec_updated)
                return;
            rec_updated = true;
            parent.transform.RotateAround(parent.transform.position, Vector3.forward, angle);
            foreach (var c in parent.ConnectedCogs)
                c.System._RotateAll(-angle * TranslationFactor(parent, c));
        }
        public void RotateAll(float angle)
        {
            _RotateAll(angle);
            rec_finished();
        }

        private bool _TestRotate(int desired)
        {
            if (_rec_dir == desired)
                return true;
            if (_rec_dir != 0)
                return false;
            _rec_dir = desired;
            foreach (var c in parent.ConnectedCogs)
                if (!c.System._TestRotate(-desired))
                    return false;
            return true;
        }
    }

    void Awake()
    {
        Movement = gameObject.AddComponent(typeof(Zahnrad.DragNDrop)) as Zahnrad.DragNDrop;
        Movement.Enabled = !this.IsFixedInPlace;
        Movement.IsInBounds = (Vector2 pos) => { return this.IsInInnerBounds(pos); };
        Movement.SelectionCallback = this.CursorSelect;
        Movement.DeselectionCallback = this.CursorDeselect;

        ConnectedCogs = new List<Zahnrad>();
        System = new ConnectedComponent(this);

        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        if (colliders[0].bounds.extents[0] > colliders[1].bounds.extents[0])
        {
            InnerCollider = colliders[1];
            OuterCollider = colliders[0];
        }
        else
        {
            InnerCollider = colliders[0];
            OuterCollider = colliders[1];
        }

        sprite = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        CurrentTrial.RegisterCog(this);
    }

    // Update is called once per frame
    private float RotationSpeed = 0;
    public float Speed
    {
        get {return RotationSpeed;}
        set 
        {
            System.SetSpeed(value);
        }
    }
    
    public int Size
    {
        get {return (int)(InnerRadius*20/transform.lossyScale.x + 0.5);}
    }
    
    private bool CursorRotating = false;
    private Vector2 RotationAttachmentPoint;
    private float TotalRotation;
    private float AverageRotationSpeed;
    private Vector3 PreRotationAngle;
    private float WiggleAngle;
    private bool previouslyPaused = false;
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

        var trial = Experiment.CurrentTrial<CogTrial>();
        if(trial.IsPaused)
        {
            if(!previouslyPaused)
                Movement.Enabled = false;
            previouslyPaused = true;
            return;
        }
        else if(previouslyPaused)
            Movement.Enabled = !this.IsFixedInPlace;

        previouslyPaused = false;

        if (CursorRotating && OnBoard)
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(new Vector3(Experiment.Input.mousePosition.x, Experiment.Input.mousePosition.y, Camera.main.nearClipPlane));
            Vector2 rotator = OuterCollider.ClosestPoint(mouse);
            float rotation = Vector2.SignedAngle(RotationAttachmentPoint - (Vector2)transform.position, rotator - (Vector2)transform.position);
            if (System.CanRotate)
            {
                RotateAll(rotation);
                TotalRotation += rotation;
                RotationAttachmentPoint = rotator;


                //AverageRotationSpeed = (1-50*Time.deltaTime)*AverageRotationSpeed + 50*Time.deltaTime*rotation;
                AverageRotationSpeed = rotation / Time.deltaTime;
            }
            else
            {
                float d = Random.Range(0.0f, Mathf.Min(Mathf.Abs(rotation) * 0.1f, 2.0f));
                d *= -Mathf.Sign(WiggleAngle);
                RotateAll(d);
                WiggleAngle += d;
            }
        }

        if (System.CanRotate && OnBoard)
        {
            transform.RotateAround(transform.position, Vector3.forward, RotationSpeed * Time.deltaTime);
            RotationSpeed += -RotationSpeed * 1 *Time.deltaTime;
        }
    }

    public bool OnBoard
    {
        get
        {
            if (CurrentTrial.GameBoard == null)
                return false;
            return CurrentTrial.GameBoard.Contains(transform.position.x, transform.position.y);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 pos = Movement.CursorWorldPosition;
        if (IsInInnerBounds(pos))
            return;
        RotationSelect(pos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (CursorRotating)
            RotationDeselect(Movement.CursorWorldPosition);
    }
    /*
    void OnMouseDown()
    {
        Vector2 pos = Movement.CursorWorldPosition;
        if (IsInInnerBounds(pos))
            return;
        RotationSelect(pos);
    }
    void OnMouseUp()
    {
        if (CursorRotating)
            RotationDeselect(Movement.CursorWorldPosition);
    }
    */

    private bool IsInInnerBounds(Vector2 pos)
    {
        return this.InnerCollider.OverlapPoint(pos);
    }

    private void CursorSelect(Vector2 pos)
    {
        Speed = 0;
        Disconnect();
    }
    private void CursorDeselect(Vector2 pos)
    {
        CurrentTrial.ConnectCog(this);
        CurrentTrial.PlacementApplied(this, (Vector2)transform.position);
    }

    private void RotationSelect(Vector2 pos)
    {
        /*
            if (RotationSpeed != 0)
                SetSpeed(0);
            else
                SetSpeed(-30 / InnerRadius.bounds.extents[0]);
            Experiment.Instance.RotationApplied(this, RotationSpeed);
            */
        if (CanRotateManually)
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
    private void RotationDeselect(Vector2 pos)
    {
        CursorRotating = false;
        Speed = AverageRotationSpeed;
        CurrentTrial.RotationApplied(this, TotalRotation);
        if (!System.CanRotate)
            transform.eulerAngles = PreRotationAngle;
    }


    public static float TranslationFactor(Zahnrad from, Zahnrad to)
    {
        return from.InnerRadius / to.InnerRadius;
    }

    public void RotateAll(float angle)
    {
        System.RotateAll(angle);
    }


    public bool Contains(Vector2 pos)
    {
        return Vector2.Distance(pos, transform.position) < InnerRadius;
    }
    public bool Intersects(Zahnrad other)
    {
        //Vector2 v = OuterRadius.ClosestPoint(other.transform.position);
        //return other.OuterRadius.bounds.Contains(v);
        return Vector2.Distance(other.transform.position, transform.position) < (OuterRadius + other.OuterRadius)*0.95;
    }
    public bool Overlaps(Zahnrad other, Vector2 pos)
    {
        return Vector2.Distance(other.transform.position, pos) < (InnerRadius + other.OuterRadius);
    }

    public void ConnectTo(Zahnrad other)
    {
        ConnectedCogs.Add(other);
        System.Merge(other.System);
    }
    
    public void Disconnect()
    {
        System.Disconnect();
    }
}
