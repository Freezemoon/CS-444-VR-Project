using Game;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopDisplay : MonoBehaviour
{

    [SerializeField] private Button _closeShopButton;

    [Header("Inventory")]
    [SerializeField] private TextMeshProUGUI _inventoryTotalText;    
    [SerializeField] private TextMeshProUGUI _inventoryText;
    [SerializeField] private Button _sellInventory;

    [Header("Item Preview Section")]
    [SerializeField] private TextMeshProUGUI _playerGoldText;
    [SerializeField] private TextMeshProUGUI _itemPreviewName;
    [SerializeField] private Image _itemPreviewSprite;
    [SerializeField] private TextMeshProUGUI _itemPreviewDescription;
    [SerializeField] private TextMeshProUGUI _itemPreviewStat;
    [SerializeField] private TextMeshProUGUI _itemPreviewText;
    [SerializeField] private GameObject[] _itemPreviewObjects = new GameObject[7];

    [Header("Shopping Cart")]
    [SerializeField] private TextMeshProUGUI _basketTotalText;
    [SerializeField] private TextMeshProUGUI _basketTotalInteger;
    [SerializeField] private TextMeshProUGUI _shoppingCartText;
    [SerializeField] private Button _buyButton;
    [SerializeField] private TextMeshProUGUI _buyButtonText;
    [SerializeField] private TextMeshProUGUI[] _componentsPrice = new TextMeshProUGUI[7];
    [SerializeField] private GameObject[] _cartContent = new GameObject[7];


    public void BuyItems()
    {
        int.TryParse(_basketTotalInteger.text, out int basketTotal);
        if (GameManager.instance.GetMoney() == 0 || GameManager.instance.GetMoney() < basketTotal)
        {
            _basketTotalText.SetText("Not enough money !");
            _basketTotalInteger.SetText("");
            return;
        } else
        {
            for (int i = 0; i < 7; i++){
                var item = _cartContent[i];
                if (item.activeSelf)
                {
                    item.SetActive(false);

                    switch (i)
                    {
                        case 0:
                            GameManager.instance.AddComponent1Amount(10);
                            break;
                        case 1:
                            GameManager.instance.AddComponent2Amount(10);
                            break;
                        case 2:
                            GameManager.instance.AddComponent3Amount(10);
                            break;
                        case 3:
                            GameManager.instance.AddComponent4Amount(10);
                            break;
                        case 4:
                            GameManager.instance.AddComponent5Amount(10);
                            break;
                        case 5:
                            GameManager.instance.AddComponent6Amount(10);
                            break;
                        default:
                            GameManager.instance.AddDynamiteAmount(10);
                            break;
                    }
                }
            }
            _basketTotalText.SetText("Shopping Cart Total: ");
            GameManager.instance.AddMoney(-basketTotal);
            _basketTotalInteger.SetText("0");
            _itemPreviewText.gameObject.SetActive(true);
            _itemPreviewText.SetText("Thanks for shopping with us !");
            foreach (var item in _itemPreviewObjects) item.SetActive(false); // remove any preview UI to display thanks 
            RefreshPlayerTotal();
        }

    }

    public void SellFish()
    {
        GameManager.instance.AddMoney(GameManager.instance.GetBucketValue());
        RefreshPlayerTotal();
        RefreshCartTotal(); // in case the text was still "not enough money"
        _inventoryTotalText.SetText("Total inventory : 0G");
    }
    public void Start()
    {
        RefreshCartTotal();
        RefreshInventoryTotal();
        RefreshPlayerTotal();

    }

    public void RefreshPlayerTotal()
    {
        _playerGoldText.SetText($"You currently have: {GameManager.instance.GetMoney()}G");
    }

    public void RefreshInventoryTotal()
    {
        _inventoryTotalText.SetText($"Total inventory : {GameManager.instance.GetBucketValue()}G");
    }

    public void RefreshCartTotal()
    {
        var shoppingCartSum = 0;
        foreach (var item in _componentsPrice)
        {
            if (item.IsActive())
            {
                int.TryParse(item.text, out int temp);
                shoppingCartSum += temp;
            }
        }
        _basketTotalText.SetText("Shopping Cart Total: ");
        _basketTotalInteger.SetText($"{shoppingCartSum}");
    }
}
