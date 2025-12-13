using Godot;
using System;
using System.Collections.Generic;

public enum Tile
{
    Grey,
    Green,
    Blue,
    RailroadX,
    RailroadY
}

public static class TileManager
{
    public static Vector2I StartCoordinate = new Vector2I(15, -4);

    private static Dictionary<Tile, Vector2I> tileCoordinates = new Dictionary<Tile, Vector2I>()
    {
        { Tile.Grey, new Vector2I(0, 3) },
        { Tile.Green, new Vector2I(0, 0) },
        { Tile.Blue, new Vector2I(1, 2) },
        { Tile.RailroadX, new Vector2I(1, 5) },
        { Tile.RailroadY, new Vector2I(0, 5) },
    };

    public static Vector2I GetTileSetCoordinate(Tile tile)
    {
        if (!tileCoordinates.ContainsKey(tile))
        {
            throw new Exception($"Coordinate not found for tile {tile}");
        }

        return tileCoordinates[tile];
    }

    public static Vector2I GetTileCoordinate(Vector2I coord)
    {
        return StartCoordinate + coord;
    }
}