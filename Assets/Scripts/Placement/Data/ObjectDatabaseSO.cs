using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectDatabase", menuName = "Placement/Object Database")]
public class ObjectDatabaseSO : ScriptableObject
{
    [SerializeField] private List<ObjectData> objectsData = new();

    private Dictionary<int, ObjectData> objectsById;

    public IReadOnlyList<ObjectData> ObjectsData => objectsData;

    private void OnEnable()
    {
        BuildIndex();
    }

    private void OnValidate()
    {
        BuildIndex();
    }

    private void BuildIndex()
    {
        objectsById = new Dictionary<int, ObjectData>();

        foreach (var data in objectsData)
        {
            if (data == null)
                continue;

            if (objectsById.ContainsKey(data.ID))
            {
                Debug.LogError($"Duplicate Object ID detected: {data.ID} ({data.Name})", this);
                continue;
            }

            objectsById.Add(data.ID, data);
        }
    }

    public bool TryGetObjectById(int id, out ObjectData objectData)
    {
        if (objectsById == null || objectsById.Count != objectsData.Count)
            BuildIndex();

        return objectsById.TryGetValue(id, out objectData);
    }
}

[Serializable]
public class ObjectData
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public Vector2Int Size { get; private set; } = Vector2Int.one;
    [field: SerializeField] public GameObject Prefab { get; private set; }
}