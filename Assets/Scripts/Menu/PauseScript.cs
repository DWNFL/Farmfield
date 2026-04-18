using UnityEngine;

public class PauseScript : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject placementScript;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuPanel.activeSelf)
            {
                menuPanel.SetActive(false);
                Time.timeScale = 1f;
                placementScript.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                menuPanel.SetActive(true);
                Time.timeScale = 0f;
                placementScript.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}