using System.Collections.Generic;
using UnityEngine;

public class ProceduralGenerator : MonoBehaviour
{
    private Dictionary<TileType, GameObject> tileMap;
    public TileType[,] map;

    public int mapWidth = 10;
    public int mapLength = 10;

    // int[,] tilesToCheck = new int[,] {{x-1, z-1}, {x, z-1}, {x+1, z-1}, {x-1, z}, {x-1, z+1}, {x, z+1}, {x+1, z+1}, {x+1, z}};

    void Start()
    {
        map = new TileType[mapLength, mapWidth];

        // Fill the Map with empty spots.
        for (int x = 0; x < mapLength; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                map[x, z] = TileType.Empty;
            }
        }

        FindTiles();
        if (tileMap.Count == 0)
        {
            Debug.Log("No tiles found!");
            return;
        }

        // Generate the map
        GenerateWorld();
    }

    void GenerateWorld()
    {
        // Empty
    }

    void FindTiles()
    {
        tileMap = new Dictionary<TileType, GameObject>();

        GameObject[] objects = Resources.LoadAll<GameObject>("Prefabs");

        foreach (GameObject g in objects)
        {
            if (g.CompareTag("Tile"))
            {
                Debug.Log(g.name);
                tileMap.Add(g.GetComponent<Tile>().type, g);
            }
        }

    }
}