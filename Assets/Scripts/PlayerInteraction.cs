using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Transform handPoint;
    private GameObject currentBucket = null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 5f))
            {
                Debug.Log("ÏÎÏÀË: " + hit.collider.name);

                // ÂÇßÒÜ ÂÅÄĞÎ
                if (hit.collider.CompareTag("Bucket") && currentBucket == null)
                {
                    currentBucket = hit.collider.gameObject;
                    currentBucket.transform.SetParent(handPoint);
                    currentBucket.transform.localPosition = Vector3.zero;
                    currentBucket.transform.localRotation = Quaternion.identity;

                    Rigidbody rb = currentBucket.GetComponent<Rigidbody>();
                    if (rb) rb.isKinematic = true;

                    Debug.Log("ÂÇßË ÂÅÄĞÎ");
                    return;
                }

                // ÍÀÁĞÀÒÜ ÂÎÄÓ
                if (hit.collider.CompareTag("Well") && currentBucket != null)
                {
                    Bucket b = currentBucket.GetComponent<Bucket>();
                    if (b != null && b.waterInBucket < b.maxWater)
                    {
                        b.waterInBucket = b.maxWater;
                        Debug.Log("ÍÀÁĞÀË ÂÎÄÓ");
                    }
                    return;
                }

                // ÏÎËÈÒÜ
                if (hit.collider.CompareTag("Flower") && currentBucket != null)
                {
                    Bucket b = currentBucket.GetComponent<Bucket>();
                    if (b != null && b.waterInBucket > 0)
                    {
                        b.waterInBucket--;
                        Debug.Log("ÏÎËÈË");

                        Flower f = hit.collider.GetComponent<Flower>();
                        if (f != null) f.Water();
                    }
                    return;
                }
            }
        }
    }
}