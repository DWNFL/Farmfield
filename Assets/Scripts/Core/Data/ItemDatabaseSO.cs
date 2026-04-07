using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Databases/Item Database")]

public class ItemDatabaseSO : ScriptableObject
{
    [SerializeField] private List<Item> items;
    private Dictionary<int, Item> itemsById;

    public IReadOnlyList<Item> Items => items;

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
        itemsById = new Dictionary<int, Item>();

        foreach (var item in items)
        {
            if (item == null)
                continue;

            if (itemsById.ContainsKey(item.ID))
            {
                Debug.LogError($"Duplicate Item ID detected: {item.ID} ({item.ItemName})", this);
                continue;
            }

            itemsById.Add(item.ID, item);
        }
    }

    public bool TryGetItemById(int id, out Item item)
    {
        if (itemsById == null || itemsById.Count != items.Count)
            BuildIndex();

            return itemsById.TryGetValue(id, out item);
    }
}