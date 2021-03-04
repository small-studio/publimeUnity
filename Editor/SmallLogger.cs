using UnityEngine;
using System;
using System.Collections;


namespace SUBlime
{

public class SmallLogger
{
    [Flags]
    public enum LogType
    {
        Debug = 1,
        AssetPostProcessor = 2,
        PreImport = 4,
        PostImport = 8,
        Dependency = 16,
    }

    public static void Log(LogType type, string message)
    {
        if (SmallImporterWindow.logMask.HasFlag(type))
        {
            Debug.Log("[SMALL IMPORTER] " + message);
        }
    }

    public static void LogWarning(LogType type, string message)
    {
        if (SmallImporterWindow.logMask.HasFlag(type))
        {
            Debug.LogWarning("[SMALL IMPORTER] " + message);
        }
    }

    public static void LogError(LogType type, string message)
    {
        if (SmallImporterWindow.logMask.HasFlag(type))
        {
            Debug.LogError("[SMALL IMPORTER] " + message);
        }
    }
}

}