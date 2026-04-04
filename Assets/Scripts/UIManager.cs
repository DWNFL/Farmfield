using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour

{
    public GameObject pauseUI;
    private bool isPaused = false;

    // Для выбора кнопки в инспекторе Unity
    public KeyCode pauseKey = KeyCode.Escape;

    void Update()
    {
        // Проверяем нажатие кнопки
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                OnGameResumePress();
            else
                OnEnterPausePress();
        }
    }

    public void OnGameResumePress()
    {
        pauseUI.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f; // Возвращаем нормальный ход времени
        Cursor.lockState = CursorLockMode.Locked; // Прячем курсор (для 3D игр)
        Cursor.visible = false;
    }

    public void OnGameExitPress()
    {
        Application.Quit();
    }

    public void OnEnterPausePress()
    {
        pauseUI.SetActive(true);
        isPaused = true;
        Time.timeScale = 0f; // Останавливаем время в игре
        Cursor.lockState = CursorLockMode.None; // Показываем курсор
        Cursor.visible = true;
    }
}