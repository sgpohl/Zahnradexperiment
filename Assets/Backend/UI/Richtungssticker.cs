using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Richtungssticker : MonoBehaviour, IPointerUpHandler
{
    public class DragNDrop : DragNDrop<Richtungssticker>  { }

    private Zahnrad AttachedTo;

    public DirectionTrial.Direction Direction;
    DragNDrop<Richtungssticker> Movement;

    private SpriteRenderer sprite;

    Vector3 baseScale;
    Quaternion baseRotation;
    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Movement = gameObject.AddComponent<DragNDrop>();
        Movement.Enabled = true;
        Movement.SelectionCallback = this.CursorSelect;
        Movement.DeselectionCallback = this.CursorDeselect;

        Movement.CursorSelect();
    }

    // Update is called once per frame
    void Update()
    {
        if (AttachedTo != null)
        {
            Vector3 p = AttachedTo.transform.position;
            this.transform.position = new Vector3(p.x, p.y, this.transform.position.z);
            this.transform.rotation = AttachedTo.transform.rotation * baseRotation;


            sprite.sortingOrder = AttachedTo.sprite.sortingOrder + 1;
        }
    }

    private Vector2 SnapToCog(float x, float y)
    {
        Zahnrad below = Experiment.CurrentTrial<CogTrial>().CogAt(new Vector2(x, y));
        if (below == null || below.IsStart)
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


    private void CursorSelect(Vector2 pos)
    {
        sprite.sortingOrder = 4;
        Movement.IsInBounds = delegate { return false; };
    }

    private void CursorDeselect(Vector2 pos)
    {
        AttachedTo = CogBelow();
        if (AttachedTo != null)
        {
            baseRotation = this.transform.rotation;
            baseRotation *= Quaternion.Inverse(AttachedTo.transform.rotation);
            Experiment.CurrentTrial<DirectionTrial>().AttachSticker(this, AttachedTo);
        }
        else
            Destroy(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Movement.OnPointerUp(eventData);
    }
}
