using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopController : MonoBehaviour
{
    public static HoopController Instance { get; private set; }
    public GameObject hoop;
    public GameObject rim;
    public GameObject net;
    public Collider2D rimLeftCollider;       // 림의 왼쪽 Collider2D를 참조
    public Collider2D rimRightCollider;       // 림의 오른쪽 Collider2D를 참조
    public Collider2D[] netColliders;
    public Transform rimTopPoint;        // 림의 상단 지점을 표시하는 Transform
    public SpriteRenderer rimSpriteRenderer; // 림의 SpriteRenderer
    public SpriteRenderer netSpriteRenderer; // 그물의 SpriteRenderer
    public Vector3 initRimScale;

    public List<AudioClip> netSounds;
    public List<AudioClip> rimSounds;

    public AudioSource rimAudio;
    public AudioSource netAudio;
    private void Awake()
    {
        Instance = this;
    }
    void OnEnable()
    {
        hoop = GameObject.Find("Hoop");
        rim = GameObject.Find("Hoop/Rim");
        initRimScale = rim.transform.localScale;
        net = GameObject.Find("Hoop/Rim/Net");
        rimLeftCollider = rim.transform.Find("RimLeftCollider").GetComponent<Collider2D>();
        rimRightCollider = rim.transform.Find("RimRightCollider").GetComponent<Collider2D>();
        netColliders = GameObject.Find("Hoop/Rim/Net").GetComponentsInChildren<Collider2D>();
        rimTopPoint = rim.transform.Find("RimTopPoint");
        rimSpriteRenderer = rim.GetComponent<SpriteRenderer>();
        netSpriteRenderer = net.GetComponent<SpriteRenderer>();
        rimAudio = rim.GetComponent<AudioSource>();
        netAudio = net.GetComponent<AudioSource>();

        // 시작할 때는 림의 충돌 비활성화
        rimLeftCollider.enabled = false;
        rimRightCollider.enabled = false;


    }

    // Update is called once per frame
    void Update()
    {

    }
}
