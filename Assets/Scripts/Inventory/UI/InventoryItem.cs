using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text amountText;

    private Image image;
    private Item item;
    private int count;
    private int ownerSlotIndex;
    private int dropTargetSlotIndex;

    public Item Item => item;
    public int Count => count;
    public int OwnerSlotIndex => ownerSlotIndex;

    [HideInInspector]
    public Transform parentAfterDrag;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Bind(Item newItemData, int newCount, int slotIndex)
    {
        item = newItemData;
        count = newCount;
        ownerSlotIndex = slotIndex;
        dropTargetSlotIndex = slotIndex;

        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (item == null || count <= 0)
        {
            image.sprite = null;
            if (amountText != null)
            {
                amountText.text = string.Empty;
            }

            return;
        }

        image.sprite = item.Icon;

        if (amountText != null)
        {
            amountText.text = count > 1 ? count.ToString() : string.Empty;
        }
    }

    public void SetDropTargetSlot(int slotIndex)
    {
        dropTargetSlotIndex = slotIndex;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null)
        {
            return;
        }

        parentAfterDrag = transform.parent;
        dropTargetSlotIndex = ownerSlotIndex;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item == null)
        {
            return;
        }

        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (item == null)
        {
            return;
        }

        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.HandleDrop(ownerSlotIndex, dropTargetSlotIndex);
        }
    }
}

