using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public enum Tile
{
    Grey,
    Green,
    Blue,
    RailX,
    RailY,
    RailLeftCorner,
    RailRightCorner,
    RailTopCorner,
    RailBottomCorner,
    RailMergeLeftX,
    RailMergeRightX,
    RailMergeLeftY,
    RailMergeRightY,
    RailMergeBottomX,
    RailMergeBottomY,
    RailMergeTopX,
    RailMergeTopY,
    StraightArrow,
    BentArrow,
    PlatformPurple,
    Invalid,
}

public static class TileManager
{
    public static Vector2I StartCoordinate = new Vector2I(15, -4);

    private static Dictionary<Tile, Vector2I> tileAtlasCoords = new Dictionary<Tile, Vector2I>()
    {
        { Tile.Grey, new Vector2I(0, 3) },
        { Tile.Green, new Vector2I(0, 0) },
        { Tile.Blue, new Vector2I(1, 2) },
        { Tile.RailX, new Vector2I(1, 5) },
        { Tile.RailY, new Vector2I(0, 5) },
        { Tile.RailLeftCorner, new Vector2I(2, 5) },
        { Tile.RailRightCorner, new Vector2I(3, 5) },
        { Tile.RailBottomCorner, new Vector2I(4, 5) },
        { Tile.RailTopCorner, new Vector2I(5, 5) },
        { Tile.RailMergeLeftX, new Vector2I(2, 3) },
        { Tile.RailMergeRightX, new Vector2I(3, 3) },
        { Tile.RailMergeLeftY, new Vector2I(4, 3) },
        { Tile.RailMergeRightY, new Vector2I(5, 3) },
        { Tile.RailMergeBottomX, new Vector2I(2, 4) },
        { Tile.RailMergeBottomY, new Vector2I(3, 4) },
        { Tile.RailMergeTopX, new Vector2I(4, 4) },
        { Tile.RailMergeTopY,  new Vector2I(5, 4) },
        { Tile.StraightArrow, new Vector2I(3, 8) },
        { Tile.BentArrow, new Vector2I(3, 9) },
        { Tile.PlatformPurple, new Vector2I(0, 8) },
    };

    private static Dictionary<Vector2I, Tile> tilesAtCoord = new Dictionary<Vector2I, Tile>()
    {
        { new Vector2I(0, 3), Tile.Grey },
        { new Vector2I(0, 0), Tile.Green },
        { new Vector2I(1, 2), Tile.Blue },
        { new Vector2I(1, 5), Tile.RailX },
        { new Vector2I(0, 5), Tile.RailY },
        { new Vector2I(2, 5), Tile.RailLeftCorner },
        { new Vector2I(3, 5), Tile.RailRightCorner },
        { new Vector2I(4, 5), Tile.RailBottomCorner },
        { new Vector2I(5, 5), Tile.RailTopCorner },
        { new Vector2I(2, 3), Tile.RailMergeLeftX },
        { new Vector2I(3, 3), Tile.RailMergeRightX },
        { new Vector2I(4, 3), Tile.RailMergeLeftY },
        { new Vector2I(5, 3), Tile.RailMergeRightY },
        { new Vector2I(2, 4), Tile.RailMergeBottomX },
        { new Vector2I(3, 4), Tile.RailMergeBottomY },
        { new Vector2I(4, 4), Tile.RailMergeTopX },
        { new Vector2I(5, 4), Tile.RailMergeTopY },
        { new Vector2I(3, 8), Tile.StraightArrow },
        { new Vector2I(3, 9), Tile.BentArrow },
        { new Vector2I(0, 8), Tile.PlatformPurple }
    };

    public static ImmutableHashSet<Tile> CORNER_TILES = [
        Tile.RailLeftCorner,
        Tile.RailRightCorner,
        Tile.RailBottomCorner,
        Tile.RailTopCorner
    ];
    public static ImmutableHashSet<Tile> CORNERS_FOR_POS_X = CORNER_TILES.Union([Tile.RailMergeLeftY, Tile.RailMergeTopY]);
    public static ImmutableHashSet<Tile> CORNERS_FOR_NEG_X = CORNER_TILES.Union([Tile.RailMergeRightY, Tile.RailMergeBottomY]);
    public static ImmutableHashSet<Tile> CORNERS_FOR_POS_Y = CORNER_TILES.Union([Tile.RailMergeRightX, Tile.RailMergeTopX]);
    public static ImmutableHashSet<Tile> CORNERS_FOR_NEG_Y = CORNER_TILES.Union([Tile.RailMergeLeftX, Tile.RailMergeBottomX]);

    public static Dictionary<Direction, ImmutableHashSet<Tile>> cornerTilesForDirection = new Dictionary<Direction, ImmutableHashSet<Tile>>()
    {
        { Direction.PosX, CORNERS_FOR_POS_X },
        { Direction.PosY, CORNERS_FOR_POS_Y },
        { Direction.NegX, CORNERS_FOR_NEG_X },
        { Direction.NegY, CORNERS_FOR_NEG_Y },
    };

    public static ImmutableHashSet<Tile> SWITCHES_FOR_POS_X = [Tile.RailMergeLeftX, Tile.RailMergeTopX];
    public static ImmutableHashSet<Tile> SWITCHES_FOR_POS_Y = [Tile.RailMergeRightY, Tile.RailMergeTopY];
    public static ImmutableHashSet<Tile> SWITCHES_FOR_NEG_X = [Tile.RailMergeRightX, Tile.RailMergeBottomX];
    public static ImmutableHashSet<Tile> SWITCHES_FOR_NEG_Y = [Tile.RailMergeLeftY, Tile.RailMergeBottomY];

    public static Dictionary<Direction, ImmutableHashSet<Tile>> switchTilesForDirection = new Dictionary<Direction, ImmutableHashSet<Tile>>()
    {
        { Direction.PosX, SWITCHES_FOR_POS_X },
        { Direction.PosY, SWITCHES_FOR_POS_Y },
        { Direction.NegX, SWITCHES_FOR_NEG_X },
        { Direction.NegY, SWITCHES_FOR_NEG_Y }
    };

    public static Dictionary<Tile, Direction> newDirectionForPosX = new Dictionary<Tile, Direction>()
    {
        { Tile.RailLeftCorner, Direction.PosY },
        { Tile.RailTopCorner, Direction.NegY },
        { Tile.RailMergeLeftY, Direction.PosY },
        { Tile.RailMergeTopY, Direction.NegY }
    };

    public static Dictionary<Tile, Direction> newDirectionForPosY = new Dictionary<Tile, Direction>()
    {
        { Tile.RailRightCorner, Direction.PosX },
        { Tile.RailTopCorner, Direction.NegX },
        { Tile.RailMergeRightX, Direction.PosX },
        { Tile.RailMergeTopX, Direction.NegX }
    };

    public static Dictionary<Tile, Direction> newDirectionForNegX = new Dictionary<Tile, Direction>()
    {
        { Tile.RailRightCorner, Direction.NegY },
        { Tile.RailBottomCorner, Direction.PosY },
        { Tile.RailMergeRightY, Direction.NegY },
        { Tile.RailMergeBottomY, Direction.PosY }
    };

    public static Dictionary<Tile, Direction> newDirectionForNegY = new Dictionary<Tile, Direction>()
    {
        { Tile.RailLeftCorner, Direction.NegX },
        { Tile.RailBottomCorner, Direction.PosX },
        { Tile.RailMergeLeftX, Direction.NegX },
        { Tile.RailMergeBottomX, Direction.PosX }
    };

    public static Dictionary<Tile, Direction> bentDirectionForPosX = new Dictionary<Tile, Direction>()
    {
        { Tile.RailMergeLeftX, Direction.PosY },
        { Tile.RailMergeTopX, Direction.NegX }
    };

    public static Dictionary<Tile, Direction> bentDirectionForPosY = new Dictionary<Tile, Direction>()
    {
        { Tile.RailMergeRightY, Direction.PosX },
        { Tile.RailMergeTopY, Direction.NegX }
    };

    public static Dictionary<Tile, Direction> bentDirectionForNegX = new Dictionary<Tile, Direction>()
    {
        { Tile.RailMergeRightX, Direction.NegY },
        { Tile.RailMergeBottomX, Direction.PosY }
    };

    public static Dictionary<Tile, Direction> bentDirectionForNegY = new Dictionary<Tile, Direction>()
    {
        { Tile.RailMergeLeftY, Direction.NegX },
        { Tile.RailMergeBottomY, Direction.PosX }
    };

    public static Vector2I GetTileAtlasCoordinate(Tile tile)
    {
        if (!tileAtlasCoords.ContainsKey(tile))
        {
            throw new Exception($"Coordinate not found for tile {tile}");
        }
        return tileAtlasCoords[tile];
    }

    public static Vector2I GetTileCoordinate(Vector2I coord)
    {
        return StartCoordinate + coord;
    }

    public static Tile GetTileForAtlasCoord(Vector2I atlasCoord)
    {
        if (!tilesAtCoord.ContainsKey(atlasCoord))
        {
            return Tile.Invalid;
        }
        return tilesAtCoord[atlasCoord];
    }

    /// <summary>
    /// Determines whether the given tile is a corner for a train moving in the given direction.
    /// </summary>
    public static bool IsCornerForDirection(Tile tile, Direction direction)
    {
        return cornerTilesForDirection[direction].Contains(tile);
    }

    /// <summary>
    /// Determines whether the given tile is a switch for a train moving in the given direction.
    /// </summary>
    public static bool IsSwitchForDirection(Tile tile, Direction direction)
    {
        return switchTilesForDirection[direction].Contains(tile);
    }

    public static Direction GetNewDirectionForCorner(Tile tile, Direction oldDirection)
    {
        switch (oldDirection)
        {
            case Direction.PosX:
                return newDirectionForPosX[tile];
            case Direction.PosY:
                return newDirectionForPosY[tile];
            case Direction.NegX:
                return newDirectionForNegX[tile];
            case Direction.NegY:
                return newDirectionForNegY[tile];
            default:
                throw new Exception($"Unsupported direction {oldDirection}");
        }
    }

    public static Direction GetBentDirectionForSwitch(Tile tile, Direction oldDirection)
    {
        switch (oldDirection)
        {
            case Direction.PosX:
                return bentDirectionForPosX[tile];
            case Direction.PosY:
                return bentDirectionForPosY[tile];
            case Direction.NegX:
                return bentDirectionForNegX[tile];
            case Direction.NegY:
                return bentDirectionForNegY[tile];
            default:
                throw new Exception($"Unsupported direction {oldDirection}");
        }
    }
}