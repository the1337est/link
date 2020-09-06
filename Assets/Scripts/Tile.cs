using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    public int ID;
    public Color Color;

    SpriteRenderer rend;

    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
    }

    public void SetColor(Color color)
    {
        Color = color;
        rend.material.color = Color;
    }

    public void OnMouseDown()
    {
        Debug.Log("Clicked!");
        GameController.Instance.OnTileClicked(this);
    }
}
