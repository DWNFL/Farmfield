using UnityEngine;

public class ClickHandler : MonoBehaviour
{
    public Bucket bucket;
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

                Well well = clickedObject.GetComponent<Well>();
                if (well != null)
                {
                    bucket.TakeWaterFromWell(well);
                    return;
                }

                Flower flower = clickedObject.GetComponent<Flower>();
                if (flower != null)
                {
                    bucket.WaterFlower(flower);
                    return;
                }
            }
        }
    }
}