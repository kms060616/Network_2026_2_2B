using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private TMP_Text resultInfo;
    [SerializeField] private Button retryButton;

    private bool restarting;

    private void Awake()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void Show(
        bool cleared,
        string message,
        int reachedWave,
        float playTime)
    {
        int totalSeconds = Mathf.FloorToInt(playTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (resultTitle != null)
            resultTitle.text = cleared ? "클리어!" : "패배";

        if (resultInfo != null)
        {
            resultInfo.text =
                $"{message}\n" +
                $"도달 웨이브: {reachedWave}\n" +
                $"플레이 시간: {minutes:00}:{seconds:00}";
        }

        if (resultPanel != null)
            resultPanel.SetActive(true);
    }

    public void Retry()
    {
        if (restarting)
            return;

        Scene scene = SceneManager.GetActiveScene();

        if (scene.buildIndex < 0)
        {
            Debug.LogError(
                "현재 씬을 Build Profiles의 Scene List에 추가해주세요.",
                this
            );
            return;
        }

        restarting = true;

        if (retryButton != null)
            retryButton.interactable = false;

        Time.timeScale = 1f;
        SceneManager.LoadScene(scene.buildIndex);
    }
}
