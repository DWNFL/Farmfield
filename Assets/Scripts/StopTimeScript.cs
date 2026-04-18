using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public GameObject menuPanel;        // Ваша панель меню
    public MonoBehaviour playerController; // Скрипт управления игроком

    void Update()
    {
        // Открытие/закрытие по Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void OpenMenu()
    {
        menuPanel.SetActive(true);
        Time.timeScale = 0f;              // Останавливает игру
        playerController.enabled = false; // Отключает управление
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMenu()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;              // Возобновляет игру
        playerController.enabled = true;  // Включает управление
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ToggleMenu()
    {
        if (menuPanel.activeSelf)
            CloseMenu();
        else
            OpenMenu();
    }
}