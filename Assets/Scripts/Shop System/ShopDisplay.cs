using Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
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

    [Header("Shopping Cart")]
    [SerializeField] private TextMeshProUGUI _basketTotalText;
    [SerializeField] private TextMeshProUGUI _basketTotalInteger;
    [SerializeField] private TextMeshProUGUI _shoppingCartText;
    [SerializeField] private Button _buyButton;
    [SerializeField] private TextMeshProUGUI _buyButtonText;
    [SerializeField] private List<TextMeshProUGUI> _componentsPrice;
    [SerializeField] private List<GameObject> _cartContent;

    [SerializeField] private GameObject _itemListContentPanel;
    [SerializeField] private GameObject _shoppingCartContentPanel;
    [SerializeField] private GameObject _inventoryContentPanel;

    private int _basketTotal;

    public void BuyItems()
    {
        var basketTotal = 0;
        int.TryParse(_basketTotalInteger.text, out basketTotal);
        if (GameManager.instance.GetMoney() == 0 || GameManager.instance.GetMoney() < _basketTotal)
        {
            _basketTotalText.SetText("Not enough money !");
            _basketTotalInteger.SetText("");
            return;
        } else
        {
            foreach (var item in _cartContent)
            {
                item.SetActive(false);
            }

            _basketTotalText.SetText("Shopping Cart Total: ");
            GameManager.instance.AddMoney(-basketTotal);
            _basketTotalInteger.SetText("0");
            RefreshPlayerTotal();
            //still need to add bought components into inventory
        }

    }

    public void SellFish()
    {
        GameManager.instance.AddMoney(GameManager.instance.GetBucketValue());
        RefreshPlayerTotal();
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
        _playerGoldText.SetText($"You currently have: {GameManager.instance.GetMoney()}");
    }

    public void RefreshInventoryTotal()
    {
        _inventoryTotalText.SetText("Total inventory : " + GameManager.instance.GetBucketValue() + "G");
    }

    public void RefreshCartTotal()
    {
        var shoppingCartSum = 0;
        var temp = 0;
        foreach (var item in _componentsPrice)
        {
            if (item.IsActive())
            {
                int.TryParse(item.text, out temp);
                shoppingCartSum += temp;
            }
        }
        _basketTotalText.SetText("Shopping Cart Total: ");
        _basketTotalInteger.SetText(shoppingCartSum.ToString());
    }
}
