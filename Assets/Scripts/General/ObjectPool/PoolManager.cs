using System;
using System.Collections;
using System.Collections.Generic;
using GameBuilders.Collections;
using UnityEngine;

[System.Serializable]
public struct InitialPooledItem
{
    public string name;
    public GameObject prefab;
    public int size;
}

public class PoolManager : Singleton<PoolManager>
{
    public bool logStatus;
    public Transform root;

    public InitialPooledItem[] initialPooledItems;

    private Dictionary<GameObject, ObjectPool<GameObject>> _prefabLookup;
    private Dictionary<GameObject, ObjectPool<GameObject>> _instanceLookup;
    public Dictionary<string, GameObject> namedPrefabLookup;
    private bool _dirty = false;

    private void Awake()
    {
        _prefabLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
        _instanceLookup = new Dictionary<GameObject, ObjectPool<GameObject>>();
        namedPrefabLookup = new Dictionary<string, GameObject>();

        foreach (InitialPooledItem item in initialPooledItems)
        {
            warmPool(item.prefab, item.size);
            namedPrefabLookup[item.name] = item.prefab;
        }
    }

    private void Update()
    {
        if (logStatus && _dirty)
        {
            PrintStatus();
            _dirty = false;
        }
    }

    public void warmPool(GameObject prefab, int size)
    {
        if (_prefabLookup.ContainsKey(prefab))
        {
            throw new Exception("Pool for prefab " + prefab.name + " has already been created");
        }
        var pool = new ObjectPool<GameObject>(() => { return InstantiatePrefab(prefab); }, size);
        _prefabLookup[prefab] = pool;
        _dirty = true;
    }

    public GameObject spawnObject(GameObject prefab)
    {
        return spawnObject(prefab, Vector3.zero, Quaternion.identity);
    }

    public GameObject spawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!_prefabLookup.ContainsKey(prefab))
        {
            warmPool(prefab, 1);
        }

        ObjectPool<GameObject> pool = _prefabLookup[prefab];

        GameObject clone = pool.GetItem();
        clone.transform.SetPositionAndRotation(position, rotation);
        clone.SetActive(true);

        _instanceLookup.Add(clone, pool);
        _dirty = true;
        return clone;
    }

    IEnumerator ReleaseAfterTime(GameObject clone, float time)
    {
        yield return new WaitForSeconds(time);
        releaseObject(clone);
    }

    public void releaseObject(GameObject clone)
    {
        clone.SetActive(false);
        if (_instanceLookup.ContainsKey(clone))
        {
            _instanceLookup[clone].ReleaseItem(clone);
            _instanceLookup.Remove(clone);
            clone.transform.SetParent(root);
            _dirty = true;
        }
        else
        {
            Debug.LogWarning("No pool contains the object: " + clone.name, clone);
        }
    }

    public void releaseAllObjects()
    {
        foreach (GameObject go in new List<GameObject>(_instanceLookup.Keys))
        {
            releaseObject(go);
        }
    }

    private GameObject InstantiatePrefab(GameObject prefab)
    {
        var go = Instantiate(prefab) as GameObject;
        if (root != null) { go.transform.SetParent(root); }
        go.SetActive(false);
        return go;
    }

    public void PrintStatus()
    {
        foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> keyValuePair in _prefabLookup)
        {
            Debug.Log(string.Format(
                "Object Pool for Prefab: {0} In Use: {1} Total {2}",
                keyValuePair.Key.name, keyValuePair.Value.CountUsedItems, keyValuePair.Value.Count
            ));
        }
    }

    #region Static API

    public static void WarmPool(GameObject prefab, int size)
    {
        Instance.warmPool(prefab, size);
    }

    public static GameObject SpawnObject(GameObject prefab)
    {
        return Instance.spawnObject(prefab);
    }

    public static GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Instance.spawnObject(prefab, position, rotation);
    }

    public static GameObject SpawnByName(string name)
    {
        if (!Instance.namedPrefabLookup.ContainsKey(name))
        {
            Debug.LogError($"Pooled object {name} does not exist!");
            return null;
        }
        GameObject go = Instance.namedPrefabLookup[name];
        return Instance.spawnObject(go);
    }

    // avoid this unless you are short on time!
    public static GameObject SpawnByName(String name, Vector3 position, Quaternion rotation)
    {
        if (!Instance.namedPrefabLookup.ContainsKey(name))
        {
            Debug.LogError($"Pooled object {name} does not exist!");
            return null;
        }
        GameObject go = Instance.namedPrefabLookup[name];
        return Instance.spawnObject(go, position, rotation);
    }

    public static void ReleaseObject(GameObject clone)
    {
        Instance.releaseObject(clone);
    }

    public static void ReleaseObject(GameObject clone, float delaySeconds)
    {
        Instance.StartCoroutine(Instance.ReleaseAfterTime(clone, delaySeconds));
    }

    public static void ReleaseAllObjects()
    {
        Instance.releaseAllObjects();
    }

    #endregion
}
