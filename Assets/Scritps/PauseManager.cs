using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public static bool IsPaused =>
        Instance != null && Instance.paused;

    [SerializeField] private WaveManager waveManager;
    [SerializeField] private BoardInput boardInput;
    [SerializeField] private GameObject pausePanel;

    private bool paused;
    private bool boardInputWasEnabled;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("PauseManager는 하나만 두세요.", this);
            Destroy(this);
            return;
        }

        Instance = this;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void Pause()
    {
        if (paused ||
            waveManager == null ||
            waveManager.IsGameEnded ||
            Time.timeScale <= 0f)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        paused = true;

        if (boardInput != null)
        {
            boardInputWasEnabled = boardInput.enabled;

            // 기존 OnDisable에서 선택 표시를 정리합니다.
            boardInput.enabled = false;
        }

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (!paused)
            return;

        bool gameEnded =
            waveManager == null || waveManager.IsGameEnded;

        paused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        // 종료된 게임을 다시 움직이게 하지 않습니다.
        if (gameEnded)
            return;

        Time.timeScale = previousTimeScale;

        if (boardInput != null)
            boardInput.enabled = boardInputWasEnabled;
    }

    private void LateUpdate()
    {
        if (paused &&
            (waveManager == null || waveManager.IsGameEnded))
        {
            Resume();
        }
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        if (paused)
            Time.timeScale = 1f;

        Instance = null;
    }
}
