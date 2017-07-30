using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    public enum TileType { floor, wall, goal, door, button, weighedbutton }

    public TileType type;

    public GameObject currentObject;

    public bool isClosed = false;

    public List<Tile> neighbors;

    public int posX;

    public int posY;

    public Sprite normalSprite;
    public Sprite offSprite;

    public int pairNumber;
}
