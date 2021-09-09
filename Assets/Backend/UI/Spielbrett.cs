using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Spielbrett : MonoBehaviour
{
    public float Rastergroesse = 0.0f;
    public Sprite sprite;
    // Start is called before the first frame update
    private SpriteRenderer spRenderer;
    void Start()
    {
        spRenderer = GetComponent<SpriteRenderer>();
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying)
        {
            Clear();
            Init();
        }
        
    }

    public bool Contains(float x, float y)
    {
        return spRenderer.bounds.Contains(new Vector3(x, y, 0));
    }

    public Vector2 SnapToGrid(float x, float y)
    {
        if (Rastergroesse == 0.0f)
            return new Vector2(x, y);
        if (!Contains(x, y))
            return new Vector2(x, y);


        float xo = (spRenderer.bounds.size.x % Rastergroesse) / 2;
        float yo = (spRenderer.bounds.size.y % Rastergroesse) / 2;
        Vector2 frameOffset = new Vector2(xo, yo);
        Vector2 boardOffset = transform.position - spRenderer.bounds.size / 2;
        boardOffset += frameOffset;

        float g = Rastergroesse;
        x += 0 / 2 - boardOffset.x;
        y += 0 / 2 - boardOffset.y;
        int ix = (int)(x / g);
        if (x < 0)
            ix--;
        int iy = (int)(y / g);
        if (y < 0)
            iy--;

        float innerCellOffset = 0.5f;
        Vector2 cellMidpoint = new Vector2((ix + innerCellOffset) * Rastergroesse, (iy + innerCellOffset) * Rastergroesse);
        return cellMidpoint + boardOffset;
    }

    private void Init()
    {
        if(spRenderer == null)
            spRenderer = GetComponent<SpriteRenderer>();
        if (Rastergroesse > 0)
        {
            float xo = (spRenderer.bounds.size.x % Rastergroesse) / 2;
            float yo = (spRenderer.bounds.size.y % Rastergroesse) / 2;
            for (float x = spRenderer.bounds.min.x+xo; x < spRenderer.bounds.max.x; x += Rastergroesse)
                for (float y = spRenderer.bounds.min.y+yo; y < spRenderer.bounds.max.y; y += Rastergroesse)
                    AddPoint(x, y);
        }
    }

    private void AddPoint(float x, float y)
    {
        GameObject o = new GameObject("Point");


        o.transform.parent = this.transform;
        o.transform.position = new Vector3(x, y, 0);
        o.transform.localScale *= Rastergroesse * 0.20f;

        SpriteRenderer renderer = o.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 0;
        /*
        Color tmp = renderer.color;
        tmp.r = 0.4f;
        tmp.b = 0.4f;
        tmp.g = 0.4f;
        tmp.a = 1.0f;
        renderer.color = tmp;
        */
        renderer.sprite = sprite;
    }

    private void Clear()
    {
        Transform p = transform.Find("Point");
        while (p != null)
        {
            DestroyImmediate(p.gameObject);
            p = transform.Find("Point");
        }
    }
}
