using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour

{
    public GameObject pauseUI;
    

    public void OnGameResumePress()
    {
        pauseUI.SetActive(false);
    }
    public void OnGameExitPress()
    {
        Application.Quit();
    }

    public void OnEnterPausePress()
    {
        pauseUI.SetActive(true);

    }
}