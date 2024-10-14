using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }
    public int maxPlayCount = 3;
    public TMP_InputField userNumberInputField;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI loginMessageText;
    public User currentUser;
    public Toggle rememberToggle;
    public bool loginSuccess = false;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void UpdateInputFieldForRememberedUser()
    {
        if (rememberToggle.isOn && currentUser != null)
        {
            userNumberInputField.text = currentUser.userNumber;
            nameInputField.text = currentUser.name;
        }
        else
        {
            userNumberInputField.text = string.Empty;
            nameInputField.text = string.Empty;
        }
    }
    // 학생 번호 유효성 검사
    public IEnumerator ValidateSignIn()
    {
        string studentNumber = userNumberInputField.text; // 입력된 학생 번호 가져오기
        string name = nameInputField.text;
        if (studentNumber.Length == 8 && int.TryParse(studentNumber, out _)) // 8자리 숫자인지 확인
        {
            if (name.Length == 0 || !IsKorean(name))
            {
                loginMessageText.color = Color.red;
                loginMessageText.text = "Invalid Name..";
            }
            else
            {

                loginMessageText.color = Color.yellow;
                loginMessageText.text = "Loading..";
                Debug.Log("학생 번호가 유효합니다.");
                yield return WebConnector.Instance.StartCoroutine(WebConnector.Instance.Login(studentNumber, name));
                if (loginSuccess)
                {
                    //로그인 성공
                    currentUser.playCount++;
                    StartCoroutine(ShowSignInSuccessMessage());
                }
                else
                {
                    loginMessageText.color = Color.red;
                    loginMessageText.text = "You have reached your play limit..";
                }
            }
        }
        else
        {
            Debug.Log("학생 번호는 8자리 숫자여야 합니다.");
            loginMessageText.color = Color.red;
            loginMessageText.text = "Invalid Student number..";
        }

    }


    // 로그인 성공 메시지 출력하는 코루틴
    IEnumerator ShowSignInSuccessMessage()
    {
        for (int i = 3; i > 0; i--)
        {
            loginMessageText.color = new Color(20f / 255f, 190f / 255f, 0f / 255f);
            loginMessageText.text = $"Sign In Success!.....{i}";
            yield return new WaitForSecondsRealtime(1); // 1초 대기
        }
        loginMessageText.text = "";
        // 3초 대기 후 씬 전환
        GameManager.Instance.GoToGameScene();
    }
    public bool IsKorean(string input)
    {
        foreach (char c in input)
        {
            // 한글 음절 범위 체크 (U+AC00 ~ U+D7A3)
            if (c < '\uAC00' || c > '\uD7A3')
            {
                return false; // 한글이 아닌 문자가 있으면 false 반환
            }
        }
        return true; // 모든 문자가 한글이면 true 반환
    }

}
