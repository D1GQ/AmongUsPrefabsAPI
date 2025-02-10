using Il2CppInterop.Runtime;
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
    private static readonly Dictionary<string, GameObject> Cached = [];

    /// <summary>
    /// Loads an asset of type <typeparamref name="T"/> from resources and optionally caches it.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <param name="parent">The parent GameObject to attach the instantiated prefab to (optional).</param>
    /// <param name="cache">Determines whether the prefab should be cached for future use.</param>
    /// <returns>An instance of the requested prefab component if found, otherwise null.</returns>
    private static T? LoadAsset<T>(GameObject? parent = null, bool cache = false) where T : Component
    {
        Component? obj = Resources.FindObjectsOfTypeAll(Il2CppType.Of<T>()).FirstOrDefault()?.Cast<T>();
        if (obj != null)
        {
            var instance = parent != null
                ? UnityEngine.Object.Instantiate(obj.gameObject, parent.transform)
                : UnityEngine.Object.Instantiate(obj.gameObject);
            instance.name = instance.name.Replace("(Clone)", "") + "(AUPrefab)";

            if (cache)
            {
                CachedTypes.Add(typeof(T).Name);
                Cached[typeof(T).Name] = instance.gameObject;
                UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
            }

            return instance.GetComponent<T>();
        }

        return null;
    }

    /// <summary>
    /// Loads a prefab of type <typeparamref name="T"/> without caching it.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <param name="parent">The parent GameObject to attach the instantiated prefab to (optional).</param>
    /// <returns>An instance of the requested prefab component if found, otherwise null.</returns>
    public static T? LoadPrefab<T>(GameObject? parent = null) where T : Component
    {
        return LoadAsset<T>(parent);
    }

    /// <summary>
    /// Retrieves a cached prefab of type <typeparamref name="T"/>.
    /// If the prefab is not already cached, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <returns>The cached instance of the requested prefab.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the requested prefab is not cached.</exception>
    public static T? GetPrefab<T>() where T : Component
    {
        if (!CachedTypes.Contains(typeof(T).Name))
            throw new InvalidOperationException("Unable to get a prefab that hasn't been cached!");

        if (Cached.TryGetValue(typeof(T).Name, out var obj))
        {
            return obj.GetComponent<T>();
        }

        return LoadAsset<T>(null, true);
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

        LoadAsset<T>(null, true);
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