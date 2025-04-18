using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;
    public GameObject windArrow;
    public GameObject fogGameObject;
    public GameObject fogUI;
    public TextMeshProUGUI windVelocityText;
    public bool isWindy;
    public bool isFoggy;
    public float windForce;
    public Vector2 windDirection;
    public ParticleSystem windParticleSystem;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        windArrow = GameObject.Find("Canvas/Wind/WindArrow");
        windVelocityText = GameObject.Find("Canvas/Wind/WindVelocity").GetComponent<TextMeshProUGUI>();
        isWindy = false;
        windParticleSystem = GameObject.Find("WindParticle").GetComponent<ParticleSystem>();
        windParticleSystem.Pause();

        fogGameObject = GameObject.Find("Fog");
        fogUI = GameObject.Find("Canvas/FogUI");
        isFoggy = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isWindy)
        {
            windArrow.transform.parent.gameObject.SetActive(true);
            ApplyWindEffect();
        }
        else
        {
            windArrow.transform.parent.gameObject.SetActive(false);
        }
        if (ItemManager.Instance.currentItemState != ItemState.Goggle && ScoreManager.Instance.currentScore > 500)
        {
            isFoggy = true;
        }
        if (isFoggy)
        {
            if (ItemManager.Instance.currentItemState == ItemState.Goggle)
            {
                fogGameObject.SetActive(false);
            }
            else
            {
                fogGameObject.SetActive(true);
            }
            fogUI.SetActive(true);
        }
        else
        {
            fogGameObject.SetActive(false);
            fogUI.SetActive(false);
        }
    }

    // 바람의 영향을 적용하는 함수
    void ApplyWindEffect()
    {
        // 바람의 세기에 따라 X축으로 힘을 가함
        Vector2 windEffect = windDirection * windForce * Time.deltaTime;
        BallController.Instance.rb.AddForce(windEffect, ForceMode2D.Force);
        var main = windParticleSystem.main;
        main.startSpeed = windDirection.x * windForce / 10;
        windParticleSystem.Play();
    }

    public void WindInit()
    {
        if (ScoreManager.Instance.currentScore >= 100)
        {
            isWindy = true;
        }
        windForce = Random.Range(1, 6) * 10; // 10 ~ 100 사이의 랜덤 값으로 바람의 세기를 설정
        windVelocityText.text = ((int)(windForce / 10)).ToString();
        // 50% 확률로 왼쪽 또는 오른쪽 방향 설정
        if (Random.Range(0, 2) == 0) // 0 또는 1이 랜덤으로 선택됨
        {
            windDirection = Vector2.left; // 왼쪽 방향
            windArrow.transform.eulerAngles = new Vector3(0, 0, 90);
        }
        else
        {
            windDirection = Vector2.right; // 오른쪽 방향
            windArrow.transform.eulerAngles = new Vector3(0, 0, -90);
        }
    }
}
