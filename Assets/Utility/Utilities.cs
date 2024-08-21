using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class Extentions
{
    public static Transform Clear(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        return transform;
    }

    /// <summary>
    /// Apply an action to the given component and all of its children of same type
    /// </summary>
    /// <typeparam name="T">Type of the component</typeparam>
    /// <param name="component">Root component on which to apply the action</param>
    /// <param name="action">Action to apply</param>
    /// <returns><paramref name="component"/></returns>
    public static T Apply<T>(this T component, Action<T> action) where T : Component
    {
        action(component);
        foreach (Transform t in component.transform)
        {
            foreach (T c in t.GetComponents<T>())
                c.Apply(action);
        }

        return component;
    }

    /// <summary>
    /// Apply an action to the given object and all of its children of same type
    /// </summary>
    /// <param name="gameobject">Root object on which to apply the action</param>
    /// <param name="action">Action to apply</param>
    /// <returns><paramref name="gameobject"/></returns>
    public static GameObject Apply(this GameObject gameobject, Action<GameObject> action)
    {
        action(gameobject);
        foreach (Transform t in gameobject.transform)
        {
            t.gameObject.Apply(action);
        }

        return gameobject;
    }

    /// <summary>
    /// Extension method to convert a range of float
    /// </summary>
    /// <param name="originalStart">original Start</param>
    /// <param name="originalEnd">original End</param>
    /// <param name="newStart">new Start</param>
    /// <param name="newEnd">new End</param>
    /// <param name="value">value</param>
    /// <returns></returns>
    public static float ConvertRange(float originalStart, float originalEnd, float newStart, float newEnd, float value)
    {
        float originalDiff = originalEnd - originalStart;
        if (originalDiff == 0)
            return newStart;

        float newDiff = newEnd - newStart;
        float finalValue = (((value - originalStart) * newDiff) / originalDiff) + newStart;
        return finalValue;
    }
}

/// <summary>
/// Add custom function to IEnumerator
/// </summary>
public static class WaitFor
{
    /// <summary>
    /// Wait for frame by using frameCount
    /// </summary>
    /// <param name="frameCount"></param>
    /// <returns></returns>
    public static IEnumerator Frames(int frameCount)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
    }

    public static IEnumerator ActionAfterFrames(int frame, Action action)
    {
        yield return Frames(frame); // wait for x frames
        action();
    }

    public static IEnumerator Seconds(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds); // wait for x seconds
        action();
    }

    public static IEnumerator EndFrame(Action action)
    {
        yield return new WaitForEndOfFrame(); // wait end of frame
        action();
    }
}