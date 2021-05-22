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

    public Vector2 SnapToGrid(float x, float y)
    {
        if (Rastergroesse == 0.0f)
            return new Vector2(x, y);
        if (!spRenderer.bounds.Contains(new Vector3(x, y, 0)))
            return new Vector2(x, y);

        float xo = spRenderer.bounds.extents.x % Rastergroesse;
        float yo = spRenderer.bounds.extents.y % Rastergroesse;

        float g = Rastergroesse;
        x += g / 2 +xo;
        y += g / 2;
        int ix = (int)(x / g);
        if (x < 0)
            ix--;
        int iy = (int)(y / g);
        if (y < 0)
            iy--;

        return new Vector2(ix * Rastergroesse -xo, iy * Rastergroesse);
    }

    private void Init()
    {
        if(spRenderer == null)
            spRenderer = GetComponent<SpriteRenderer>();
        if (Rastergroesse > 0)
        {
            float xo = spRenderer.bounds.extents.x % Rastergroesse;
            float yo = spRenderer.bounds.extents.y % Rastergroesse;
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
        o.transform.localScale *= Rastergroesse*0.15f;

        SpriteRenderer renderer = o.AddComponent<SpriteRenderer>();
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
