using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragNDrop<T>: MonoBehaviour, IPointerDownHandler, IPointerUpHandler where T : MonoBehaviour
{
    public delegate bool SelectionFilterDelegate(Vector2 pos);
    public delegate void SelectionCallbackDelegate(Vector2 pos);
    public SelectionFilterDelegate IsInBounds = delegate { return true; };
    public SelectionCallbackDelegate SelectionCallback = delegate { };
    public SelectionCallbackDelegate DeselectionCallback = delegate { };
    public float SelectionScale = 1.15f;

    private bool _enabled = true;
    public bool Enabled
    {
        get { return _enabled; }
        set
        {
            if (_enabled && !value)
                CursorDeselect();
            _enabled = value;
        }
    }

    public bool IsSelected { get { return CursorSelected; } }
    public Vector2 CursorScreenPosition { get { return Experiment.Input.mousePosition; } }
    public Vector2 CursorWorldPosition { get { return Camera.main.ScreenToWorldPoint(new Vector3(CursorScreenPosition.x, CursorScreenPosition.y, Camera.main.nearClipPlane)); ; } }

    protected T FunctionalComponent = null;

    protected ITrialFunctionality<T> CurrentTrial { get { return Experiment.CurrentTrial<ITrialFunctionality<T>>(); } }
    private bool CursorSelected = false;
    private Vector2 SelectionOffset;
    private SpriteRenderer sprite;


    void Start()
    {
        FunctionalComponent = GetComponent<T>();

        sprite = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        if (!Enabled)
            return;
        if (CursorSelected)
        {
            Vector3 tpos = CursorWorldPosition;
            Vector2 spos = PositionConstraint(tpos.x + SelectionOffset.x, tpos.y + SelectionOffset.y);
            spos = NearestPositionCandidate(spos);

            if (PositionIsValid(spos))
                transform.position = new Vector3(spos.x, spos.y, transform.position.z);
        }
    }

    protected virtual Vector2 PositionConstraint(float x, float y)
    {
        return new Vector2(x, y);
    }
    private Vector2 NearestPositionCandidate(Vector2 pos)
    {
        return CurrentTrial.NearestPositionCandidate(pos, FunctionalComponent);
    }
    private bool PositionIsValid(Vector2 pos)
    {
        return CurrentTrial.PositionIsValid(pos, FunctionalComponent);
    }


    //Detect when the user clicks the GameObject
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Enabled)
            return;
        CursorSelect();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!Enabled)
            return;
        CursorDeselect();
    }
    /*
    void OnMouseDown()
    {
        if (!Enabled)
            return;
        CursorSelect();
    }
    void OnMouseUp()
    {
        if (!Enabled)
            return;
        CursorDeselect();
    }
    */

    public void CursorSelect()
    {
        Vector2 pos = CursorWorldPosition;
        if (IsInBounds(pos))
        {
            CursorSelected = true;

            SelectionOffset = ((Vector2)gameObject.transform.position) - pos;

            sprite.transform.localScale = Vector3.one * SelectionScale;
            sprite.sortingOrder = 3;

            SelectionCallback(pos);
        }
    }

    public void CursorDeselect()
    {
        if (CursorSelected)
        {
            Vector2 pos = CursorWorldPosition;
            CursorSelected = false;

            sprite.transform.localScale = Vector3.one;
            sprite.sortingOrder = 1;
            DeselectionCallback(pos);
        }
    }
}
