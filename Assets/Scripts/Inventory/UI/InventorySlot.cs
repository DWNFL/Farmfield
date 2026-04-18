using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Color selectedColor;
    public Color notSelectedColor;

    public int SlotIndex { get; private set; }

    public void Initialise(int slotIndex)
    {
        SlotIndex = slotIndex;
    }

    public void Awake()
    {
        Deselect();
    }

    public void Select()
    {
        image.color = selectedColor;
    }

    public void Deselect()
    {
        image.color = notSelectedColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null)
        {
            return;
        }

        InventoryItem draggableItem = dropped.GetComponent<InventoryItem>();
        if (draggableItem == null)
        {
            return;
        }

        draggableItem.parentAfterDrag = transform;
        draggableItem.SetDropTargetSlot(SlotIndex);
    }
}

