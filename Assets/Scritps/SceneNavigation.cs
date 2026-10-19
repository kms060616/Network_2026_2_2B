using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    [SerializeField] private string titleScene = "Title";
    [SerializeField] private string lobbyScene = "Lobby";
    [SerializeField] private string gameScene = "Game";

    private bool loading;

    public void GoToTitle()
    {
        LoadScene(titleScene);
    }

    public void GoToLobby()
    {
        LoadScene(lobbyScene);
    }

    public void StartGame()
    {
        LoadScene(gameScene);
    }

    private void LoadScene(string sceneName)
    {
        if (loading)
            return;

        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"씬을 불러올 수 없습니다: {sceneName}. " +
                "Build Profiles의 Scene List를 확인해주세요.",
                this
            );
            return;
        }

        loading = true;

        // 일시정지·게임 종료 상태에서도 씬을 이동합니다.
        Time.timeScale = 1f;

        SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Single
        );
    }
}
