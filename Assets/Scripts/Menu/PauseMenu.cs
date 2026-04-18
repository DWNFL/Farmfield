using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour

{
    public GameObject pauseUI;
    private bool isPaused = false;

    public KeyCode pauseKey = KeyCode.Escape;

    void Update()
    {
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
        Time.timeScale = 1f;  // ЭТО ГЛАВНОЕ
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnGameExitPress()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public void OnEnterPausePress()
    {
        pauseUI.SetActive(true);
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}