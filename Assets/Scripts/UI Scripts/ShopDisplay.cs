using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopDisplay : MonoBehaviour
{
    [SerializeField] private ShopSlotUI _shopSlotPrefab;
    [SerializeField] private ShoppingCartItemUI _shoppingCartItemPrefab;

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
    [SerializeField] private TextMeshProUGUI _shoppingCartText;
    [SerializeField] private Button _buyButton;
    [SerializeField] private TextMeshProUGUI _buyButtonText;

    [SerializeField] private GameObject _itemListContentPanel;
    [SerializeField] private GameObject _shoppingCartContentPanel;
    [SerializeField] private GameObject _inventoryContentPanel;

    private int _basketTotal;
    
    private bool _isSelling;
    
    private ShopSystem _shopSystem;
    private PlayerInventoryHolder _playerInventoryHolder;

    private Dictionary<InventoryItemData, int> _shoppingCart = new Dictionary<InventoryItemData, int>();

    private Dictionary<InventoryItemData, int> _fishInventory = new Dictionary<InventoryItemData, int>();

    private Dictionary<InventoryItemData, ShoppingCartItemUI> _shoppingCartUI =
        new Dictionary<InventoryItemData, ShoppingCartItemUI>();

    public void DisplayShopWindow(ShopSystem shopSystem, PlayerInventoryHolder playerInventoryHolder)
    {
        _shopSystem = shopSystem;
        _playerInventoryHolder = playerInventoryHolder;

        RefreshDisplay();
    }

    
    private void RefreshDisplay()
    {
        if (_buyButton != null)
        {
            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(SellFish);
        }
        
        ClearSlots();
        ClearItemPreview();

        _basketTotalText.enabled = false;
        _buyButton.gameObject.SetActive(false);
        _basketTotal = 0;
        _playerGoldText.text = $"You currently have: {_playerInventoryHolder.PrimaryInventorySystem.Gold}";

        DisplayShopInventory();
       
    }

    private void BuyItems()
    {
        if (_playerInventoryHolder.PrimaryInventorySystem.Gold < _basketTotal) return;

        if (!_playerInventoryHolder.PrimaryInventorySystem.CheckInventoryRemaining(_shoppingCart)) return;

        foreach (var kvp in _shoppingCart)
        {
            _shopSystem.PurchaseItem(kvp.Key, kvp.Value);

            for (int i = 0; i < kvp.Value; i++)
            {
                _playerInventoryHolder.PrimaryInventorySystem.AddToInventory(kvp.Key, 1);
            }
        }

        _shopSystem.GainGold(_basketTotal);
        _playerInventoryHolder.PrimaryInventorySystem.SpendGold(_basketTotal);
        
        RefreshDisplay();
    }

    private void SellFish()
    {
        var sum = 0;
        foreach (var kvp in _fishInventory)
        {
            sum += kvp.Value;
        }

        _playerInventoryHolder.PrimaryInventorySystem.GainGold(sum);
        _playerGoldText.text = $"You currently have: {_playerInventoryHolder.PrimaryInventorySystem.Gold}";
        
        RefreshDisplay();
    }

    private void ClearSlots()
    {
        _shoppingCart = new Dictionary<InventoryItemData, int>();
        _shoppingCartUI = new Dictionary<InventoryItemData, ShoppingCartItemUI>();
        
        foreach (var item in _itemListContentPanel.transform.Cast<Transform>())
        {
            Destroy(item.gameObject);
        }
        
        foreach (var item in _shoppingCartContentPanel.transform.Cast<Transform>())
        {
            Destroy(item.gameObject);
        }
    }

    private void DisplayShopInventory()
    {
        foreach (var item in _shopSystem.ShopInventory)
        {
            if (item.ItemData == null) continue;

            var shopSlot = Instantiate(_shopSlotPrefab, _itemListContentPanel.transform);
            shopSlot.Init(item, _shopSystem.BuyMarkUp);
        }
    }

    private void DisplayPlayerInventory()
    {
        foreach (var item in _playerInventoryHolder.PrimaryInventorySystem.GetAllItemsHeld())
        {
            var tempSlot = new ShopSlot();
            tempSlot.AssignItem(item.Key, item.Value);

            var shopSlot = Instantiate(_shopSlotPrefab, _itemListContentPanel.transform);
            shopSlot.Init(tempSlot, _shopSystem.SellMarkUp);
        }
    }
    
    public void RemoveItemFromCart(ShopSlotUI shopSlotUI)
    {
        var price = 0;
        int.TryParse(shopSlotUI.getPrice().text, out price);
        _basketTotal -= price;
        _basketTotalText.text = $"Total: {_basketTotal}G";
        return;
    }

    private void ClearItemPreview()
    {
        _itemPreviewSprite.sprite = null;
        _itemPreviewSprite.color = Color.clear;
        _itemPreviewName.text = "";
        _itemPreviewDescription.text = "";
    }
    
    public void AddItemToCart(ShopSlotUI shopSlotUI)
    {

        var price = 0;
        int.TryParse(shopSlotUI.getPrice().text, out price);
        
        _basketTotal += price;
        _basketTotalText.text = $"Total: {_basketTotal}G";
        _basketTotalText.enabled = true;
        _buyButton.gameObject.SetActive(true);
         
        CheckCartVsAvailableGold();
    } 
    
    private void CheckCartVsAvailableGold()
    {
        var goldToCheck = _playerInventoryHolder.PrimaryInventorySystem.Gold;
        _basketTotalText.color = _basketTotal > goldToCheck ? Color.red : Color.white;
    } 
}
