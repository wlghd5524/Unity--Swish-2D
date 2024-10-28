using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } // 싱글톤 인스턴스
    public TextMeshProUGUI scoreText;
    public int currentScore;

    void Awake()
    {
        Instance = this;
        // 싱글톤 패턴 구현
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        scoreText = GameObject.Find("Canvas/Score").GetComponent<TextMeshProUGUI>();
        currentScore = 0;
        UpdateScoreText();
    }

    // 점수 업데이트 함수
    public void AddScore(int value)
    {
        if (ItemManager.Instance.currentItemState == ItemState.Goldenball)
        {
            value *= 2;
        }
        currentScore += value;
        UpdateScoreText();
    }



    // 점수 UI 업데이트 함수
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }
}