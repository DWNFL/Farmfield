using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private Item[] startingItems; // <--- ДОБАВЛЕНО ДЛЯ ТЕСТА
    int selectedSlot = -1;

    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start(){
        // Выдаем стартовые предметы (например, семена картошки)
        if (startingItems != null)
        {
            foreach (var item in startingItems)
            {
                if (item != null) AddItem(item);
            }
        }

        ChangeSelectedSlot(0);
    }

    private void Update(){
        if (!string.IsNullOrEmpty(Input.inputString)){
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber && number >= 0 && number < 10){
                if (number == 0)
                    ChangeSelectedSlot(9);
                else
                    ChangeSelectedSlot(number - 1);
            }
        }        
    }
    void ChangeSelectedSlot(int newValue){
        if (selectedSlot >= 0)
            inventorySlots[selectedSlot].Deselect();

        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }

    public bool AddItem(Item item){ 
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }

        return false;  
    }

    void SpawnNewItem(Item item, InventorySlot slot){
        GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItem.GetComponentInChildren<InventoryItem>(); 
        inventoryItem.InitialiseItem(item);
    }

    /// <summary>
    /// Проверяет, есть ли в инвентаре нужное количество предметов.
    /// </summary>
    public bool HasItems(Item item, int count)
    {
        if (item == null || count <= 0) return false;

        int found = 0;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventoryItem itemInSlot = inventorySlots[i].GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.Item == item)
            {
                found++;
                if (found >= count) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Списывает нужное количество предметов из инвентаря.
    /// Возвращает true если успешно списано.
    /// </summary>
    public bool RemoveItems(Item item, int count)
    {
        if (!HasItems(item, count)) return false;

        int remaining = count;
        for (int i = 0; i < inventorySlots.Length && remaining > 0; i++)
        {
            InventoryItem itemInSlot = inventorySlots[i].GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.Item == item)
            {
                Destroy(itemInSlot.gameObject);
                remaining--;
            }
        }

        return true;
    }

    public Item GetSelectedItem()
    {
        if (selectedSlot < 0  || selectedSlot >= inventorySlots.Length)
            return null;

            var item = inventorySlots[selectedSlot].GetComponentInChildren<InventoryItem>();   

            return item != null ? item.Item : null;
    }
}
