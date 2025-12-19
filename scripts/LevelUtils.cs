using System.Collections.Generic;

public enum Level
{
    LevelOne,
    LevelTwo,
    LevelThree
}

public static class LevelUtils
{
    private static Dictionary<Level, string> sceneNameForLevel = new Dictionary<Level, string>()
    {
        { Level.LevelOne, "res://scenes/LevelTwo.tscn" },
        { Level.LevelTwo, "res://scenes/LevelTwo.tscn" },
        { Level.LevelThree, "res://scenes/LevelTwo.tscn" },
    };

    public static string GetSceneNameForLevel(Level level)
    {
        return sceneNameForLevel[level];
    }
}