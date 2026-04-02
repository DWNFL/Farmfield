using UnityEngine;

public class GridPlacement : MonoBehaviour
{
    [Header("Настройки сетки")]
    public float gridSize = 1f;
    public LayerMask groundLayer;

    [Header("Визуализация")]
    public Material validMaterial;
    public Material invalidMaterial;

    [Header("Ссылки")]
    public Transform placementObject;
    public Transform gridRoot;

    private bool isPlacing = false;
    private Material originalMaterial;
    private Renderer objectRenderer;

    void Start()
    {
        if (placementObject != null)
        {
            objectRenderer = placementObject.GetComponent<Renderer>();
            if (objectRenderer != null)
                originalMaterial = objectRenderer.material;
        }

        if (gridRoot == null)
            gridRoot = transform;

        // Автоматически начинаем размещение
        if (placementObject != null)
        {
            StartPlacement(placementObject.gameObject);
        }
    }

    void Update()
    {
        if (!isPlacing) return;

        // Получаем позицию на сетке
        Vector3 targetPosition = GetMousePositionOnGrid();
        placementObject.position = targetPosition;

        // ВСЕГДА показываем валидный цвет (убрали проверку препятствий)
        UpdateVisual(true);

        // ЛКМ для установки
        if (Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }

        // ПКМ для отмены
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    Vector3 GetMousePositionOnGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 hitPoint = hit.point;

            float x = Mathf.Round(hitPoint.x / gridSize) * gridSize;
            float z = Mathf.Round(hitPoint.z / gridSize) * gridSize;
            float y = hitPoint.y + 0.5f; // Поднимаем куб на поверхность

            return new Vector3(x, y, z);
        }

        return placementObject.position;
    }

    void UpdateVisual(bool canPlace)
    {
        if (objectRenderer == null) return;

        // Меняем цвет в зависимости от canPlace
        if (canPlace)
            objectRenderer.material = validMaterial;
        else
            objectRenderer.material = invalidMaterial;
    }

    void PlaceObject()
    {
        GameObject newObject = Instantiate(placementObject.gameObject, placementObject.position, placementObject.rotation);

        Renderer newRenderer = newObject.GetComponent<Renderer>();
        if (newRenderer != null && originalMaterial != null)
            newRenderer.material = originalMaterial;

        newObject.transform.SetParent(gridRoot);

        Debug.Log($"Объект установлен на позиции {placementObject.position}");
    }

    void CancelPlacement()
    {
        isPlacing = false;

        if (objectRenderer != null && originalMaterial != null)
            objectRenderer.material = originalMaterial;

        placementObject.gameObject.SetActive(false);
        Debug.Log("Размещение отменено");
    }

    public void StartPlacement(GameObject objectToPlace)
    {
        isPlacing = true;
        placementObject.gameObject.SetActive(true);
        if (placementObject.GetComponent<Renderer>() != null && validMaterial != null)
            placementObject.GetComponent<Renderer>().material = validMaterial;
        placementObject.position = Vector3.zero;
        Debug.Log("Начало размещения. ЛКМ - установить, ПКМ - отмена");
    }
}