using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevelPhysics;
using UnityEngine.UI;

public enum UIState { START, GAME, GAMEOVER }
public class UIManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> uiPanels;
    [SerializeField] private Button StartButton;
    [SerializeField] private TMPro.TextMeshProUGUI inGameScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverTimeText;

    [SerializeField] private AudioSource gameOverAudioSource;


    private AudioSource bgAudioSource;
    private DateTime sessionStart_Time;

    public static Action<int> AddScoreAction;
    public static Action GameOverAction;
    public static Action RetryGameAction;


    private void Start()
    {
        bgAudioSource = GetComponent<AudioSource>();

        ShowPanel(UIState.START);

        StartButton.onClick.AddListener(OnStartButtonClicked);

        AddScoreAction += UpdateScore;

        GameOverAction += GameOver;
        bgAudioSource.volume = 0.05f;
    }

    private void UpdateScore(int value)
    {
        if (value > 0)
        {
            Debug.Log("Updating score by " + value);
            int currentValue = int.Parse(inGameScoreText.text);
            inGameScoreText.text = (currentValue + value).ToString();
        }
    }

    private void OnStartButtonClicked()
    {
        BallController.startPlayerAction?.Invoke();

        ShowPanel(UIState.GAME);
        sessionStart_Time = DateTime.Now;
        inGameScoreText.text = "0";
        bgAudioSource.volume = 0.1f;

    }

    private void ShowPanel(UIState uIState)
    {
        int index = (int)uIState;
        foreach (GameObject panel in uiPanels)
        {
            if (panel.activeSelf)
                panel.SetActive(false);
        }

        uiPanels[index].SetActive(true);
    }

    public void OnRetryGame()
    {
        ShowPanel(UIState.START);
        RetryGameAction?.Invoke();
    }

    private void GameOver()
    {
        ShowPanel(UIState.GAMEOVER);
        bgAudioSource.volume = 0.05f;
        gameOverAudioSource.Play();
        int score = int.Parse(inGameScoreText.text);
        float time = (float)(DateTime.Now - sessionStart_Time).TotalSeconds;

        gameOverTimeText.text = time.ToString("F1") + "s";
        gameOverScoreText.text = score.ToString();
    }


}
