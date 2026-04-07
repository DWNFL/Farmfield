    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Image image;

        //Для теста можно прописать [SerializeField] чтобы дать слоту предмет
        private Item item;
        public Item Item => item;
        
        [HideInInspector] 
        public Transform parentAfterDrag;

        private void Awake()
        {
            image = GetComponent<Image>();
        }
        // Для теста start нужен так как итемам пока неоткуда браться
        private void Start()
        {
            InitialiseItem(item);
        }

        public void InitialiseItem(Item newItemData)
        {
            if (newItemData == null || image == null)
                return;

            item = newItemData;
            image.sprite = newItemData.Icon;
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
