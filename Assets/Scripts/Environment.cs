﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public static Environment M;

    [SerializeField] private List<EnvironmentTile> AccessibleTiles;
    [SerializeField] private List<EnvironmentTile> InaccessibleTiles;
    [SerializeField] private Vector2Int Size;
    [SerializeField] private float AccessiblePercentage;

    private EnvironmentTile[][] mMap;
    private List<EnvironmentTile> mAll;
    private List<EnvironmentTile> mToBeTested;
    private List<EnvironmentTile> mLastSolution;

    private readonly Vector3 NodeSize = Vector3.one * 9.0f; 
    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;

    public List<EnvironmentTile> StartPos;

    private void Awake()
    {
        if (M == null)
        {
            M = this;
        }
        else if (M != this)
        {
            Destroy(this);
        }

        StartPos = new List<EnvironmentTile>();
        mAll = new List<EnvironmentTile>();
        mToBeTested = new List<EnvironmentTile>();
    }

    private void OnDrawGizmos()
    {
        // Draw the environment nodes and connections if we have them
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    if (mMap[x][y].Connections != null)
                    {
                        for (int n = 0; n < mMap[x][y].Connections.Count; ++n)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(mMap[x][y].Position, mMap[x][y].Connections[n].Position);
                        }
                    }

                    // Use different colours to represent the state of the nodes
                    Color c = Color.white;
                    if ( !mMap[x][y].IsAccessible )
                    {
                        c = Color.red;
                    }
                    else
                    {
                        if(mLastSolution != null && mLastSolution.Contains( mMap[x][y] ))
                        {
                            c = Color.green;
                        }
                        else if (mMap[x][y].Visited)
                        {
                            c = Color.yellow;
                        }
                    }

                    Gizmos.color = c;
                    Gizmos.DrawWireCube(mMap[x][y].Position, NodeSize);
                }
            }
        }
    }

    private void Generate()
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage


        mMap = new EnvironmentTile[Size.x][];
        for (int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];
        }

        Misc();
        MakeBase();
    }

    private void Misc()
    {
        int halfWidth = Size.x / 2;
        int halfHeight = Size.y / 2;
        Vector3 position = new Vector3( -(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize) );
        bool start = true;

        for ( int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];
            for ( int y = 0; y < Size.y; ++y)
            {
                bool isAccessible = start || Random.value < AccessiblePercentage;
                List<EnvironmentTile> tiles = isAccessible ? AccessibleTiles : InaccessibleTiles;
                EnvironmentTile prefab = tiles[Random.Range(0, tiles.Count)];
                EnvironmentTile tile = Instantiate(prefab, position, Quaternion.identity, transform);
                tile.Position = new Vector3( position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2));
                tile.IsAccessible = isAccessible;
                tile.gameObject.name = string.Format("Tile({0},{1})", x, y);
                mMap[x][y] = tile;
                mAll.Add(tile);

                if(start)
                {
                    //Start = tile;
                }

                position.z += TileSize;
                start = false;
            }

            position.x += TileSize;
            position.z = -(halfHeight * TileSize);
        }
    }
    void MakeBase()
    {
        Vector2 centre = new Vector2(Size.x / 2, Size.y / 2);
        int x = (int)centre.x;
        int y = (int)centre.y;

        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                if (i == 0 && j == 0)
                    Spawn(PlayerBase.M.BaseTile, x + i, y + j).IsAccessible = false;
                else if(i <= 1 && i >= -1 && j <= 1 && j >= -1)
                    Spawn(AccessibleTiles[0], x + i, y + j).IsAccessible = false;
                else
                {
                    EnvironmentTile temp = Spawn(AccessibleTiles[0], x + i, y + j);
                    temp.IsAccessible = true;
                    StartPos.Add(temp);
                    mAll.Add(temp);

                    if (Vector3.Distance(temp.Position, PlayerBase.M.DoorFront.transform.position) < 5)
                    {
                        PlayerBase.M.DoorTile = temp;
                    }
                }
            }
        }
    }

    EnvironmentTile Spawn(EnvironmentTile prefab, int x, int y)
    {
        Destroy(mMap[x][y].gameObject);
        Vector3 tilePos = new Vector3(mMap[x][y].Position.x - (TileSize / 2), mMap[x][y].Position.y - TileHeight, mMap[x][y].Position.z - (TileSize / 2));
        EnvironmentTile tile;

        if (prefab.gameObject.activeInHierarchy)
        {
            tile = prefab;
            tile.transform.position = tilePos;
        }
        else
        {
           tile = Instantiate(prefab, tilePos, Quaternion.identity, transform);
        }

        tile.Position = new Vector3(tilePos.x + (TileSize / 2), TileHeight, tilePos.z + (TileSize / 2)); //Centre-Top of the tile
        mMap[x][y] = tile;

        return tile;
    }

    public void Replace(EnvironmentTile tile)
    {
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                if(mMap[x][y] == tile)
                {
                    Destroy(tile.gameObject);
                    Vector3 position = new Vector3(mMap[x][y].Position.x - (TileSize / 2), mMap[x][y].Position.y - TileHeight, mMap[x][y].Position.z - (TileSize / 2));
                    mMap[x][y] = Instantiate(AccessibleTiles[0], position, Quaternion.identity, transform);
                }
            }
        }
    }

    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacnt nodes
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                EnvironmentTile tile = mMap[x][y];
                tile.Connections = new List<EnvironmentTile>();

                //8-Way Directional Connections
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (InRange(x + i, y + j))
                        {
                            tile.Connections.Add(mMap[x + i][y + j]);
                        }
                    }
                }
            }
        }
    }

    bool InRange(float x, float y)
    {
        return x < Size.x && x >= 0 && y < Size.y && y >= 0;
    }

    public EnvironmentTile ClosestTile(Vector3 pos)
    {
        EnvironmentTile closest = mMap[0][0];

        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                if(Vector3.Distance(pos, mMap[x][y].Position) < Vector3.Distance(pos, closest.Position))
                    closest = mMap[x][y];
            }
        }

        return closest;
    }

    private float Distance(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        EnvironmentTile directConnection = a.Connections.Find(c => c == b);
        if (directConnection != null)
        {
            result = TileSize;
        }
        return result;
    }

    public float Heuristic(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance(a.Position, b.Position);
    }

    public void GenerateWorld()
    {
        Generate();
        SetupConnections();
    }

    public void CleanUpWorld()
    {
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    Destroy(mMap[x][y].gameObject);
                }
            }
        }
    }

    public List<EnvironmentTile> Solve(EnvironmentTile begin, EnvironmentTile destination)
    {
        List<EnvironmentTile> result = null;
        if (begin != null && destination != null)
        {
            // Nothing to solve if there is a direct connection between these two locations
            EnvironmentTile directConnection = begin.Connections.Find(c => c == destination);
            if (directConnection == null)
            {
                // Set all the state to its starting values
                mToBeTested.Clear();

                for( int count = 0; count < mAll.Count; ++count )
                {
                    mAll[count].Parent = null;
                    mAll[count].Global = float.MaxValue;
                    mAll[count].Local = float.MaxValue;
                    mAll[count].Visited = false;
                }

                // Setup the start node to be zero away from start and estimate distance to target
                EnvironmentTile currentNode = begin;
                currentNode.Local = 0.0f;
                currentNode.Global = Heuristic(begin, destination);

                // Maintain a list of nodes to be tested and begin with the start node, keep going
                // as long as we still have nodes to test and we haven't reached the destination
                mToBeTested.Add(currentNode);

                while (mToBeTested.Count > 0 && currentNode != destination)
                {
                    // Begin by sorting the list each time by the heuristic
                    mToBeTested.Sort((a, b) => (int)(a.Global - b.Global));

                    // Remove any tiles that have already been visited
                    mToBeTested.RemoveAll(n => n.Visited);

                    // Check that we still have locations to visit
                    if (mToBeTested.Count > 0)
                    {
                        // Mark this note visited and then process it
                        currentNode = mToBeTested[0];
                        currentNode.Visited = true;

                        // Check each neighbour, if it is accessible and hasn't already been 
                        // processed then add it to the list to be tested 
                        for (int count = 0; count < currentNode.Connections.Count; ++count)
                        {
                            EnvironmentTile neighbour = currentNode.Connections[count];

                            if (!neighbour.Visited && neighbour.IsAccessible)
                            {
                                mToBeTested.Add(neighbour);
                            }

                            // Calculate the local goal of this location from our current location and 
                            // test if it is lower than the local goal it currently holds, if so then
                            // we can update it to be owned by the current node instead 
                            float possibleLocalGoal = currentNode.Local + Distance(currentNode, neighbour);
                            if (possibleLocalGoal < neighbour.Local)
                            {
                                neighbour.Parent = currentNode;
                                neighbour.Local = possibleLocalGoal;
                                neighbour.Global = neighbour.Local + Heuristic(neighbour, destination);
                            }
                        }
                    }
                }

                // Build path if we found one, by checking if the destination was visited, if so then 
                // we have a solution, trace it back through the parents and return the reverse route
                if (destination.Visited)
                {
                    result = new List<EnvironmentTile>();
                    EnvironmentTile routeNode = destination;

                    while (routeNode.Parent != null)
                    {
                        result.Add(routeNode);
                        routeNode = routeNode.Parent;
                    }
                    result.Add(routeNode);
                    result.Reverse();

                    Debug.LogFormat("Path Found: {0} steps {1} long", result.Count, destination.Local);
                }
                else
                {
                    Debug.LogWarning("Path Not Found");
                }
            }
            else
            {
                result = new List<EnvironmentTile>();
                result.Add(begin);
                result.Add(destination);
                Debug.LogFormat("Direct Connection: {0} <-> {1} {2} long", begin, destination, TileSize);
            }
        }
        else
        {
            Debug.LogWarning("Cannot find path for invalid nodes");
        }

        mLastSolution = result;

        return result;
    }
}
