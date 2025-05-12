using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop System/Shop Item List")]
public class ShopItemList : ScriptableObject
{
    [SerializeField] private List<ShopInventoryItem> _items;
    [SerializeField] private int _maxAllowedGold;
    [SerializeField] private float _sellMarkUp;
    [SerializeField] private float _buyMarkUp;

    public List<ShopInventoryItem> Items => _items;
    public int MaxAllowedGold => _maxAllowedGold;
    public float SellMarkUp => _sellMarkUp;
    public float BuyMarkUp => _buyMarkUp;
}

[System.Serializable]
public struct ShopInventoryItem
{
    public InventoryItemData ItemData;
    public int Amount;
}
