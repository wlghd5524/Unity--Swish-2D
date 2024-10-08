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

    public GameObject hoop;
    public GameObject rim;
    public Collider2D rimLeftCollider;       // 림의 왼쪽 Collider2D를 참조
    public Collider2D rimRightCollider;       // 림의 오른쪽 Collider2D를 참조
    public EdgeCollider2D netLeftCollider;   //그물의 왼쪽 콜라이더
    public EdgeCollider2D netRightCollider;  //그물의 오른쪽 콜라이더
    public Transform rimTopPoint;        // 림의 상단 지점을 표시하는 Transform
    public Transform ballBottomPoint;
    public SpriteRenderer rimSpriteRenderer; // 림의 SpriteRenderer
    public SpriteRenderer ballSpriteRenderer; // 공의 SpriteRenderer
    public SpriteRenderer netSpriteRenderer; // 그물의 SpriteRenderer


    public float minScale;        // 공의 최소 크기
    public float maxScale;          // 공의 최대 크기
    public float baseScaleDuration; // 기본 크기 변화 시간 (초)
    private float scaleDuration;    // 공이 점점 작아지는데 걸리는 시간 (초)

    public Vector2 ballForce;

    public Vector2 ballMinForceForTrajectoryLine; //앞 림을 넘어가기 위한 최소 힘
    public Vector2 ballMaxForceForTrajectoryLine; //백보드를 넘어가기 전 최대 힘

    private bool hasPassedRim = false;   // 림을 완전히 넘어갔는지 체크

    private bool isScaling = false;     // 공의 크기 변화가 진행 중인지 확인
    private float currentScaleTime = 0f; // 크기 변화에 사용될 타이머

    void Start()
    {
        Time.timeScale = 2.0f;
        hoop = GameObject.Find("Hoop");
        rim = GameObject.Find("Hoop/Rim");
        rimLeftCollider = rim.transform.Find("RimLeftCollider").GetComponent<Collider2D>();
        rimRightCollider = rim.transform.Find("RimRightCollider").GetComponent<Collider2D>();
        netLeftCollider = GameObject.Find("Hoop/Net/NetLeftCollider").GetComponent<EdgeCollider2D>();
        netRightCollider = GameObject.Find("Hoop/Net/NetRightCollider").GetComponent<EdgeCollider2D>();
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

        // 초기에는 공이 림보다 위에 그려지도록 설정
        ballSpriteRenderer.sortingOrder = 3;
        rimSpriteRenderer.sortingOrder = 2;
        netSpriteRenderer.sortingOrder = 2;
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

            

            // 던지는 방향과 힘 계산 (드래그 방향의 반대 방향으로 던짐)
            Vector2 throwDirection = new Vector2(dragVector.x, dragVector.y).normalized;
            // 드래그 거리의 최대값을 설정하여 너무 멀리 드래그해도 일정 값 이상 힘이 증가하지 않도록 설정
            ballForce = throwDirection * Mathf.Clamp(dragDistance * forceMultiplier, baseThrowForce, maxThrowForce);

            // 유도선 업데이트
            UpdateTrajectory(ballForce);
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

            
            float forceMaginitude = Mathf.Clamp(dragDistance * forceMultiplier, baseThrowForce, maxThrowForce);
            // 던지는 방향과 힘 계산 (드래그 방향의 반대 방향으로 던짐)
            Vector2 throwDirection = new Vector2(dragVector.x, dragVector.y).normalized;
            // 드래그 거리의 최대값을 설정하여 너무 멀리 드래그해도 일정 값 이상 힘이 증가하지 않도록 설정
            ballForce = throwDirection * forceMaginitude;

            // 공에 힘을 가하여 던지기
            rb.AddForce(ballForce, ForceMode2D.Impulse);
            
            // 던지는 힘에 비례하여 회전력 추가
            float torque = forceMaginitude * rotationMultiplier * (throwDirection.x > 0 ? 1 : -1);
            rb.AddTorque(torque);

            // 공의 크기 변화 시간 설정: 던지는 힘에 비례하여 설정
            scaleDuration = baseScaleDuration / (forceMaginitude / baseThrowForce);

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
            ballSpriteRenderer.sortingOrder = 3;
            rimSpriteRenderer.sortingOrder = 2;
            netSpriteRenderer.sortingOrder = 2;
        }
        else
        {
            // 공이 림을 넘어간 후에는 림이 공보다 위에 그려지도록 설정
            ballSpriteRenderer.sortingOrder = 2;
            rimSpriteRenderer.sortingOrder = 3;
            netSpriteRenderer.sortingOrder = 3;

        }

        // 공이 림 위를 완전히 넘어갔는지 확인
        if (ballBottomPoint.position.y > rimTopPoint.position.y && ballForce.magnitude > ballMinForceForTrajectoryLine.magnitude)
        {
            hasPassedRim = true; // 림 위를 완전히 넘어감
        }

        // 공이 림 위를 완전히 넘어간 후 내려올 때만 림의 콜라이더 활성화
        if (hasPassedRim && rb.velocity.y < 0)
        {
            //공이 너무 세면 백보드를 넘어가 공이 백보드 뒤에 그려지도록 설정
            if (ballForce.magnitude > ballMaxForceForTrajectoryLine.magnitude)
            {
                ballSpriteRenderer.sortingOrder = 0;
            }
            else
            {
                rimLeftCollider.enabled = true;
                rimRightCollider.enabled = true;
                netLeftCollider.enabled = true;
                netRightCollider.enabled = true;
            }

        }
        else
        {
            rimLeftCollider.enabled = false;
            rimRightCollider.enabled = false;
            netLeftCollider.enabled = false;
            netRightCollider.enabled = false;
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
        float timeSlot = 0.01f;
        int pointCount = 300;
        Vector2 startPosition = transform.position;
        Vector2 velocity = force / rb.mass;
        float gravity = Physics2D.gravity.magnitude;

        // 최고점 Y 좌표 찾기
        float highestY = float.MinValue;
        int peakIndex = 0;

        // 최고점 찾기
        for (int i = 0; i < pointCount; i++)
        {
            float t = i * timeSlot;
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
            float t = i * timeSlot;
            Vector2 pointPosition = startPosition + velocity * t + 0.5f * Physics2D.gravity * t * t;
            trajectoryLines[0].SetPosition(i, pointPosition);
        }

        // 두 번째 LineRenderer의 첫 번째 포인트를 첫 번째 LineRenderer의 마지막 포인트로 설정
        trajectoryLines[1].positionCount = pointCount - peakIndex;
        trajectoryLines[1].SetPosition(0, trajectoryLines[0].GetPosition(peakIndex));

        for (int i = peakIndex + 1; i < pointCount; i++)
        {
            float t = i * timeSlot;
            Vector2 pointPosition = startPosition + velocity * t + 0.5f * Physics2D.gravity * t * t;
            trajectoryLines[1].SetPosition(i - peakIndex, pointPosition);
        }

        // 앞부분 궤도는 림보다 앞에 렌더링
        trajectoryLines[0].sortingOrder = 3;

        if (ballForce.magnitude > ballMinForceForTrajectoryLine.magnitude)
        {
            if (ballForce.magnitude > ballMaxForceForTrajectoryLine.magnitude)
            {
                // 뒷부분 궤도는 일정 힘 이상일 때 백보드보다 뒤에 렌더링
                trajectoryLines[1].sortingOrder = 0;
            }
            else
            {
                // 뒷부분 궤도는 일정 힘 이상일 때 림보다 앞에 렌더링
                trajectoryLines[1].sortingOrder = 2;
            }

        }
        else
        {
            trajectoryLines[1].sortingOrder = 3;
        }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Net"))
        {
            rb.velocity = new Vector2(0, rb.velocity.y * 0.5f);
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

        hoop.transform.position = new Vector2(Random.Range(-7.5f, 7.5f), Random.Range(1.5f, 4.0f));


        // 기준이 되는 림의 위치와 그에 따른 힘 (림의 초기 위치와 힘)
        Vector2 referenceRimPosition = new Vector2(0f, 2.0f);  // 림의 초기 X, Y 위치
        Vector2 referenceMinForce = new Vector2(0f, 10.3f);    // 기준 림 위치에서 필요한 최소 힘
        Vector2 referenceMaxForce = new Vector2(0f, 12.5f);    // 기준 림 위치에서 넘지 않도록 할 최대 힘

        // 현재 림의 위치 가져오기
        Vector2 currentRimPosition = hoop.transform.position;

        // 림의 위치 차이 벡터 계산
        Vector2 rimPositionDifference = currentRimPosition - referenceRimPosition;

        // 림 위치에 따라 최소 및 최대 힘을 조정 (거리 비율을 적용)
        ballMinForceForTrajectoryLine = referenceMinForce + rimPositionDifference * 0.5f;  // 거리 비율로 조정
        ballMaxForceForTrajectoryLine = referenceMaxForce + rimPositionDifference * 0.5f;  // 거리 비율로 조정

    }
}
