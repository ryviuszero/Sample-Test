using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TilePos
{
  int xPos, yPos;

  Vector2[] uvs;

  public TilePos(int x, int y)
  {
    xPos = x;
    yPos = y;
    // TODO 这里的16f, 0.001f的作用是什么？
    uvs = new Vector2[]
    {
        new Vector2(xPos/16f + .001f, yPos/16f + .001f),
        new Vector2(xPos/16f+ .001f, (yPos+1)/16f - .001f),
        new Vector2((xPos+1)/16f - .001f, (yPos+1)/16f - .001f),
        new Vector2((xPos+1)/16f - .001f, yPos/16f+ .001f),
    };
  }

   public Vector2[] GetUVs()
   {
        return uvs;
   }

   public static Dictionary<Tile, TilePos> tiles = new Dictionary<Tile, TilePos>()
    {
        {Tile.Dirt, new TilePos(0,0)},
        {Tile.Grass, new TilePos(1,0)},
        {Tile.GrassSide, new TilePos(0,1)},
        {Tile.Stone, new TilePos(0,2)},
        {Tile.TreeSide, new TilePos(0,4)},
        {Tile.TreeCX, new TilePos(0,3)},
        {Tile.Leaves, new TilePos(0,5)},
    };
  
}

public enum Tile {Dirt, Grass, GrassSide, Stone, TreeSide, TreeCX, Leaves}
