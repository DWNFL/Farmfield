using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
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