using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIShop : MonoBehaviour
{
    [SerializeField] private GameObject shop1;
    [SerializeField] private GameObject shop2;
    [SerializeField] private GameObject shopItemTemplate;
    [SerializeField] private ItemEffectManager itemEffectManager;

    [SerializeField] private Sprite[] itemSprite;
    
    private List<GameObject> _shopItem1;
    private int _index1;
    private bool _botHasBoughtItem;
    private GameObject _characterInShop;
    
    private void Start()
    {
        shop1.SetActive(false);
        _shopItem1 = new List<GameObject>();
        CreateShop();
    }

    private void CreateShop()
    {
        CreateItemShop(Item.GetName(Item.ItemType.Shield), Item.GetDescription(Item.ItemType.Shield), Item.GetCost(Item.ItemType.Shield),0, 0, itemSprite[0]);
        CreateItemShop(Item.GetName(Item.ItemType.Rain), Item.GetDescription(Item.ItemType.Rain), Item.GetCost(Item.ItemType.Rain),0, 1, itemSprite[1]);
        CreateItemShop(Item.GetName(Item.ItemType.Exit), Item.GetDescription(Item.ItemType.Exit), Item.GetCost(Item.ItemType.Exit),0, 2, itemSprite[2]);
        
        CreateItemShop(Item.GetName(Item.ItemType.Tsunami), Item.GetDescription(Item.ItemType.Tsunami), Item.GetCost(Item.ItemType.Tsunami),1, 0, itemSprite[3]);
        CreateItemShop(Item.GetName(Item.ItemType.Mouse), Item.GetDescription(Item.ItemType.Mouse), Item.GetCost(Item.ItemType.Mouse),1, 1, itemSprite[4]);
        CreateItemShop(Item.GetName(Item.ItemType.Thunder), Item.GetDescription(Item.ItemType.Thunder), Item.GetCost(Item.ItemType.Thunder),1, 2, itemSprite[5]);
    }
    
    private void CreateItemShop(string itemName, string description, int cost, int x, int y, Sprite sprite)
    {
        GameObject itemClone = Instantiate(shopItemTemplate, shop1.transform);
        itemClone.name = itemName;
        RectTransform rectTransform = itemClone.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(-60 + 120 * x, 140 - 120 * y);

        itemClone.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = sprite;

        Transform costTransform = itemClone.transform.GetChild(1).GetChild(1).transform;
        Transform descriptionTransform = itemClone.transform.GetChild(1).GetChild(0).transform;

        costTransform.GetComponentInChildren<TextMeshProUGUI>().text = cost.ToString();
        descriptionTransform.GetComponentInChildren<TextMeshProUGUI>().text = description;
        
        itemClone.transform.GetChild(1).gameObject.SetActive(false);
        
        _shopItem1.Add(itemClone);
        itemClone.transform.SetAsFirstSibling();
        
        if(cost == 0)
            costTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (shop1.activeSelf) // PLAYER 1
        {
            if (Input.GetKeyDown(KeyCode.W)) MoveSelection(-1);
            if (Input.GetKeyDown(KeyCode.S)) MoveSelection(1);
            if (Input.GetKeyDown(KeyCode.A)) MoveSelection(-3);
            if (Input.GetKeyDown(KeyCode.D)) MoveSelection(3);

            HighlightItem(_index1, _shopItem1);

            int itemCost = Item.GetCost((Item.ItemType)Enum.Parse(typeof(Item.ItemType), _shopItem1[_index1].name));
            if (Input.GetKeyDown(KeyCode.Space) && PlayerController.Instance.score >= itemCost)
            {
                PlayerController.Instance.score -= itemCost;
                ApplyItem(1, _index1);
            }
        }

        // BOT 
        if (BotController.Instance.buying && !_botHasBoughtItem)
        {
            // int item = Random.Range(3, 5);
            // item = Random.Range(0, 2) == 0 ? 1 : item;
            // int itemCost = Item.GetCost((Item.ItemType)Enum.Parse(typeof(Item.ItemType), _shopItem1[item].name));
            // ApplyItem(2, item);
            // BotController.Instance.score -= itemCost;
            // _botHasBoughtItem = true; 
            
            int index = BotController.Instance.buyItemIndex;
            if(index >= 1 && index <= 5)
                ApplyItem(2, index);
            int itemCost = Item.GetCost((Item.ItemType)Enum.Parse(typeof(Item.ItemType), _shopItem1[index].name));
            BotController.Instance.score -= itemCost;
        
            _botHasBoughtItem = true; 
        }

        if (!BotController.Instance.buying)
        {
            _botHasBoughtItem = false;
        }
    }
    
    private void ApplyItem(int player, int indexItem)
    {
        if (_shopItem1[indexItem].name == "Exit")
        {
            Exit();
            return;
        }
        itemEffectManager.GetEffect(_shopItem1[indexItem].name, player);
    }

    private void Exit()
    {
        _characterInShop.gameObject.GetComponent<PlayerController>().isShopping = false;
        foreach (var item in _shopItem1)
        {
            item.GetComponentInChildren<Image>().color = Color.white;
            item.transform.GetChild(1).gameObject.SetActive(false);
        }
        shop1.SetActive(false);
    }

    private void MoveSelection(int direction)
    {
        int row = _index1 % 3; // 0 1 2
        int col = _index1 / 3; // 0 1

        int newRow = row;
        int newCol = col;

        if (direction == -1 && row > 0) newRow--;
        if (direction == 1 && row < 2) newRow++;
        if (direction == -3 && col > 0) newCol--;
        if (direction == 3 && col < 1) newCol++;

        int newIndex = newCol * 3 + newRow;
        
        _index1 = newIndex;
    }

    private void HighlightItem(int index, List<GameObject> shopItem)
    {
        for (int i = 0; i < shopItem.Count; i++)
        {
            shopItem[i].transform.localScale = Vector3.one;
            shopItem[i].transform.GetChild(1).gameObject.SetActive(false);
        }

        shopItem[index].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        shopItem[index].transform.GetChild(1).gameObject.SetActive(true);
    }

    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _characterInShop = other.gameObject;
            _characterInShop.GetComponent<PlayerController>().isShopping = true;
            shop1.SetActive(true);
            _index1 = 0;
            HighlightItem(0, _shopItem1);
        }
    }

}
