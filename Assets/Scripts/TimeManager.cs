using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    public bool timeOn = false;
    public Image timerDial; // 다이얼 UI 이미지
    public Text timerText;
    public float totalTime = 120f; // 총 타이머 시간 (2분)
    public AudioClip endBuzzerSound;
    private AudioSource audioSource;
    private float timeRemaining;
    private bool hasPlayedEndSound = false; // 게임 종료
    public bool hasCalledGameOver = false;
    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnEnable()
    {
        Time.timeScale = 2.0f;
        timeOn = false;
        timeRemaining = totalTime; // 초기 시간 설정
        audioSource = GetComponent<AudioSource>();
        UpdateTimerUI();
    }

    void Update()
    {
        if (timeOn)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime / Time.timeScale; // 남은 시간 갱신
                UpdateTimerText();
                UpdateTimerUI();
            }
            else
            {
                if(!hasPlayedEndSound)
                {
                    audioSource.clip = endBuzzerSound;
                    audioSource.Play();
                    hasPlayedEndSound = true;
                }
                if (ThrowingBall.Instance.rb.velocity.sqrMagnitude == 0 && !hasCalledGameOver)
                {
                    GameManager.Instance.GameOver();
                    hasCalledGameOver = true;
                }
            }
        }
    }

    void UpdateTimerUI()
    {
        if (timerDial != null)
        {
            // Fill Amount 값을 0에서 1 사이로 설정하여 다이얼 UI 업데이트
            timerDial.fillAmount = timeRemaining / totalTime;

            // 시간에 따른 색상 변경 (초록 -> 노랑 -> 빨강)
            float t = timeRemaining / totalTime;
            if (t > 0.5f)
            {
                // 초록색에서 노란색으로 서서히 변경
                timerDial.color = Color.Lerp(Color.yellow, Color.green, (t - 0.5f) * 2);
            }
            else
            {
                // 노란색에서 빨간색으로 서서히 변경
                timerDial.color = Color.Lerp(Color.red, Color.yellow, t * 2);
            }
        }
    }

    void UpdateTimerText()
    {
        if (timeRemaining >= 60)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }
        else
        {
            timerText.text = string.Format("{0:F1}", timeRemaining);
        }
    }
}