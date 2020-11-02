using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;


public class ProceduralGenerator : MonoBehaviour
{
    private Dictionary<TileType, GameObject> tileMap;
    public TileType[,] map;

    public int mapWidth = 10;
    public int mapLength = 10;
    public int Seed = 0;

    public float ForestWeightFactor = 0.6f;
    public float DesertWeightFactor = 0.6f;
    public float PlainsWeightFactor = 0.4f;

    // int[,] tilesToCheck = new int[,] {{x-1, z-1}, {x, z-1}, {x+1, z-1}, {x-1, z}, {x-1, z+1}, {x, z+1}, {x+1, z+1}, {x+1, z}};

    void Start()
    {
        map = new TileType[mapLength, mapWidth];

        if (Seed == 0)
        {
            Seed = Random.Range(1, 10000);
        }

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
        for (var x = 0; x < mapLength; x++)
        {
            for (var z = 0; z < mapWidth; z++)
            {
                var spawnPosition = new Vector3(x * 10, 0, z * 10);

                var tile = GenerateTileFor(x, z);

                Instantiate(tile, spawnPosition, transform.rotation).SetActive(true);

                map[x, z] = tile.GetComponent<Tile>().type;
            }
        }
    }

    private GameObject GetRandomTile(List<TileType> exclude)
    {
        var tileTypeList = new List<TileType>(tileMap.Keys).Except(exclude).ToList();

        return tileMap[tileTypeList[Random.Range(0, tileTypeList.Count())]];
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

    Dictionary<TileType, int> GetSurroundingTiles(int x, int z)
    {
        var amounts = new Dictionary<TileType, int>();

        int[,] tilesToCheck = new int[,] { { x - 1, z - 1 }, { x, z - 1 }, { x + 1, z - 1 }, { x - 1, z }, { x - 1, z + 1 }, { x, z + 1 }, { x + 1, z + 1 }, { x + 1, z } };

        for (int i = 0; i < tilesToCheck.GetLength(0); i++)
        {
            var xToCheck = tilesToCheck[i, 0];
            var zToCheck = tilesToCheck[i, 1];

            if (xToCheck >= 0 && zToCheck >= 0 && xToCheck < mapLength && zToCheck < mapWidth)
            {
                if (map[xToCheck, zToCheck] != TileType.Empty)
                {
                    var type = map[xToCheck, zToCheck];

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
        var amount = GetSurroundingTiles(x, z);

        var total = 0;

        foreach (var i in amount.Values)
        {
            total += 1;
        }

        if (total == 0)
        {
            return GetRandomTile(new List<TileType>());
        }

        if (amount.ContainsKey(TileType.Forest) && amount.ContainsKey(TileType.Desert))
        {
            return tileMap[TileType.Plains];
        }

        if (amount.ContainsKey(TileType.Forest))
        {
            var forestChance = amount[TileType.Forest] * ForestWeightFactor;

            return Random.Range(0f, total) <= forestChance ? tileMap[TileType.Forest] : tileMap[TileType.Plains];
        }

        if (amount.ContainsKey(TileType.Desert))
        {
            var desertChance = amount[TileType.Desert] * DesertWeightFactor;

            return Random.Range(0f, total) <= desertChance ? tileMap[TileType.Desert] : tileMap[TileType.Plains];
        }

        var plainsChance = amount[TileType.Plains] * PlainsWeightFactor;

        if (Random.Range(0f, total) <= plainsChance)
        {
            return tileMap[TileType.Plains]; 
        }
        else
        {
            var excluded = new List<TileType>() { TileType.Plains }; 

            return GetRandomTile(excluded);
        }
    }
}