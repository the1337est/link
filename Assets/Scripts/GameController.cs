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

    [SerializeField] private List<Texture2D> levels;

    [Header("Gameplay Variables")]
    [SerializeField] private Transform container;
    [SerializeField] private GameObject tile;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject trophy;
    [SerializeField] private List<Color> colours;
    [SerializeField] private List<Color> shuffledColours;

    private Direction currentDirection = Direction.Right;

    public GameState State = GameState.None;

    private Tile selected = null;

    private int levelIndex = 0;

    public List<Tile> Tiles = new List<Tile>();

    public GameObject Canvas;

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
        levels = Resources.LoadAll<Texture2D>("Levels/").ToList();
    }

    private void Start()
    {
        LoadLevel();
    }

    private void LoadLevel()
    {
        if (levels != null && levels.Count > levelIndex)
        {
            levelMap = levels[levelIndex];
            FindAllColours();
            CreateLevel();
            ResetCamera();
            State = GameState.Playing;
        }      
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartNavigating();
        }
    }

    private void ResetCamera()
    {
        if (levelMap != null)
        {
            float x = (levelMap.width  - 1)/ 2f;
            float y = (levelMap.height - 1)/ 2f;
            main.transform.position = new Vector3(x, y, -10);
        }
    }

    public void FindAllColours()
    {
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
        List<Color> temp = new List<Color>(colours);
        temp.RemoveAt(temp.Count - 1);
        temp.RemoveAt(0);
        if (levelIndex > 0)
        {
            shuffledColours = new List<Color>();
            System.Random r = new System.Random();
            shuffledColours = temp.OrderBy(x => r.Next()).ToList();
            shuffledColours.Insert(0, colours[0]);
            shuffledColours.Add(colours[colours.Count - 1]);
        }
        else
        {
            shuffledColours = new List<Color>
            {
                colours[0],
                colours[2],
                colours[1],
                colours[3]
            };
        }
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
            player.transform.position = new Vector3(x, y, 0);
            
            Vector2 current = new Vector2(x, y);
            Vector2 next = current;
            GetNextTile(current, out next);
            bool check = true;
            int maxTries = levelMap.height * levelMap.width;
            int tries = 1;
            while (current != next && check)
            {
                SpawnTile((int)next.x, (int)next.y);
                current = next;
                if (!GetNextTile(current, out next))
                {
                    check = false;
                }
                if (++tries > maxTries)
                {
                    Debug.LogError("Breaking because potential infinite loop detected");
                    break;
                }
            }
            trophy.transform.position = Tiles[Tiles.Count - 1].transform.position;
        }
    }

    private bool GetNextTile(Vector2 current, out Vector2 next)
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

            Vector2 left = current + Vector2.left;
            if (currentDirection != Direction.Right && left.x >= 0)
            {
                Color c = levelMap.GetPixel((int)left.x, (int)left.y);
                if (c == Color.white)
                {
                    currentDirection = Direction.Left;
                    next = left;
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
            int tempID = a.ColorID;
            Color temp = a.Color;
            a.ColorID = b.ColorID;
            b.ColorID = tempID;
            a.SetColor(b.Color);
            b.SetColor(temp); 
        }
    }

    private void SpawnTile(int x, int y)
    {
        GameObject g = Instantiate(tile, container);
        Tile t = g.GetComponent<Tile>();
        int index = Tiles.Count;
        if (shuffledColours.Count > index)
        {
            t.ID = index;
            Color color = shuffledColours[index];
            int colourIndex = colours.IndexOf(color);
            t.ColorID = colourIndex;
            t.SetColor(color);
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

    public void StartNavigating()
    {
        if (State == GameState.Playing)
        {
            StopCoroutine(Navigation());
            State = GameState.Navigating;
            StartCoroutine(Navigation());
        }
    }

    private IEnumerator Navigation()
    {
        if (Tiles != null && Tiles.Count > 0)
        {
            int index = 0;
            bool proceed = true;
            while (proceed)
            {
                index++;
                if (index < Tiles.Count)
                {
                    Tile t = Tiles[index];
                    if (t.ID == t.ColorID)
                    {
                        player.transform.position = t.transform.position;
                        yield return new WaitForSeconds(0.35f);
                    }
                    else
                    {
                        proceed = false;
                    }
                }
                else
                {
                    //game finished
                    ResetGame();
                    levelIndex++;
                    yield return new WaitForSeconds(3f);
                    LoadLevel();
                    proceed = false;
                }
            }

        }
        State = GameState.Playing;
        yield return null;
    }

    public void NewGame()
    {
        levelIndex = 0;
    }

    public void ToggleCredits(bool toggle)
    {

    }

    public void ResetGame()
    {
        Transform[] all = container.GetComponentsInChildren<Transform>();
        for (int i = 1; i < all.Length; i++)
        {
            
            Destroy(all[i].gameObject);
        }
        Tiles.Clear();
        shuffledColours.Clear();
        colours.Clear();
    }
}

public enum Direction
{
    Right,
    Up,
    Down, 
    Left
}

public enum GameState
{
    None,
    Playing,
    Navigating
}
