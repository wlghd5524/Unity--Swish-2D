using System.Net.Sockets;
using UnityEngine;

public class ThrowingBall : MonoBehaviour
{
    private Vector3 initialPosition;    // 공의 초기 위치
    private Rigidbody2D rb;             // 공의 Rigidbody2D 컴포넌트
    public LineRenderer[] trajectoryLines;  // 공의 궤도 예측선

    // 던지기 힘 계수
    public float baseThrowForce;     // 기본 힘 크기
    public float maxThrowForce;    // 최대 힘 제한
    public float forceMultiplier;   // 드래그 길이에 따른 힘 증가 비율
    public float rotationMultiplier; // 회전력 계수

    private bool isDragging = false;    // 드래그 중인지 확인
    private Vector3 startMousePosition; // 드래그 시작 위치
    private Vector3 endMousePosition;   // 드래그 끝 위치

    public GameObject rim;
    public Collider2D rimLeftCollider;       // 림의 왼쪽 Collider2D를 참조
    public Collider2D rimRightCollider;       // 림의 오른쪽 Collider2D를 참조
    public Transform rimTopPoint;        // 림의 상단 지점을 표시하는 Transform
    public Transform ballBottomPoint;
    public SpriteRenderer rimSpriteRenderer; // 림의 SpriteRenderer
    public SpriteRenderer ballSpriteRenderer; // 공의 SpriteRenderer
    public SpriteRenderer netSpriteRenderer; // 그물의 SpriteRenderer

    public float minScale;        // 공의 최소 크기
    public float maxScale;          // 공의 최대 크기
    public float baseScaleDuration; // 기본 크기 변화 시간 (초)
    private float scaleDuration;    // 공이 점점 작아지는데 걸리는 시간 (초)

    public float ballForce;

    private bool hasPassedRim = false;   // 림을 완전히 넘어갔는지 체크

    private bool isScaling = false;     // 공의 크기 변화가 진행 중인지 확인
    private float currentScaleTime = 0f; // 크기 변화에 사용될 타이머

    void Start()
    {
        rim = GameObject.Find("Hoop/Rim");
        rimLeftCollider = rim.transform.Find("RimLeftCollider").GetComponent<Collider2D>();
        rimRightCollider = rim.transform.Find("RimRightCollider").GetComponent<Collider2D>();
        rimTopPoint = rim.transform.Find("RimTopPoint");
        ballBottomPoint = transform.Find("BallBottomPoint");
        rimSpriteRenderer = rim.GetComponent<SpriteRenderer>();
        ballSpriteRenderer = GetComponent<SpriteRenderer>();
        netSpriteRenderer = GameObject.Find("Hoop/Net").GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position; // 공의 초기 위치 저장
        rb.gravityScale = 1;            // 중력 활성화
        rb.isKinematic = true;          // 초기에는 공이 움직이지 않도록 설정

        // LineRenderer 설정
        trajectoryLines = GetComponentsInChildren<LineRenderer>();
        trajectoryLines[0].positionCount = 0;
        trajectoryLines[0].startWidth = 0.1f;
        trajectoryLines[0].endWidth = 0.1f;
        // 유도선 색상 설정 (첫 번째 궤도선: 노란색에서 주황색)
        trajectoryLines[0].startColor = Color.yellow;
        trajectoryLines[0].endColor = new Color(1f, 0.5f, 0f); // 주황색

        trajectoryLines[1].positionCount = 0;
        trajectoryLines[1].startWidth = 0.1f;
        trajectoryLines[1].endWidth = 0.1f;
        // 유도선 색상 설정 (두 번째 궤도선: 주황색에서 빨간색)
        trajectoryLines[1].startColor = new Color(1f, 0.5f, 0f); // 주황색
        trajectoryLines[1].endColor = Color.red;


        // 시작할 때는 림의 충돌 비활성화
        rimLeftCollider.enabled = false;
        rimRightCollider.enabled = false;

        // 유도선의 색상을 설정 (노란색에서 빨간색으로 그라데이션)
        //trajectoryLine.startColor = Color.yellow;
        //trajectoryLine.endColor = Color.red;

        // 초기에는 공이 림보다 위에 그려지도록 설정
        ballSpriteRenderer.sortingOrder = 2;
        rimSpriteRenderer.sortingOrder = 1;
        netSpriteRenderer.sortingOrder = 1;
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
                hasPassedRim = false; // 공이 림 위로 완전히 지나가지 않았음을 초기화
            }
        }

        // 드래그 중일 때
        if (isDragging)
        {
            endMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 드래그한 거리와 방향 계산
            Vector3 dragVector = startMousePosition - endMousePosition;
            float dragDistance = dragVector.magnitude; // 드래그 길이 계산

            // 드래그 거리의 최대값을 설정하여 너무 멀리 드래그해도 일정 값 이상 힘이 증가하지 않도록 설정
            ballForce = Mathf.Clamp(dragDistance * forceMultiplier, baseThrowForce, maxThrowForce);

            // 던지는 방향과 힘 계산 (드래그 방향의 반대 방향으로 던짐)
            Vector2 throwDirection = new Vector2(dragVector.x, dragVector.y).normalized;
            Vector2 force = throwDirection * ballForce;

            // 유도선 업데이트
            UpdateTrajectory(force);
        }

        // 마우스 클릭 종료 시 (왼쪽 버튼 놓기)
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            rb.isKinematic = false; // 물리 효과 활성화
            trajectoryLines[0].positionCount = 0; // 유도선 초기화
            trajectoryLines[1].positionCount = 0; // 유도선 초기화
            endMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 드래그한 거리와 방향 계산
            Vector3 dragVector = startMousePosition - endMousePosition;
            float dragDistance = dragVector.magnitude; // 드래그 길이 계산

            // 드래그 거리의 최대값을 설정하여 너무 멀리 드래그해도 일정 값 이상 힘이 증가하지 않도록 설정
            ballForce = Mathf.Clamp(dragDistance * forceMultiplier, baseThrowForce, maxThrowForce);

            // 던지는 방향과 힘 계산 (드래그 방향의 반대 방향으로 던짐)
            Vector2 throwDirection = new Vector2(dragVector.x, dragVector.y).normalized;
            Vector2 force = throwDirection * ballForce;

            // 공에 힘을 가하여 던지기
            rb.AddForce(force, ForceMode2D.Impulse);

            // 던지는 힘에 비례하여 회전력 추가
            float torque = ballForce * rotationMultiplier * (throwDirection.x > 0 ? 1 : -1);
            rb.AddTorque(torque);

            // 공의 크기 변화 시간 설정: 던지는 힘에 비례하여 설정
            scaleDuration = baseScaleDuration / (ballForce / baseThrowForce);

            isScaling = true;

        }

        // 공이 날아가는 동안 크기 변화
        if (isScaling)
        {
            ScaleBallOverTime();
        }

        // 공이 림을 완전히 넘어가기 전에는 공이 림보다 위에 그려지도록 설정
        if (!hasPassedRim)
        {
            ballSpriteRenderer.sortingOrder = 2;
            rimSpriteRenderer.sortingOrder = 1;
            netSpriteRenderer.sortingOrder = 1;
        }
        else
        {
            // 공이 림을 넘어간 후에는 림이 공보다 위에 그려지도록 설정
            ballSpriteRenderer.sortingOrder = 1;
            rimSpriteRenderer.sortingOrder = 2;
            netSpriteRenderer.sortingOrder = 2;
        }

        // 공이 림 위를 완전히 넘어갔는지 확인
        if (ballBottomPoint.position.y > rimTopPoint.position.y && ballForce > 10.3f)
        {
            hasPassedRim = true; // 림 위를 완전히 넘어감
        }

        // 공이 림 위를 완전히 넘어간 후 내려올 때만 림의 콜라이더 활성화
        if (hasPassedRim && rb.velocity.y < 0)
        {
            rimLeftCollider.enabled = true;
            rimRightCollider.enabled = true;
        }
        else
        {
            rimLeftCollider.enabled = false;
            rimRightCollider.enabled = false;
        }
    }

    void LateUpdate()
    {
        Vector3 ballPosition = gameObject.transform.position;
        // ballBottomPoint가 항상 공의 밑부분에 위치하도록 회전 고정
        ballBottomPoint.transform.position = new Vector3(ballPosition.x, ballPosition.y - 0.65f, ballPosition.z); // 로컬 좌표에서 아래쪽으로 고정
        ballBottomPoint.localRotation = Quaternion.identity; // 로컬 회전값 초기화
    }

    //공의 예상 궤적 그리기
    private void UpdateTrajectory(Vector2 force)
    {
        int pointCount = 30;
        Vector2 startPosition = transform.position;
        Vector2 velocity = force / rb.mass;
        float gravity = Physics2D.gravity.magnitude;

        // 최고점 Y 좌표 찾기
        float highestY = float.MinValue;
        int peakIndex = 0;

        // 최고점 찾기
        for (int i = 0; i < pointCount; i++)
        {
            float t = i * 0.1f;
            Vector2 pointPosition = startPosition + velocity * t + 0.5f * Physics2D.gravity * t * t;

            if (pointPosition.y > highestY)
            {
                highestY = pointPosition.y;
                peakIndex = i;
            }
        }

        // 궤도 그리기
        trajectoryLines[0].positionCount = peakIndex + 1;
        for (int i = 0; i <= peakIndex; i++)
        {
            float t = i * 0.1f;
            Vector2 pointPosition = startPosition + velocity * t + 0.5f * Physics2D.gravity * t * t;
            trajectoryLines[0].SetPosition(i, pointPosition);
        }

        // 두 번째 LineRenderer의 첫 번째 포인트를 첫 번째 LineRenderer의 마지막 포인트로 설정
        trajectoryLines[1].positionCount = pointCount - peakIndex;
        trajectoryLines[1].SetPosition(0, trajectoryLines[0].GetPosition(peakIndex));

        for (int i = peakIndex + 1; i < pointCount; i++)
        {
            float t = i * 0.1f;
            Vector2 pointPosition = startPosition + velocity * t + 0.5f * Physics2D.gravity * t * t;
            trajectoryLines[1].SetPosition(i - peakIndex, pointPosition);
        }

        // 앞부분 궤도는 림보다 앞에 렌더링
        trajectoryLines[0].sortingOrder = 2;

        // 뒷부분 궤도는 림보다 뒤에 렌더링
        trajectoryLines[1].sortingOrder = 1;
    }



    // 공이 날아가는 동안 크기를 점차적으로 줄이는 함수
    private void ScaleBallOverTime()
    {
        // 크기 변화 시간 갱신
        currentScaleTime += Time.deltaTime;

        // 공의 크기 변화 비율 계산
        float scaleProgress = currentScaleTime / scaleDuration;

        // 공의 크기를 선형적으로 줄이기
        float newScale = Mathf.Lerp(maxScale, minScale, scaleProgress);
        transform.localScale = new Vector3(newScale, newScale, newScale);

        // 크기 변화가 완료되었는지 체크
        if (scaleProgress >= 1f)
        {
            isScaling = false; // 크기 변화 중지
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rim"))
        {
            isScaling = false;
        }
    }

    // 공 리셋 함수
    private void ResetBall()
    {
        // 공의 위치와 상태를 초기화
        transform.position = initialPosition; // 초기 위치로 이동
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector2.zero;           // 속도 초기화
        rb.angularVelocity = 0f;              // 회전 속도 초기화
        rb.isKinematic = true;                // 다시 움직이지 않도록 설정

        // 공의 크기를 초기 크기로 설정
        transform.localScale = Vector3.one * maxScale;

        // 크기 변화 상태 초기화
        isScaling = false;
        currentScaleTime = 0f;

        // 림의 콜라이더 비활성화
        rimLeftCollider.enabled = false;
        rimRightCollider.enabled = false;
        hasPassedRim = false; // 림 위를 넘지 않은 상태로 초기화
    }
}
