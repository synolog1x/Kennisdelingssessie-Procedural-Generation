using System.Collections.Generic;
using UnityEngine;

public class ProceduralGenerator : MonoBehaviour
{
    private Dictionary<TileType, GameObject> tileMap;
    public TileType[,] map;
    public int seed = 0;

    public float forestFactor = 0.6f;
    public float desertFactor = 0.6f;
    public float plainsFactor = 0.4f;

    public int mapWidth = 10;
    public int mapLength = 10;

    void Start()
    {
        map = new TileType[mapLength, mapWidth];

        if (seed == 0)
        {
            seed = Random.Range(1, 10000);
        }

        Random.InitState(seed);

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
        for (int x = 0; x < mapLength; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                Vector3 spawnPos = new Vector3(x * 10, 0, z * 10);

                GameObject tile = GenerateTileFor(x, z);

                Instantiate(tile, spawnPos, transform.rotation).SetActive(true);
                map[x, z] = tile.GetComponent<Tile>().type;
            }
        }
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

    private GameObject GetRandomTile(List<TileType> exclude)
    {
        List<TileType> tileTypeList = new List<TileType>(tileMap.Keys).FindAll(t => !exclude.Contains(t));

        return tileMap[tileTypeList[Random.Range(0, tileTypeList.Count)]];
    }

    Dictionary<TileType, int> GetSurroundingTiles(int x, int z)
    {
        Dictionary<TileType, int> amounts = new Dictionary<TileType, int>();

        int[,] tilesToCheck = new int[,] { { x - 1, z - 1 }, { x, z - 1 }, { x + 1, z - 1 }, { x - 1, z }, { x - 1, z + 1 }, { x, z + 1 }, { x + 1, z + 1 }, { x + 1, z } };

        for (int i = 0; i < tilesToCheck.GetLength(0); i++)
        {
            int xToCheck = tilesToCheck[i, 0];
            int zToCheck = tilesToCheck[i, 1];

            if (xToCheck >= 0 && zToCheck >= 0 && xToCheck < mapLength && zToCheck < mapWidth)
            {
                if (map[xToCheck, zToCheck] != TileType.Empty)
                {
                    TileType type = map[xToCheck, zToCheck];
                    if (amounts.ContainsKey(type))
                    {
                        amounts[type] += 1;
                    }
                    else
                    {
                        amounts.Add(type, 1);
                    }
                }
            }
        }
        return amounts;
    }

    private GameObject GenerateTileFor(int x, int z)
    {
        Dictionary<TileType, int> amount = GetSurroundingTiles(x, z);
        int total = 0;

        foreach (int i in amount.Values)
        {
            total += i;
        }

        if (total == 0)
        {
            return GetRandomTile(new List<TileType>());
        }

        // If there is both Forest and Desert, the tile has to be plains
        if (amount.ContainsKey(TileType.Forest) && amount.ContainsKey(TileType.Desert))
        {
            return tileMap[TileType.Plains];
        }

        // If there is Forest, it means the tile can only be Plains or Forest
        if (amount.ContainsKey(TileType.Forest))
        {
            float chanceForForest = amount[TileType.Forest] * forestFactor;

            if (Random.Range(0f, total) <= chanceForForest)
            {
                return tileMap[TileType.Forest];
            }
            else
            {
                return tileMap[TileType.Plains];
            }
        }

        // If there is Desert, it means the tile can only be Plains or Desert
        if (amount.ContainsKey(TileType.Desert))
        {
            float chanceForDesert = amount[TileType.Desert] * desertFactor;

            if (Random.Range(0f, total) <= chanceForDesert)
            {
                return tileMap[TileType.Desert];
            }
            else
            {
                return tileMap[TileType.Plains];
            }
        }

        // If there is no Desert or Forest around the tile, it means the tile can be any type (plains is prefered)
        float chanceForPlains = amount[TileType.Plains] * plainsFactor;

        if (Random.Range(0f, total) <= chanceForPlains)
        {
            return tileMap[TileType.Plains];
        }
        else
        {
            List<TileType> excluded = new List<TileType>();
            excluded.Add(TileType.Plains);

            return GetRandomTile(excluded);
        }
    }
}