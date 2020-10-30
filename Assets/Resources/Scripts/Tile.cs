using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Empty,
    Desert,
    Forest,
    Plains,
}

public class Tile : MonoBehaviour
{
    public TileType type;

}
