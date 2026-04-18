using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject menuPanel;        // Добавьте ссылку на панель меню
    public MonoBehaviour playerController; // Добавьте ссылку на скрипт игрока

    void Update()
    {
        // Открытие/закрытие меню по Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void PlayGame()
    {
        Debug.Log("Кнопка PlayGame нажата!");

        // Вариант 1: загрузка по имени (проверяем, что сцена существует)
        string sceneName = "GameScene";

        // Проверяем, есть ли такая сцена в Build Settings
        if (IsSceneInBuild(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Сцена '" + sceneName + "' не найдена в Build Settings!");
            Debug.Log("Доступные сцены:");
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                Debug.Log("  " + i + ": " + name);
            }
        }
    }

    public void ExitGame()
    {
        Debug.Log("Кнопка ExitGame нажата!");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // НОВЫЙ МЕТОД: открыть меню
    public void OpenMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            Time.timeScale = 0f;              // Останавливаем игру
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Отключаем управление игроком, если он есть
            if (playerController != null)
                playerController.enabled = false;
        }
    }

    // НОВЫЙ МЕТОД: закрыть меню
    public void CloseMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            Time.timeScale = 1f;              // Возобновляем игру
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Включаем управление игроком обратно
            if (playerController != null)
                playerController.enabled = true;
        }
    }

    // НОВЫЙ МЕТОД: переключить меню
    public void ToggleMenu()
    {
        if (menuPanel != null && menuPanel.activeSelf)
            CloseMenu();
        else
            OpenMenu();
    }

    // Вспомогательная функция: проверяет, есть ли сцена в Build Settings
    private bool IsSceneInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (name == sceneName)
                return true;
        }
        return false;
    }
}