using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Creates a random path, of tile instances, following a set of rules and recolor them to a gradient
/// </summary>
public class PathCreator : MonoBehaviour
{
    public BEController beController;
    public GameObject pathTile;
    public GameObject pathGoal;

    Vector3 pathTilePosition;
    Renderer[] tiles;

    public int numberOfPathTiles;

    int lineSize;
    float tileSize;
    public float spaceBetweenTiles;
    public int minLineSize;
    public int maxLineSize;
    public int maxConsecutiveTurns;
    public Color startColor;
    public Color endColor;
    int lastDirection;
    int lastMin;
    int lastMax;
    int consecutiveTurns;
    string lastDirectionString;

    void Start()
    {
        lastMin = minLineSize;
        lastMax = maxLineSize;
    }

    private void OnValidate()
    {
        if (numberOfPathTiles < 2)
        {
            numberOfPathTiles = 2;
        }
        if (minLineSize < 2)
        {
            minLineSize = 2;
        }
        if (minLineSize > numberOfPathTiles)
        {
            minLineSize = numberOfPathTiles;
        }
        if (maxLineSize < 2)
        {
            maxLineSize = 2;
        }
        if (lastMin != minLineSize)
        {
            if (minLineSize > maxLineSize)
            {
                minLineSize = maxLineSize;
            }
            lastMin = minLineSize;
        }
        if (lastMax != maxLineSize)
        {
            if (minLineSize > maxLineSize)
            {
                maxLineSize = minLineSize;
            }
            lastMax = maxLineSize;
        }
        if (maxConsecutiveTurns < 1)
        {
            maxConsecutiveTurns = 1;
        }
    }

    Color ColorGradient(Color minColor, Color maxColor, int index, int size)
    {
        // excerpt from https://stackoverflow.com/a/2011839
        var rAverage = minColor.r + ((maxColor.r - minColor.r) * index / size);
        var gAverage = minColor.g + ((maxColor.g - minColor.g) * index / size);
        var bAverage = minColor.b + ((maxColor.b - minColor.b) * index / size);
        return new Color(rAverage, gAverage, bAverage);
    }

    /// <summary>
    /// Creates a random path of equal objects following a set of rules and recolor them to a gradient
    /// </summary>
    /// <returns>List of the path tile Renderers</returns>
    public Renderer[] CreatePath()
    {
        //--create path--
        tileSize = pathTile.transform.localScale.x;
        pathTilePosition = pathTile.transform.position;
        lineSize = 1;
        consecutiveTurns = 0;

        if (tiles != null)
        {
            for (int i = 1; i < tiles.Length; i++)
            {
                pathGoal.transform.SetParent(null);
                Destroy(tiles[i].gameObject);
            }
        }

        tiles = new Renderer[numberOfPathTiles];
        foreach (BETargetObject targetObject in BEController.beTargetObjectList)
        {
            targetObject.transform.position = Vector3.zero;
            targetObject.transform.rotation = Quaternion.identity;
        }
        lastDirection = 0;
        lastDirectionString = "";
        for (int i = 0; i < numberOfPathTiles; i++)
        {

            int oppositeDirection = lastDirection - 2;
            if (oppositeDirection < 0)
            {
                oppositeDirection += 4;
            }

            if (i == 0)
            {
                pathTile.name = "tile" + i;
                tiles[i] = pathTile.GetComponent<Renderer>();
                pathTile.GetComponent<Renderer>().material.color = ColorGradient(startColor, endColor, 0, numberOfPathTiles);
            }
            else
            {
                int direction = 0;

                exclude = new HashSet<int> { };
                ExcludeDirection(oppositeDirection);
                for (int direction_ = 0; direction_ < 4; direction_++)
                {
                    if (lineSize >= maxLineSize)
                    {
                        if (lastDirection == direction_)
                        {
                            ExcludeDirection(direction_);
                        }
                    }


                    if (consecutiveTurns + 1 >= maxConsecutiveTurns)
                    {
                        if (GetDirectionString(direction_, lastDirection) == lastDirectionString)
                        {
                            ExcludeDirection(direction_);
                        }
                    }
                }

                if (lineSize > minLineSize - 1)
                {
                    direction = RandDirection(exclude);
                }
                else
                {
                    direction = lastDirection;
                }

                if (lastDirectionString == GetDirectionString(direction, lastDirection))
                {
                    consecutiveTurns++;
                }
                else
                {
                    if (GetDirectionString(direction, lastDirection) != "")
                    {
                        lastDirectionString = GetDirectionString(direction, lastDirection);
                        consecutiveTurns = 0;
                    }
                }

                if (direction != lastDirection)
                {
                    lineSize = 2;
                }
                else
                {
                    lineSize++;
                }

                lastDirection = direction;

                pathTilePosition += GetDirection(direction);

                GameObject tile = Instantiate(pathTile, pathTilePosition, Quaternion.identity);
                tile.name = "tile" + i;

                tiles[i] = tile.GetComponent<Renderer>();
                tile.GetComponent<Renderer>().material.color = ColorGradient(startColor, endColor, i + 1, numberOfPathTiles);
            }

        }

        pathGoal.transform.SetParent(tiles[tiles.Length - 1].transform);
        pathGoal.transform.localPosition = Vector3.zero;

        return tiles;
    }

    string GetDirectionString(int direction, int lastDirection)
    {
        string lastDirectionString = "";
        int left = lastDirection + 1;
        int right = lastDirection - 1;

        if (left > 3)
        {
            left = 0;
        }
        if (right < 0)
        {
            right = 3;
        }

        if (direction == left)
        {
            lastDirectionString = "left";
        }
        else if (direction == right)
        {
            lastDirectionString = "right";
        }

        return lastDirectionString;
    }

    Vector3 GetDirection(int direction)
    {
        Vector3 pathTilePosition = Vector3.zero;
        switch (direction)
        {
            case 0:
                pathTilePosition = new Vector3(0, 0, tileSize + spaceBetweenTiles);
                break;
            case 1:
                pathTilePosition = new Vector3(-(tileSize + spaceBetweenTiles), 0, 0);
                break;
            case 2:
                pathTilePosition = new Vector3(0, 0, -(tileSize + spaceBetweenTiles));
                break;
            case 3:
                pathTilePosition = new Vector3(tileSize + spaceBetweenTiles, 0, 0);
                break;
            default:
                break;
        }

        return pathTilePosition;
    }

    HashSet<int> exclude = new HashSet<int>() { };
    void ExcludeDirection(int direction)
    {
        if (!exclude.Contains(direction))
        {
            exclude.Add(direction);
        }
    }

    private int RandDirection(HashSet<int> exclude)
    {
        var range = Enumerable.Range(0, 4).Where(i => !exclude.Contains(i));

        var rand = new System.Random();
        int index = rand.Next(0, 4 - exclude.Count);
        return range.ElementAt(index);
    }

}