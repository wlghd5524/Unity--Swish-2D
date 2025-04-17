using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public GameObject replayPopupPanel;
    public GameObject homePopupPanel;
    public GameObject itemUI;
    public GameObject ingameBackground;
    public GameObject[] resetZones;

    public List<GameObject> goalEffect = new List<GameObject>();
    //public Slider itemSlider;

    private void Awake()
    {
        Instance = this;
    }
    void OnEnable()
    {
        itemUI = GameObject.Find("Canvas/Item");
        itemUI.SetActive(false);
        //itemSlider = itemUI.GetComponentInChildren<Slider>();
        
        // 게임 씬에서만 백그라운드 조정
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            ingameBackground = GameObject.Find("Background");
            
            // 백그라운드 크기를 카메라 크기에 맞게 조정
            if (ingameBackground != null)
            {
                AdjustBackgroundToCamera();
            }
            resetZones = GameObject.FindGameObjectsWithTag("ResetZone");
            AdjustResetZonesToCamera();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (itemSlider.IsActive() && itemSlider.value == 0)
        //{
        //    itemUI.SetActive(false);
        //    ItemManager.Instance.currentItemState = ItemState.Normal;
        //}
    }
    public void PressRePlayButton()
    {
        replayPopupPanel.gameObject.SetActive(true);
    }
    public void PressHomeButton()
    {
        homePopupPanel.gameObject.SetActive(true);
    }
    public void PressCancelButton()
    {
        replayPopupPanel.gameObject.SetActive(false);
        homePopupPanel.gameObject.SetActive(false);
    }
    public void PressOKButton()
    {
        if (replayPopupPanel.gameObject.activeInHierarchy)
        {
            SceneManager.LoadScene("GameScene");
        }
        else if (homePopupPanel.gameObject.activeInHierarchy)
        {
            SceneManager.LoadScene("MainScene");
        }
    }
    
    // 백그라운드 크기를 카메라 크기에 맞게 조정하는 메서드
    public void AdjustBackgroundToCamera()
    {
        // 메인 카메라 가져오기
        Camera mainCamera = Camera.main;
        if (mainCamera == null || ingameBackground == null)
            return;
        
        // 카메라의 orthographic 크기 계산
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        // 백그라운드의 스프라이트 렌더러 가져오기
        SpriteRenderer backgroundRenderer = ingameBackground.GetComponent<SpriteRenderer>();
        if (backgroundRenderer == null)
            return;
        
        // 스프라이트의 실제 크기 가져오기
        Sprite backgroundSprite = backgroundRenderer.sprite;
        if (backgroundSprite == null)
            return;
        
        float spriteWidth = backgroundSprite.bounds.size.x;
        float spriteHeight = backgroundSprite.bounds.size.y;
        
        // 스케일 계산
        Vector3 backgroundScale = ingameBackground.transform.localScale;
        backgroundScale.x = cameraWidth / spriteWidth;
        backgroundScale.y = cameraHeight / spriteHeight;
        
        // 새로운 스케일 적용
        ingameBackground.transform.localScale = backgroundScale;
        
        // 위치를 카메라 중앙으로 설정
        ingameBackground.transform.position = new Vector3(
            mainCamera.transform.position.x,
            mainCamera.transform.position.y,
            ingameBackground.transform.position.z
        );
        
        Debug.Log($"백그라운드 크기 조정: {cameraWidth}x{cameraHeight}");
    }
    
    // ResetZone의 위치를 카메라 크기에 맞게 조정하는 메서드
    public void AdjustResetZonesToCamera()
    {
        // 메인 카메라 가져오기
        Camera mainCamera = Camera.main;
        if (mainCamera == null || resetZones == null || resetZones.Length == 0)
            return;
            
        // 카메라의 orthographic 크기 계산
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        // 카메라 위치
        Vector3 cameraPos = mainCamera.transform.position;
        
        // 각 ResetZone 찾아서 위치 조정
        GameObject resetZonesParent = resetZones[0].transform.parent.gameObject;
        
        foreach (Transform child in resetZonesParent.transform)
        {
            switch (child.name)
            {
                case "ResetBottomZone":
                    // 화면 하단에 위치
                    child.position = new Vector3(cameraPos.x, cameraPos.y - (cameraHeight / 2) - 1f, child.position.z);
                    // 너비 조정
                    AdjustColliderWidth(child, cameraWidth * 1.5f);
                    break;
                    
                case "ResetTopZone":
                    // 화면 상단에 위치 (카메라 높이의 1.5배 만큼 위쪽에 배치)
                    child.position = new Vector3(cameraPos.x, cameraPos.y + (cameraHeight*0.75f), child.position.z);
                    // 너비 조정
                    AdjustColliderWidth(child, cameraWidth * 1.5f);
                    break;
                    
                case "ResetRightZone":
                    // 화면 오른쪽에 위치
                    child.position = new Vector3(cameraPos.x + (cameraWidth / 2) + 1f, cameraPos.y, child.position.z);
                    // 높이 조정
                    AdjustColliderHeight(child, cameraHeight * 1.5f);
                    break;
                    
                case "ResetLeftZone":
                    // 화면 왼쪽에 위치
                    child.position = new Vector3(cameraPos.x - (cameraWidth / 2) - 1f, cameraPos.y, child.position.z);
                    // 높이 조정
                    AdjustColliderHeight(child, cameraHeight * 1.5f);
                    break;
            }
        }
        
        Debug.Log("ResetZones 위치 조정 완료");
    }
    
    // BoxCollider2D의 너비 조정
    private void AdjustColliderWidth(Transform obj, float width)
    {
        BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Vector2 size = collider.size;
            size.x = width;
            collider.size = size;
        }
    }
    
    // BoxCollider2D의 높이 조정
    private void AdjustColliderHeight(Transform obj, float height)
    {
        BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Vector2 size = collider.size;
            size.y = height;
            collider.size = size;
        }
    }
}
