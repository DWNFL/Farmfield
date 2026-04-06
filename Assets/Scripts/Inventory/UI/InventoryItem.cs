    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        [Header("UI")]
        public Image image;
        public ItemData item;
        [HideInInspector] 
        public Transform parentAfterDrag;

        private void Start()
        {
            InitialiseItem(item);
        }

        public void InitialiseItem(ItemData newItemData){
            image.sprite = newItemData.icon;
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("Begin drag");
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            image.raycastTarget = false;

        }
        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("Dragging");
            transform.position = Input.mousePosition;
        }
    
        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("End drag");
            transform.SetParent(parentAfterDrag);
            image.raycastTarget = true;
        }
    }
