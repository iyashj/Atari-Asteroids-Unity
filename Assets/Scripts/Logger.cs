using UnityEngine;

public static class Logger
{
    public static void Info(string log)
    {
        Debug.Log(log);
    }
        
    public static void Warn(string log)
    {
        Debug.LogWarning(log);
    }
        
    public static void Error(string log)
    {
        Debug.LogError(log);
    }
}