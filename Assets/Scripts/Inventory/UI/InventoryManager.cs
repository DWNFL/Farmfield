using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private GameObject inventoryItemPrefab;
    int selectedSlot = -1;

    private void Start(){
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

    public Item GetSelectedItem()
    {
        if (selectedSlot < 0  || selectedSlot >= inventorySlots.Length)
            return null;

            var item = inventorySlots[selectedSlot].GetComponentInChildren<InventoryItem>();   

            return item != null ? item.Item : null;
    }
}
