using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemState
{
    Fireball,
    Goldenball,
    Goggle,
    GiantRim,
    Normal
}

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    public ItemState currentItemState;
    public ItemState currentItem;
    public GameObject itemGameObject;
    public List<GameObject> itemImages = new List<GameObject>();
    private void Awake()
    {
        Instance = this;
    }
    void OnEnable()
    {
        currentItemState = ItemState.Normal;
        currentItem = ItemState.Normal;
        itemGameObject = GameObject.Find("Item");
        for (int i = 0; i < itemGameObject.transform.childCount; i++)
        {
            itemImages.Add(itemGameObject.transform.GetChild(i).gameObject);
        }
        itemGameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        itemImages[(int)currentItem].SetActive(true);

        if (currentItemState == ItemState.Fireball)
        {
            BallController.Instance.fireEffect.gameObject.SetActive(true);
        }
        else
        {
            BallController.Instance.fireEffect.gameObject.SetActive(false);
        }
        if (currentItemState == ItemState.Goldenball)
        {
            BallController.Instance.ballSpriteRenderers[1].gameObject.SetActive(false);
            BallController.Instance.ballSpriteRenderers[2].gameObject.SetActive(true);
        }
        else
        {
            BallController.Instance.ballSpriteRenderers[1].gameObject.SetActive(true);
            BallController.Instance.ballSpriteRenderers[2].gameObject.SetActive(false);
        }
        if (currentItemState == ItemState.GiantRim)
        {
            HoopController.Instance.rim.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
        else
        {
            HoopController.Instance.rim.transform.localScale = HoopController.Instance.initRimScale;
        }
        if (currentItemState == ItemState.Goggle)
        {
            WeatherManager.Instance.fogOn = false;
        }
    }
}
