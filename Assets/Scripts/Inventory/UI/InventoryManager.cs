using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;

    int selectedSlot = -1;

    private void Start(){
        ChangeSelectedSlot(0);
    }

    private void Update(){
        if (Input.inputString != null) {
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

    public bool AddItem(ItemData item){ 
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

    void SpawnNewItem(ItemData item, InventorySlot slot){
        GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItem.GetComponentInChildren<InventoryItem>(); 
        inventoryItem.InitialiseItem(item);
    }
}
