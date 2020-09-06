using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public static GameController Instance;

    private Camera main;

    [Header("References")]
    public GameObject Selection;
    [SerializeField] private Texture2D levelMap;

    [Header("Gameplay Variables")]
    [SerializeField] private GameObject tile;
    [SerializeField] private List<Color> colours;
    [SerializeField] private List<Color> shuffledColours;

    private Direction currentDirection = Direction.Right;

    private Tile selected = null;

    public List<Tile> Tiles = new List<Tile>();

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

        main = Camera.main;
    }

    private void Start()
    {
        FindAllColours();
        CreateLevel();
        ResetCamera();
    }

    private void ResetCamera()
    {
        if (levelMap != null)
        {
            float x = levelMap.width / 2f;
            float y = levelMap.height / 2f;
            main.transform.position = new Vector3(x, y, -10);
        }
    }

    //public void CreateLevel()
    //{
    //    if (levelMap != null)
    //    {
    //        for (int y = levelMap.height - 1; y >= 0; y--)
    //        {
    //            for (int x = 0; x < levelMap.width; x++)
    //            {
    //                Color color = levelMap.GetPixel(x, y);
    //                if (color == Color.white)
    //                {
    //                    SpawnTile(x, y);
    //                }
    //            }
    //        }
    //    }
    //}

    public void FindAllColours()
    {
        colours = new List<Color>();
        for (int y = 0; y < levelMap.height; y++)
        {
            bool shouldBreak = false;
            for (int x = 0; x < levelMap.width; x++)
            {
                bool found = false;
                Color c = levelMap.GetPixel(x, y);
                if(c != Color.black && c != Color.white)
                {
                    found = true;
                    colours.Add(levelMap.GetPixel(x, y));
                }
                if (!found)
                {
                    shouldBreak = true;
                    break;
                }
            }
            if (shouldBreak)
            {
                break;
            }
        }
        shuffledColours = new List<Color>();
        System.Random r = new System.Random();
        shuffledColours = colours.OrderBy(x => r.Next()).ToList();
        Debug.Log("Found " + colours.Count + " colours");
    }

    public void CreateLevel()
    {
        if (levelMap != null)
        {
            int x = 0, y = 0;
            for (int entry = 0; entry < levelMap.height; entry++)
            {
                Color color = levelMap.GetPixel(0, entry);
                if (color == Color.white)
                {
                    y = entry;
                    SpawnTile(x, y);
                    break;
                }
            }
            Debug.Log("Found entry at Y = " + y);
            Vector2 current = new Vector2(x, y);
            Vector2 next = current;
            GetNextWhite(current, out next);
            bool check = true;
            int maxTries = levelMap.height * levelMap.width;
            int tries = 1;
            while (current != next && check)
            {
                SpawnTile((int)next.x, (int)next.y);
                current = next;
                if (!GetNextWhite(current, out next))
                {
                    check = false;
                }
                if (++tries > maxTries)
                {
                    Debug.LogError("Breaking because potential infinite loop detected");
                    break;
                }
            }
        }
    }

    private bool GetNextWhite(Vector2 current, out Vector2 next)
    {
           
        bool found = false;
        if (levelMap != null)
        {
            //check right
            Vector2 right = current + Vector2.right;
            if (currentDirection != Direction.Left && levelMap.width > right.x)
            {
                Color c = levelMap.GetPixel((int)right.x, (int)right.y);
                if (c == Color.white)
                {
                    currentDirection = Direction.Right;
                    next = right;
                    return true;
                }
            }

            //check above
            Vector2 above = current + Vector2.up;
            if (currentDirection != Direction.Down && levelMap.height > above.y)
            {
                Color c = levelMap.GetPixel((int)above.x, (int)above.y);
                if (c == Color.white)
                {
                    currentDirection = Direction.Up;
                    next = above;
                    return true;
                }
            }

            //check below
            Vector2 below = current + Vector2.down;
            if (currentDirection != Direction.Up && below.y >= 0)
            {
                Color c = levelMap.GetPixel((int)below.x, (int)below.y);
                if (c == Color.white)
                {
                    currentDirection = Direction.Down;
                    next = below;
                    return true;
                }
            }
        }
        next = current;
        return found;
    }

    private void Swap(Tile a, Tile b)
    {
        if (a != null && b != null)
        {

            Color temp = a.Color;
            a.SetColor(b.Color);
            b.SetColor(temp); 
        }
    }

    private void SpawnTile(int x, int y)
    {
        GameObject g = Instantiate(tile, transform);
        Tile t = g.GetComponent<Tile>();
        int index = Tiles.Count;
        if (shuffledColours.Count > index)
        {
            t.SetColor(shuffledColours[index]);
        }
        Tiles.Add(t);
        g.transform.position = new Vector3(x, y, 0);
    }

    public void OnTileClicked(Tile tile)
    {
        if (selected == null)
        {
            selected = tile;
            Selection.SetActive(true);
            Selection.transform.position = tile.transform.position;
        }
        else if (selected == tile)
        {
            selected = null;
            Selection.SetActive(false);
        }
        else
        {
            Swap(tile, selected);
            selected = null;
            Selection.SetActive(false);
        }
    }
}

public enum Direction
{
    Right,
    Up,
    Down, 
    Left
}
