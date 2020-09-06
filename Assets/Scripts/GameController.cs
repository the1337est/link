using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public static GameController Instance;

    [SerializeField] private Texture2D levelMap;
    [SerializeField] private GameObject tile;

    public List<Tile> Tiles;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                DestroyImmediate(this);
            }
        }
    }

    public void CreateLevel()
    {
        if (levelMap != null)
        {

            //for(int x = 0; x < levelMap.wid)
        }
    }
    
}
