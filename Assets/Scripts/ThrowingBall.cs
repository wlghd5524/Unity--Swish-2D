using UnityEngine;

public class ThrowingBall : MonoBehaviour
{
    private Vector3 initialPosition;    // 공의 초기 위치
    private Rigidbody2D rb;             // 공의 Rigidbody2D 컴포넌트

    // 던지기 힘 계수
    public float baseThrowForce = 10f;     // 기본 힘 크기
    public float maxThrowForce = 1000f;    // 최대 힘 제한
    public float forceMultiplier = 10f;   // 드래그 길이에 따른 힘 증가 비율

    private bool isDragging = false;    // 드래그 중인지 확인
    private Vector3 startMousePosition; // 드래그 시작 위치
    private Vector3 endMousePosition;   // 드래그 끝 위치

    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position; // 공의 초기 위치 저장
        rb.gravityScale = 1;            // 중력 활성화
        rb.isKinematic = true;          // 초기에는 공이 움직이지 않도록 설정
    }

    void Update()
    {
        // 마우스 클릭 시작 시 (왼쪽 버튼 클릭)
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                startMousePosition = mousePosition;
                rb.isKinematic = true; // 드래그 중에는 공이 움직이지 않도록 설정
            }
        }

        // 드래그 중일 때
        if (isDragging)
        {
            // 공이 드래그 동안 움직이지 않도록 고정
        }

        // 마우스 클릭 종료 시 (왼쪽 버튼 놓기)
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            rb.isKinematic = false; // 물리 효과 활성화
            endMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 드래그한 거리와 방향 계산
            Vector3 dragVector = startMousePosition - endMousePosition;
            float dragDistance = dragVector.magnitude; // 드래그 길이 계산

            // 드래그 거리의 최대값을 설정하여 너무 멀리 드래그해도 일정 값 이상 힘이 증가하지 않도록 설정
            float appliedForce = Mathf.Clamp(dragDistance * forceMultiplier, baseThrowForce, maxThrowForce);

            // 던지는 방향과 힘 계산 (드래그 방향의 반대 방향으로 던짐)
            Vector2 throwDirection = new Vector2(dragVector.x, dragVector.y).normalized;
            Vector2 force = throwDirection * appliedForce;

            // 공에 힘을 가하여 던지기
            rb.AddForce(force, ForceMode2D.Impulse);
            
            animator.SetBool("IsThrowing", true);
        }
    }

    // 충돌 감지 함수
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트의 태그가 "resetZone"일 경우
        if (collision.CompareTag("ResetZone"))
        {
            ResetBall(); // 공 리셋 함수 호출
        }
    }

    // 공 리셋 함수
    private void ResetBall()
    {
        animator.SetBool("IsThrowing",false);
        // 공의 위치와 상태를 초기화
        transform.localScale = new Vector2(1,1);
        transform.position = initialPosition; // 초기 위치로 이동
        rb.velocity = Vector2.zero;           // 속도 초기화
        rb.angularVelocity = 0f;              // 회전 속도 초기화
        rb.isKinematic = true;                // 다시 움직이지 않도록 설정
    }
}