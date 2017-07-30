using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour 
{
    protected int sizex;
    protected int sizey;

    public Camera camera;
    public AudioClip goalSfx;
    public AudioClip moveBoxSfx;
    public AudioClip buttonSfx;
    public AudioClip powerOutSfx;

    int maxPower = 20;
    public int power = 20;
    int levelCounter = 0;

    public GameObject floorPrefab;

    public GameObject buttonPrefab;
    public GameObject buttonWeightedPrefab;
    public GameObject doorPrefabVert;
    public GameObject doorPrefabHoriz;

    public GameObject player;

    public Text levelText;
    public Text powerText;
    public Image powerFill;

    [System.Serializable]
    public struct ColorPrefab
    {
        public Color32 color;
        public GameObject prefab;
    }

    [System.Serializable]
    public struct levelInfo
    {
        public int levelPower;
        public Texture2D levelMap;
    }

    public Color32 playerColor;
    public Color32 boxColor;
    public ColorPrefab[] prefabs;
    public levelInfo[] levels;

    public Dictionary<Color32, GameObject> colorPrefabDict;

    bool playerMoving = false;

    public int playerPosX;
    public int playerPosY;

    public Tile[,] tiles;

    void Start()
    {

        colorPrefabDict = new Dictionary<Color32, GameObject>();

        foreach (ColorPrefab cp in prefabs)
        {
            colorPrefabDict.Add(cp.color, cp.prefab);
        }

        EndLevel();
    }

    void Update()
    {
        MovePlayer();
        if (playerMoving == false)
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                AudioManager.instance.PlayClip(goalSfx);
                EndLevel();   
            }
        }
    }

    void LoadLevel(Texture2D level)
    {
        sizex = level.width;
        sizey = level.height;
        tiles = new Tile[sizex,sizey];

        camera.transform.position = new Vector3(sizex / 2, sizey / 2, -10);
        Color32[] map = level.GetPixels32();

        for (int y = 0; y < sizey; y++)
        {
            for (int x = 0; x < sizex; x++)
            {

                if (map[(y * sizex) + x].Equals(playerColor))
                {
                    player = Instantiate(colorPrefabDict[playerColor], new Vector3(x, y, -0.1f), this.transform.rotation, this.transform) as GameObject;
                    playerPosX = x;
                    playerPosY = y;
                    GameObject go = Instantiate(floorPrefab, new Vector3(x, y, 0), this.transform.rotation, this.transform) as GameObject;
                    tiles[x, y] = go.GetComponent<Tile>();
                    tiles[x, y].posX = x;
                    tiles[x, y].posY = y;

                }
                else if (map[(y * sizex) + x].Equals(boxColor))
                {
                    GameObject go = Instantiate(floorPrefab, new Vector3(x, y, 0), this.transform.rotation, this.transform) as GameObject;
                    tiles[x, y] = go.GetComponent<Tile>();
                    tiles[x, y].posX = x;
                    tiles[x, y].posY = y;
                    tiles[x,y].currentObject = Instantiate(colorPrefabDict[boxColor], new Vector3(x, y, -0.1f), this.transform.rotation, this.transform) as GameObject;
                }
                else if (map[(y * sizex) + x].r == 255 && map[(y * sizex) + x].g != 255)
                {
                    if (map[(y * sizex) + x].b == 0)
                    {
                        GameObject go = Instantiate(buttonPrefab, new Vector3(x, y, 0), this.transform.rotation, this.transform) as GameObject;
                        tiles[x, y] = go.GetComponent<Tile>();
                        tiles[x, y].posX = x;
                        tiles[x, y].posY = y;
                        tiles[x, y].pairNumber = map[(y * sizex) + x].g;
                    }
                    else if (map[(y * sizex) + x].b == 1)
                    {
                        GameObject go = Instantiate(buttonWeightedPrefab, new Vector3(x, y, 0), this.transform.rotation, this.transform) as GameObject;
                        tiles[x, y] = go.GetComponent<Tile>();
                        tiles[x, y].posX = x;
                        tiles[x, y].posY = y;
                        tiles[x, y].pairNumber = map[(y * sizex) + x].g;
                    }
                }
                else if (map[(y * sizex) + x].b == 255)
                {
                    if (map[(y * sizex) + x].r == 0)
                    {
                        GameObject go = Instantiate(doorPrefabVert, new Vector3(x, y, 0), this.transform.rotation, this.transform) as GameObject;
                        tiles[x, y] = go.GetComponent<Tile>();
                        tiles[x, y].posX = x;
                        tiles[x, y].posY = y;
                        tiles[x, y].pairNumber = map[(y * sizex) + x].g;
                    }
                    else if (map[(y * sizex) + x].r == 1)
                    {
                        GameObject go = Instantiate(doorPrefabHoriz, new Vector3(x, y, 0), this.transform.rotation, this.transform) as GameObject;
                        go.transform.Rotate(new Vector3(0, 0, 90));
                        tiles[x, y] = go.GetComponent<Tile>();
                        tiles[x, y].posX = x;
                        tiles[x, y].posY = y;
                        tiles[x, y].pairNumber = map[(y * sizex) + x].g;
                    }
                }
                else
                {
                    GameObject go = Instantiate(colorPrefabDict[map[(y * sizex) + x]], new Vector3(x, y, 0), this.transform.rotation, this.transform) as GameObject;
                    tiles[x, y] = go.GetComponent<Tile>();
                    tiles[x, y].posX = x;
                    tiles[x, y].posY = y;
                }

            } 
        }

        for (int x = 0; x < sizex; x++)
        {
            for (int y = 0; y < sizey; y++)
            {
                if (x > 0)
                {
                    tiles[x, y].neighbors.Add(tiles[x - 1, y]);
                }

                if (x < sizex - 1)
                {
                    tiles[x, y].neighbors.Add(tiles[x + 1, y]);
                }

                if (y > 0)
                {
                    tiles[x, y].neighbors.Add(tiles[x, y - 1]);
                }

                if (y < sizey - 1)
                {
                    tiles[x, y].neighbors.Add(tiles[x, y + 1]);
                }

            }
        } 
    }

    void MovePlayer()
    {
        if (power > 0)
        {
            if (playerMoving == false)
            {
                if(Input.GetKeyDown(KeyCode.UpArrow))
                {  
                    //Move Up 1 Space.
                    if (!((tiles[playerPosX, playerPosY + 1].type == Tile.TileType.wall || tiles[playerPosX, playerPosY + 1].isClosed == true )
                        || (tiles[playerPosX, playerPosY + 1].currentObject != null && tiles[playerPosX, playerPosY + 1].currentObject.tag.Equals("Box") && (tiles[playerPosX, playerPosY + 2].type == Tile.TileType.wall || tiles[playerPosX, playerPosY + 2].isClosed == true))
                        || (tiles[playerPosX, playerPosY + 1].currentObject != null && tiles[playerPosX, playerPosY + 1].currentObject.tag.Equals("Box") && tiles[playerPosX, playerPosY + 2].currentObject != null && tiles[playerPosX, playerPosY + 2].currentObject.tag.Equals("Box"))))
                    {
                        if (tiles[playerPosX, playerPosY].type == Tile.TileType.weighedbutton)
                        {
                            for (int x = 0; x < sizex; x++)
                            {
                                for (int y = 0; y < sizey; y++)
                                {
                                    if (tiles[playerPosX, playerPosY].pairNumber == tiles[x, y].pairNumber)
                                    {
                                        if (tiles[x, y].type == Tile.TileType.door)
                                        {
                                            tiles[x, y].isClosed = true;
                                        }
                                        tiles[x, y].GetComponent<SpriteRenderer>().sprite = tiles[x, y].normalSprite;
                                        AudioManager.instance.PlayClip(buttonSfx);
                                    }
                                }
                            }
                        }
                        playerPosY += 1;
                        player.transform.eulerAngles = new Vector3(0, 0, 90);                    
                        StartCoroutine(movePlayer(tiles[playerPosX, playerPosY].transform.position, 2f));

                        if (tiles[playerPosX, playerPosY].currentObject != null && tiles[playerPosX, playerPosY].currentObject.tag.Equals("Box"))
                        {
                            AudioManager.instance.PlayClip(moveBoxSfx);
                            GameObject box = tiles[playerPosX, playerPosY].currentObject;
                            StartCoroutine(moveBox(box, tiles[playerPosX, playerPosY + 1].transform.position, 2f, tiles[playerPosX, playerPosY + 1]));
             
                            tiles[playerPosX, playerPosY].currentObject = null;
                            tiles[playerPosX, playerPosY + 1].currentObject = box;
                        }
                    }
                }
                else if(Input.GetKeyDown(KeyCode.DownArrow))
                {
                    //Move Down 1 Space.
                    if (!((tiles[playerPosX, playerPosY - 1].type == Tile.TileType.wall || tiles[playerPosX, playerPosY - 1].isClosed == true) 
                        || (tiles[playerPosX, playerPosY - 1].currentObject != null && tiles[playerPosX, playerPosY - 1].currentObject.tag.Equals("Box") && (tiles[playerPosX, playerPosY - 2].type == Tile.TileType.wall || tiles[playerPosX, playerPosY - 2].isClosed == true))
                        || (tiles[playerPosX, playerPosY - 1].currentObject != null && tiles[playerPosX, playerPosY - 1].currentObject.tag.Equals("Box") && tiles[playerPosX, playerPosY - 2].currentObject != null && tiles[playerPosX, playerPosY - 2].currentObject.tag.Equals("Box"))))
                    {
                        if (tiles[playerPosX, playerPosY].type == Tile.TileType.weighedbutton)
                        {
                            for (int x = 0; x < sizex; x++)
                            {
                                for (int y = 0; y < sizey; y++)
                                {
                                    if (tiles[playerPosX, playerPosY].pairNumber == tiles[x, y].pairNumber)
                                    {
                                        if (tiles[x, y].type == Tile.TileType.door)
                                        {
                                            tiles[x, y].isClosed = true;
                                        }
                                        tiles[x, y].GetComponent<SpriteRenderer>().sprite = tiles[x, y].normalSprite;
                                        AudioManager.instance.PlayClip(buttonSfx);
                                    }
                                }
                            }
                        }
                        playerPosY -= 1;
                        player.transform.eulerAngles = new Vector3(0, 0, 270); 
                        StartCoroutine(movePlayer(tiles[playerPosX, playerPosY].transform.position, 2f));
                        if (tiles[playerPosX, playerPosY].currentObject != null && tiles[playerPosX, playerPosY].currentObject.tag.Equals("Box"))
                        {
                            AudioManager.instance.PlayClip(moveBoxSfx);
                            GameObject box = tiles[playerPosX, playerPosY].currentObject;
                            StartCoroutine(moveBox(box, tiles[playerPosX, playerPosY - 1].transform.position, 2f, tiles[playerPosX, playerPosY - 1]));
                            tiles[playerPosX, playerPosY].currentObject = null;
                            tiles[playerPosX, playerPosY - 1].currentObject = box;
                        }
                    }
                }
                else if(Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    //Move Left 1 Space.
                    if (!((tiles[playerPosX - 1, playerPosY].type == Tile.TileType.wall || tiles[playerPosX - 1, playerPosY].isClosed == true) 
                        || (tiles[playerPosX - 1, playerPosY].currentObject != null && tiles[playerPosX - 1, playerPosY].currentObject.tag.Equals("Box") && (tiles[playerPosX - 2, playerPosY].type == Tile.TileType.wall || tiles[playerPosX - 2, playerPosY].isClosed == true))
                        || (tiles[playerPosX - 1, playerPosY].currentObject != null && tiles[playerPosX - 1, playerPosY].currentObject.tag.Equals("Box") && tiles[playerPosX - 2, playerPosY].currentObject != null && tiles[playerPosX - 2, playerPosY].currentObject.tag.Equals("Box"))))
                    {
                        if (tiles[playerPosX, playerPosY].type == Tile.TileType.weighedbutton)
                        {
                            for (int x = 0; x < sizex; x++)
                            {
                                for (int y = 0; y < sizey; y++)
                                {
                                    if (tiles[playerPosX, playerPosY].pairNumber == tiles[x, y].pairNumber)
                                    {
                                        if (tiles[x, y].type == Tile.TileType.door)
                                        {
                                            tiles[x, y].isClosed = true;
                                        }
                                        tiles[x, y].GetComponent<SpriteRenderer>().sprite = tiles[x, y].normalSprite;
                                        AudioManager.instance.PlayClip(buttonSfx);
                                    }
                                }
                            }
                        }
                        playerPosX -= 1;
                        player.transform.eulerAngles = new Vector3(0, 0, 180); 
                        StartCoroutine(movePlayer(tiles[playerPosX, playerPosY].transform.position, 2f));

                        if (tiles[playerPosX, playerPosY].currentObject != null && tiles[playerPosX, playerPosY].currentObject.tag.Equals("Box"))
                        {
                            AudioManager.instance.PlayClip(moveBoxSfx);
                            GameObject box = tiles[playerPosX, playerPosY].currentObject;
                            StartCoroutine(moveBox(box, tiles[playerPosX - 1, playerPosY].transform.position, 2f, tiles[playerPosX - 1, playerPosY]));
                            tiles[playerPosX, playerPosY].currentObject = null;
                            tiles[playerPosX - 1, playerPosY].currentObject = box;
                        }
                    }
                }
                else if(Input.GetKeyDown(KeyCode.RightArrow))
                {
                    //Move Right 1 Space.
                    if (!((tiles[playerPosX + 1, playerPosY].type == Tile.TileType.wall || tiles[playerPosX + 1, playerPosY].isClosed == true) 
                        || (tiles[playerPosX + 1, playerPosY].currentObject != null && tiles[playerPosX + 1, playerPosY].currentObject.tag.Equals("Box") && (tiles[playerPosX + 2, playerPosY].type == Tile.TileType.wall || tiles[playerPosX + 2, playerPosY].isClosed == true))
                        || (tiles[playerPosX + 1, playerPosY].currentObject != null && tiles[playerPosX + 1, playerPosY].currentObject.tag.Equals("Box") && tiles[playerPosX + 2, playerPosY].currentObject != null && tiles[playerPosX + 2, playerPosY].currentObject.tag.Equals("Box"))))
                    {
                        if (tiles[playerPosX, playerPosY].type == Tile.TileType.weighedbutton)
                        {
                            for (int x = 0; x < sizex; x++)
                            {
                                for (int y = 0; y < sizey; y++)
                                {
                                    if (tiles[playerPosX, playerPosY].pairNumber == tiles[x, y].pairNumber)
                                    {
                                        if (tiles[x, y].type == Tile.TileType.door)
                                        {
                                            tiles[x, y].isClosed = true;
                                        }
                                        tiles[x, y].GetComponent<SpriteRenderer>().sprite = tiles[x, y].normalSprite;
                                        AudioManager.instance.PlayClip(buttonSfx);
                                    }
                                }
                            }
                        }
                        playerPosX += 1;
                        player.transform.eulerAngles = new Vector3(0, 0, 0); 
                        StartCoroutine(movePlayer(tiles[playerPosX, playerPosY].transform.position, 2f));
                        if (tiles[playerPosX, playerPosY].currentObject != null && tiles[playerPosX, playerPosY].currentObject.tag.Equals("Box"))
                        {
                            GameObject box = tiles[playerPosX, playerPosY].currentObject;
                            AudioManager.instance.PlayClip(moveBoxSfx);
                            StartCoroutine(moveBox(box, tiles[playerPosX + 1, playerPosY].transform.position, 2f, tiles[playerPosX + 1, playerPosY]));

                            tiles[playerPosX, playerPosY].currentObject = null;
                            tiles[playerPosX + 1, playerPosY].currentObject = box;
                        }
                    }
                }

            }
               
        }


    }

    IEnumerator movePlayer(Vector3 newPos, float speed)
    {
        playerMoving = true;
        player.GetComponentsInChildren<Animator>()[1].SetBool("Moving", true);

        if (tiles[playerPosX, playerPosY].type == Tile.TileType.button)
        {
            for (int x = 0; x < sizex; x++)
            {
                for (int y = 0; y < sizey; y++)
                {
                    if (tiles[playerPosX, playerPosY].pairNumber == tiles[x, y].pairNumber)
                    {
                        tiles[x, y].isClosed = false;
                        tiles[x, y].GetComponent<SpriteRenderer>().sprite = tiles[x, y].offSprite;
                        AudioManager.instance.PlayClip(buttonSfx);
                    }
                }
            }
        }
        else if (tiles[playerPosX, playerPosY].type == Tile.TileType.weighedbutton)
        {
            for (int x = 0; x < sizex; x++)
            {
                for (int y = 0; y < sizey; y++)
                {
                    if (tiles[playerPosX, playerPosY].pairNumber == tiles[x, y].pairNumber)
                    {
                        tiles[x, y].isClosed = false;
                        tiles[x, y].GetComponent<SpriteRenderer>().sprite = tiles[x, y].offSprite;
                        AudioManager.instance.PlayClip(buttonSfx);
                    }
                }
            }
        }
        while (Vector3.Distance(player.transform.position, newPos) > 0)
        {
            float step = speed * Time.deltaTime;
            player.transform.position = Vector3.MoveTowards(player.transform.position, newPos, step);
            yield return null;
        }

        playerMoving = false;
        player.GetComponentsInChildren<Animator>()[1].SetBool("Moving", false);

        power -= 1;
        powerFill.fillAmount = ((float)power / (float)maxPower);
        powerText.text = "Power: " + power;

        if (tiles[playerPosX, playerPosY].type == Tile.TileType.goal)
        {
            AudioManager.instance.PlayClip(goalSfx);
            levelCounter++;
            EndLevel();
        }
            
        if (power == 0)
        {
            AudioManager.instance.PlayClip(powerOutSfx);
            handleFail();
        }
    }
    IEnumerator moveBox(GameObject box, Vector3 newPos, float speed, Tile tile)
    {
        if (tile.type == Tile.TileType.button)
        {
            for (int x = 0; x < sizex; x++)
            {
                for (int y = 0; y < sizey; y++)
                {
                    if (tile.pairNumber == tiles[x, y].pairNumber)
                    {
                        tiles[x, y].isClosed = false;
                        tiles[x, y].GetComponent<SpriteRenderer>().sprite = tiles[x, y].offSprite;
                        AudioManager.instance.PlayClip(buttonSfx);
                    }
                }
            }
        }
        else if (tile.type == Tile.TileType.weighedbutton)
        {
            for (int x = 0; x < sizex; x++)
            {
                for (int y = 0; y < sizey; y++)
                {
                    if (tile.pairNumber == tiles[x, y].pairNumber)
                    {
                        tiles[x, y].isClosed = false;
                        tiles[x, y].GetComponent<SpriteRenderer>().sprite = tiles[x, y].offSprite;
                        AudioManager.instance.PlayClip(buttonSfx);
                    }
                }
            }
        }
        while (Vector3.Distance(box.transform.position, newPos) > 0)
        {
            float step = speed * Time.deltaTime;
            box.transform.position = Vector3.MoveTowards(box.transform.position, newPos, step);
            yield return null;
        }
            
    }

    void handleFail()
    {
        player.GetComponentsInChildren<Light>()[0].enabled = false;
        player.GetComponentsInChildren<Light>()[1].enabled = false;


    }

    void EndLevel()
    {
        StopCoroutine("movePlayer");
        StopCoroutine("moveBox");
        if (levelCounter + 1 > levels.Count())
        {
            //Do Victory Screen
            SceneManager.LoadScene(3);
        }
        else
        {
            foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
            {
                if(!o.tag.Equals("GameController"))
                {
                    Destroy(o);
                }
            }
            power = levels[levelCounter].levelPower;
            maxPower = power;
            powerFill.fillAmount = ((float)power / (float)maxPower);
            powerText.text = "Power: " + power;
            levelText.text = "Level " + (levelCounter + 1);
            LoadLevel(levels[levelCounter].levelMap);
        }
    }
}




