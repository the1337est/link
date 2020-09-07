using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private Sprite play;
    [SerializeField] private Sprite stop;

    [SerializeField] private Image navigationButton;

    [SerializeField] private GameObject gameover;

    [SerializeField] private AudioSource levelFinishAudio;
    [SerializeField] private AudioSource shortClickAudio;
    [SerializeField] private AudioSource swapAudio;

    private Direction currentDirection = Direction.Right;

    public GameState State = GameState.None;

    private Tile selected = null;

    private int levelIndex = 0;

    public List<Tile> Tiles = new List<Tile>();

    public GameObject MenuCanvas;
    public GameObject GameCanvas;

    public bool Testing = false;

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

    public void StartGame()
    {
        LoadLevel();
    }

    public void MainMenu()
    {
        gameover.SetActive(false);
        StartCoroutine(ResetGame(true));
        MenuCanvas.gameObject.SetActive(true);
        GameCanvas.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    private void LoadLevel()
    {
        if (levels != null && levels.Count > levelIndex)
        {
            if (!Testing)
            {
                levelMap = levels[levelIndex];
            }
            currentDirection = Direction.Right;
            //FindAllColours();
            CreateLevel();
            ShuffleColours();
            ResetCamera();
            State = GameState.Playing;
        }
        else
        {
            levelIndex = 0;
            gameover.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleNavigation();
        }
    }

    private void ResetCamera()
    {
        if (levelMap != null)
        {
            float x = (levelMap.width  - 1)/ 2f;
            float y = (levelMap.height - 1)/ 2f;
            main.transform.position = new Vector3(x, y, -10);
            main.orthographicSize = levelMap.height >= 10 ? 7f : 5f;
        }
    }

    public void ShuffleColours()
    {
        List<Color> temp = new List<Color>(colours);
        Debug.Log("Colours: " + colours.Count);
        temp.RemoveAt(temp.Count - 1);
        Debug.Log("Temp has:" + temp.Count);
        temp.RemoveAt(0);
        if (levelIndex > 0 || Testing)
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

        for (int index = 0; index < Tiles.Count; index++)
        {
            if (shuffledColours.Count > index)
            {
                Tile t = Tiles[index];
                t.ID = index;
                Color color = shuffledColours[index];
                int colourIndex = colours.IndexOf(color);
                t.ColorID = colourIndex;
                t.SetColor(color);
            }
        }
    }

    public void CreateLevel()
    {
        if (levelMap != null)
        {
            colours = new List<Color>();
            int x = 0, y = 0;
            for (int entry = 0; entry < levelMap.height; entry++)
            {
                Color color = levelMap.GetPixel(0, entry);
                if (color != Color.white && color != Color.black)
                {
                    y = entry;
                    colours.Add(color);
                    SpawnTile(x, y);
                    break;
                }
            }
            Debug.Log("Found entry at Y = " + y);
            player.transform.position = new Vector3(x, y, 0);
            
            Vector2 current = new Vector2(x, y);
            Vector2 next = current;
            
            Color nextColor;
            GetNextTile(current, out next, out nextColor);
            
            bool check = true;
            int maxTries = levelMap.height * levelMap.width;
            int tries = 1;
            while (current != next && check)
            {
                colours.Add(nextColor);
                SpawnTile((int)next.x, (int)next.y);
                current = next;
                if (!GetNextTile(current, out next, out nextColor))
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

    private bool GetNextTile(Vector2 current, out Vector2 next, out Color color)
    {
           
        bool found = false;
        if (levelMap != null)
        {
            //check right
            Vector2 right = current + Vector2.right;
            if (currentDirection != Direction.Left && levelMap.width > right.x)
            {
                Color c = levelMap.GetPixel((int)right.x, (int)right.y);
                if (c != Color.white && c != Color.black)
                {
                    currentDirection = Direction.Right;
                    next = right;
                    color = c;
                    return true;
                }
            }

            Vector2 left = current + Vector2.left;
            if (currentDirection != Direction.Right && left.x >= 0)
            {
                Color c = levelMap.GetPixel((int)left.x, (int)left.y);
                if (c != Color.white && c != Color.black)
                {
                    currentDirection = Direction.Left;
                    next = left;
                    color = c;
                    return true;
                }
            }

            //check above
            Vector2 above = current + Vector2.up;
            if (currentDirection != Direction.Down && levelMap.height > above.y)
            {
                Color c = levelMap.GetPixel((int)above.x, (int)above.y);
                if (c != Color.white && c != Color.black)
                {
                    currentDirection = Direction.Up;
                    next = above;
                    color = c;
                    return true;
                }
            }

            //check below
            Vector2 below = current + Vector2.down;
            if (currentDirection != Direction.Up && below.y >= 0)
            {
                Color c = levelMap.GetPixel((int)below.x, (int)below.y);
                if (c != Color.white && c != Color.black)
                {
                    currentDirection = Direction.Down;
                    next = below;
                    color = c;
                    return true;
                }
            }
        }
        next = current;
        color = Color.white;
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
            PlaySwap();
            Swap(tile, selected);
            selected = null;
            Selection.SetActive(false);
        }
    }

    public void ToggleNavigation()
    {
        if (State == GameState.Navigating)
        {
            State = GameState.Playing;
            navigationButton.sprite = play;
            StopCoroutine(Navigation());
            player.transform.position = Tiles[0].transform.position;
        }
        else if (State == GameState.Playing)
        {
            navigationButton.sprite = stop;
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
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(1f);
                        player.transform.position = Tiles[0].transform.position;
                        proceed = false;
                    }
                }
                else
                {
                    //game finished
                    int count = Tiles.Count;
                    float wait = 0f;
                    wait = count < 10 ? count * 0.1f : count * 0.05f;
                    StartCoroutine(ResetGame());
                    PlayLevelFinish();
                    yield return new WaitForSeconds(wait);
                    levelIndex++;
                    State = GameState.None;
                    yield return new WaitForSeconds(3f);
                    LoadLevel();
                    proceed = false;
                }
            }

        }
        State = GameState.Playing;
        navigationButton.sprite = play;
        yield return null;
    }

    public void NewGame()
    {
        levelIndex = 0;
    }

    public void ToggleCredits(bool toggle)
    {

    }

    IEnumerator ResetGame(bool instant = false)
    {
        float wait = 0f;
        wait = Tiles.Count < 10 ? 0.1f : 0.05f;

        Transform[] all = container.GetComponentsInChildren<Transform>();
        for (int i = 1; i < all.Length; i++)
        {            
            Destroy(all[i].gameObject);
            if (!instant)
            {
                yield return new WaitForSeconds(wait);
            }
        }
        Tiles.Clear();
        shuffledColours.Clear();
        colours.Clear();
        Selection.SetActive(false);
        yield return null;
    }

    public void PlayShortClick()
    {
        shortClickAudio.Play();
    }

    public void PlaySwap()
    {
        swapAudio.Play();
    }

    public void PlayLevelFinish()
    {
        levelFinishAudio.Play();
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
