using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private Item[] startingItems;

    private InventoryStack[] slots;
    private int selectedSlot = -1;

    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        slots = new InventoryStack[inventorySlots.Length];
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            slots[i] = new InventoryStack();
            inventorySlots[i].Initialise(i);
        }
    }

    private void Start()
    {
        if (startingItems != null)
        {
            foreach (Item item in startingItems)
            {
                if (item != null)
                {
                    AddItem(item, 1);
                }
            }
        }

        RefreshUI();

        if (inventorySlots.Length > 0)
        {
            ChangeSelectedSlot(0);
        }
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(Input.inputString))
        {
            return;
        }

        bool isNumber = int.TryParse(Input.inputString, out int number);
        if (!isNumber || number < 0 || number >= 10)
        {
            return;
        }

        int targetSlot = number == 0 ? 9 : number - 1;
        if (targetSlot < inventorySlots.Length)
        {
            ChangeSelectedSlot(targetSlot);
        }
    }

    private void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length)
        {
            inventorySlots[selectedSlot].Deselect();
        }

        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }

    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        int remaining = amount;
        int maxStackSize = item.Stackable ? Mathf.Max(1, item.MaxStack) : 1;

        if (item.Stackable)
        {
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                InventoryStack stack = slots[i];
                if (stack.IsEmpty || stack.Item != item)
                {
                    continue;
                }

                int freeSpace = Mathf.Max(0, maxStackSize - stack.Count);
                if (freeSpace <= 0)
                {
                    continue;
                }

                int toAdd = Mathf.Min(freeSpace, remaining);
                stack.Count += toAdd;
                remaining -= toAdd;
            }
        }

        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            InventoryStack stack = slots[i];
            if (!stack.IsEmpty)
            {
                continue;
            }

            int toAdd = Mathf.Min(maxStackSize, remaining);

            stack.Item = item;
            stack.Count = toAdd;
            remaining -= toAdd;
        }

        RefreshUI();
        return remaining == 0;
    }

    public void HandleDrop(int fromSlotIndex, int toSlotIndex)
    {
        if (fromSlotIndex < 0 || fromSlotIndex >= slots.Length)
        {
            return;
        }

        if (toSlotIndex < 0 || toSlotIndex >= slots.Length || fromSlotIndex == toSlotIndex)
        {
            return;
        }

        InventoryStack from = slots[fromSlotIndex];
        InventoryStack to = slots[toSlotIndex];

        if (from.IsEmpty)
        {
            return;
        }

        if (to.IsEmpty)
        {
            MoveStack(from, to);
        }
        else if (from.Item == to.Item && from.Item.Stackable)
        {
            MergeStacks(from, to);
        }
        else
        {
            SwapStacks(from, to);
        }

        RefreshUI();
    }

    public Item GetSelectedItem()
    {
        if (selectedSlot < 0 || selectedSlot >= slots.Length)
        {
            return null;
        }

        return slots[selectedSlot].Item;
    }

    public Item TakeSelectedItem(int amount = 1)
    {
        if (selectedSlot < 0 || selectedSlot >= slots.Length)
        {
            return null;
        }

        InventoryStack selectedStack = slots[selectedSlot];
        if (selectedStack.IsEmpty)
        {
            return null;
        }

        Item takenItem = selectedStack.Item;
        int removeCount = Mathf.Max(1, amount);
        selectedStack.Count -= removeCount;

        if (selectedStack.Count <= 0)
        {
            selectedStack.Clear();
        }

        RefreshUI();
        return takenItem;
    }

    private void MoveStack(InventoryStack from, InventoryStack to)
    {
        to.Item = from.Item;
        to.Count = from.Count;
        from.Clear();
    }

    private void MergeStacks(InventoryStack from, InventoryStack to)
    {
        int maxStack = Mathf.Max(1, from.Item.MaxStack);
        int freeSpace = Mathf.Max(0, maxStack - to.Count);
        if (freeSpace <= 0)
        {
            return;
        }

        int moved = Mathf.Min(freeSpace, from.Count);
        to.Count += moved;
        from.Count -= moved;

        if (from.Count <= 0)
        {
            from.Clear();
        }
    }

    private void SwapStacks(InventoryStack first, InventoryStack second)
    {
        Item tempItem = first.Item;
        int tempCount = first.Count;

        first.Item = second.Item;
        first.Count = second.Count;

        second.Item = tempItem;
        second.Count = tempCount;
    }

    private void RefreshUI()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            ClearSlotView(slot);

            InventoryStack stack = slots[i];
            if (stack.IsEmpty)
            {
                continue;
            }

            SpawnItemView(slot, stack, i);
        }
    }

    // ───────────────────────────────────────────
    // Публичные методы для Market-системы
    // ───────────────────────────────────────────

    public int SlotCount => slots.Length;

    /// <summary>
    /// Получить стак по индексу (read-only доступ).
    /// </summary>
    public InventoryStack GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return null;

        return slots[index];
    }

    /// <summary>
    /// Подсчитать общее количество конкретного предмета во всём инвентаре.
    /// </summary>
    public int CountItem(Item item)
    {
        if (item == null)
            return 0;

        int total = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty && slots[i].Item == item)
            {
                total += slots[i].Count;
            }
        }

        return total;
    }

    /// <summary>
    /// Убрать определённое количество конкретного предмета из инвентаря.
    /// Возвращает фактически удалённое количество.
    /// </summary>
    public int RemoveItem(Item item, int amount)
    {
        if (item == null || amount <= 0)
            return 0;

        int remaining = amount;

        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            InventoryStack stack = slots[i];
            if (stack.IsEmpty || stack.Item != item)
                continue;

            int toRemove = Mathf.Min(stack.Count, remaining);
            stack.Count -= toRemove;
            remaining -= toRemove;

            if (stack.Count <= 0)
            {
                stack.Clear();
            }
        }

        RefreshUI();
        return amount - remaining;
    }

    /// <summary>
    /// Получить список всех SellableItem в инвентаре (для UI рынка).
    /// </summary>
    public System.Collections.Generic.List<SellableItemInfo> GetSellableItems()
    {
        var result = new System.Collections.Generic.List<SellableItemInfo>();
        var counted = new System.Collections.Generic.HashSet<int>();

        for (int i = 0; i < slots.Length; i++)
        {
            InventoryStack stack = slots[i];
            if (stack.IsEmpty)
                continue;

            SellableItem sellable = stack.Item as SellableItem;
            if (sellable == null)
                continue;

            if (counted.Contains(sellable.ID))
                continue;

            counted.Add(sellable.ID);
            int totalCount = CountItem(sellable);

            result.Add(new SellableItemInfo
            {
                Item = sellable,
                TotalCount = totalCount
            });
        }

        return result;
    }

    // ───────────────────────────────────────────

    private void ClearSlotView(InventorySlot slot)
    {
        for (int child = slot.transform.childCount - 1; child >= 0; child--)
        {
            Destroy(slot.transform.GetChild(child).gameObject);
        }
    }

    private void SpawnItemView(InventorySlot slot, InventoryStack stack, int slotIndex)
    {
        GameObject view = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = view.GetComponent<InventoryItem>();
        if (inventoryItem == null)
        {
            inventoryItem = view.GetComponentInChildren<InventoryItem>();
        }

        if (inventoryItem != null)
        {
            inventoryItem.Bind(stack.Item, stack.Count, slotIndex);
        }
    }
}
