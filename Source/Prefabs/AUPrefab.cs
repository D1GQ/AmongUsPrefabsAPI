﻿using Il2CppInterop.Runtime;
using UnityEngine;

namespace AmongUsPrefabs.Prefabs;

/// <summary>
/// A base class for handling prefab management within the Among Us game.
/// This class provides functionality to load, cache, retrieve, and remove prefabs dynamically.
/// It ensures that prefabs can be instantiated, stored, and managed efficiently without requiring Unity Editor access.
/// </summary>
public static class AUPrefab
{
    private static readonly List<string> CachedTypes = [];
    private static readonly Dictionary<string, GameObject?> Cached = [];
    private static readonly Dictionary<string, GameObject?> TempCached = [];

    private static T? LoadPrefab<T>(GameObject? parent = null, int cacheType = 0) where T : Component
    {
        string name = typeof(T).Name;
        Component? obj = Resources.FindObjectsOfTypeAll(Il2CppType.Of<T>()).FirstOrDefault(com => com.Cast<T>().GetIl2CppType() == Il2CppType.Of<T>())?.Cast<T>();
        if (obj != null)
        {
            var instance = parent != null ? UnityEngine.Object.Instantiate(obj.gameObject, parent.transform) : UnityEngine.Object.Instantiate(obj.gameObject);

            instance.name = instance.name.Replace("(Clone)", "");
            if (cacheType == 1)
            {
                instance.name += "(AUPrefab)";
                CachedTypes.Add(name);
                Cached[name] = instance.gameObject;
                UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
            }
            else if (cacheType == 2)
            {
                instance.name += "(AUTemp)";
                TempCached[name] = instance;
            }

            return instance.GetComponent<T>();
        }

        return null;
    }

    /// <summary>
    /// Copies a prefab of type <typeparamref name="T"/> without caching it.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <param name="parent">The parent GameObject to attach the instantiated prefab to (optional).</param>
    /// <returns>An instance of the requested prefab component if found, otherwise null.</returns>
    public static T? CopyPrefab<T>(GameObject? parent = null) where T : Component
    {
        return LoadPrefab<T>(parent);
    }

    /// <summary>
    /// Retrieves a temporarily cached prefab of type <typeparamref name="T"/>.
    /// If the prefab is not already cached, it will be loaded and temporarily cached for future use.
    /// Note: Temporarily cached prefabs can be destroyed on load, unlike cached prefabs.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <returns>An instance of the requested prefab component if found, otherwise null.</returns>
    public static T? GetTempPrefab<T>() where T : Component
    {
        if (TempCached.TryGetValue(typeof(T).Name, out var obj) && obj != null)
        {
            return obj.GetComponent<T>();
        }

        return LoadPrefab<T>(null, 2);
    }

    /// <summary>
    /// Retrieves a cached prefab of type <typeparamref name="T"/>.
    /// If the prefab is not already cached, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <returns>The cached instance of the requested prefab.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the requested prefab is not cached.</exception>
    public static T? GetCachedPrefab<T>() where T : Component
    {
        if (!CachedTypes.Contains(typeof(T).Name))
            throw new InvalidOperationException("Unable to get a prefab that hasn't been cached!");

        if (Cached.TryGetValue(typeof(T).Name, out var obj) && obj != null)
        {
            return obj.GetComponent<T>();
        }

        return null;
    }

    /// <summary>
    /// Caches a prefab of type <typeparamref name="T"/> to allow retrieval later.
    /// If the prefab is already cached, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the prefab is already cached.</exception>
    public static void CatchPrefab<T>() where T : Component
    {
        if (CachedTypes.Contains(typeof(T).Name))
            throw new InvalidOperationException("Unable to cache a prefab that's already been cached!");

        LoadPrefab<T>(null, 1);
    }

    /// <summary>
    /// Removes a cached prefab of type <typeparamref name="T"/>.
    /// If the prefab is not cached, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the prefab is not cached.</exception>
    public static void UncachePrefab<T>() where T : Component
    {
        if (!CachedTypes.Contains(typeof(T).Name))
            throw new InvalidOperationException("Unable to uncache a prefab that hasn't been cached!");

        if (Cached.TryGetValue(typeof(T).Name, out var obj))
        {
            CachedTypes.Remove(typeof(T).Name);
            Cached.Remove(typeof(T).Name);
            UnityEngine.Object.Destroy(obj);
        }
    }

    /// <summary>
    /// Clears all cached prefabs, removing their references and destroying their instances.
    /// </summary>
    public static void UncacheAll()
    {
        CachedTypes.Clear();
        foreach (var obj in Cached.Values)
        {
            UnityEngine.Object.Destroy(obj);
        }
        Cached.Clear();
    }
}