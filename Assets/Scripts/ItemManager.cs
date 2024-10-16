using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemState
{
    Fireball,
    Goldenball,
    GiantRim,
    Goggle,
    Normal
}

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    public ItemState currentItemState;
    public ItemState currentItem;
    public GameObject itemGameObject;
    public List<GameObject> itemImages = new List<GameObject>();
    float currentSpeed = 0;
    public int itemIndex;
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
    void FixedUpdate()
    {
        if (currentItemState == ItemState.Fireball)
        {
            BallController.Instance.fireEffectGameObject.gameObject.SetActive(true);
            Vector2 rimPosition = HoopController.Instance.rim.transform.position;
            Vector2 ballPosition = BallController.Instance.transform.position;
            Vector2 direction = (rimPosition - ballPosition).normalized;
            
            if (!BallController.Instance.hasScored && BallController.Instance.rb.velocity.y < 0)
            {
                currentSpeed = Mathf.Min(currentSpeed + 20f * Time.fixedDeltaTime, 100f);
                Vector2 newPosition = Vector2.MoveTowards(ballPosition, rimPosition, currentSpeed * Time.fixedDeltaTime);
                BallController.Instance.rb.MovePosition(newPosition);
            }
            else if(BallController.Instance.hasScored)
            {
                currentSpeed = 0;
            }
        }
        else
        {
            BallController.Instance.fireEffectGameObject.gameObject.SetActive(false);
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
            WeatherManager.Instance.isFoggy = false;
        }
    }

    public void ItemUpdate()
    {
        if (currentItemState == ItemState.Normal && BallController.Instance.ballThrowCountForItem == 4)
        {
            for (int i = 0; i < itemImages.Count; i++)
            {
                if (i == itemIndex)
                {
                    itemImages[i].SetActive(true);
                }
                else
                {
                    itemImages[i].SetActive(false);
                }
            }
            itemGameObject.SetActive(true);
            currentItem = (ItemState)itemIndex++;

        }
        else if (currentItemState != ItemState.Normal && BallController.Instance.ballThrowCountForItem == 3)
        {
            currentItemState = ItemState.Normal;
            BallController.Instance.ballThrowCountForItem = 0;
            UIManager.Instance.itemUI.SetActive(false);
        }
    }
}
