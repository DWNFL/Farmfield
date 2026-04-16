using System;

[Serializable]
public class InventoryStack
{
    public Item Item;
    public int Count;

    public bool IsEmpty => Item == null || Count <= 0;

    public void Clear()
    {
        Item = null;
        Count = 0;
    }
}

