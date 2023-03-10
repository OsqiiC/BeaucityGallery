using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Watch 
{
    private static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    public static void StartWatch()
    {
        stopwatch.Start();
    }

    public static void ResetWatch()
    {
        stopwatch.Restart();
    }

    public static float GetTimeTicks()
    {
        return stopwatch.ElapsedTicks;
    }   
    public static float GetTimeMilliseconds()
    {
        return stopwatch.ElapsedMilliseconds;
    }

    public static void LogTime(string messege)
    {
        long ticks = stopwatch.ElapsedTicks;
        long ms = stopwatch.ElapsedMilliseconds;
        Debug.Log("\n" +
            +ticks + " ticks | " + ms + " ms" + " " + messege);
    }
    
    public static void LogTime()
    {
        long ticks = stopwatch.ElapsedTicks;
        long ms = stopwatch.ElapsedMilliseconds;
        Debug.Log("\n" +
            +ticks + " ticks | " + ms + " ms" + " " + "default log messege");
    }

    public static void ResetLogTime(string messege)
    {
        LogTime(messege);
        ResetWatch();
    }
}