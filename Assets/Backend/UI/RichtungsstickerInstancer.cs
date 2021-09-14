using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class RichtungsstickerInstancer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public DirectionTrial.Direction Richtung = DirectionTrial.Direction.CW;

    private Richtungssticker CurrentSticker = null;

    public void OnPointerDown(PointerEventData eventData)
    {
        var renderer = gameObject.GetComponent<SpriteRenderer>();

        GameObject go = new GameObject(Richtung.ToString());
        go.transform.position = gameObject.transform.position;
        go.transform.rotation = gameObject.transform.rotation;
        go.transform.localScale = gameObject.transform.localScale;

        SpriteRenderer sprite = go.AddComponent<SpriteRenderer>();
        sprite.flipX = renderer.flipX;
        sprite.flipY = renderer.flipY;
        sprite.sprite = renderer.sprite;
        sprite.color = renderer.color;
        sprite.sortingOrder = renderer.sortingOrder + 1;

        Richtungssticker sticker = go.AddComponent<Richtungssticker>();
        sticker.Direction = Richtung;

        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneAt(SceneManager.sceneCount - 1));

        CurrentSticker = sticker;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CurrentSticker.OnPointerUp(eventData);
    }

    private void OnValidate()
    {
        var renderer = gameObject.GetComponent<SpriteRenderer>();
        renderer.flipX = Richtung == DirectionTrial.Direction.CCW;
        renderer.flipY = true;
    }

    void Start()
    {
        //InstanceSticker();
    }

    void Update()
    {
    }
}
