using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ShopSystem
{
    [SerializeField] private List<ShopSlot> _shopInventory;
    [SerializeField] private int _availableGold;
    [SerializeField] private float _buyMarkUp;
    [SerializeField] private float _sellMarkUp;

    public List<ShopSlot> ShopInventory => _shopInventory;
    public int AvailableGold => _availableGold;

    public float BuyMarkUp => _buyMarkUp;
    public float SellMarkUp => _sellMarkUp;
    
    public ShopSystem(int size, int gold, float buyMarkUp, float sellMarkUp)
    {
        _availableGold = gold;
        _buyMarkUp = buyMarkUp;
        _sellMarkUp = sellMarkUp;
        
        SetShopSize(size);
    }

    private void SetShopSize(int size)
    {
        _shopInventory = new List<ShopSlot>(size);

        for (int i = 0; i < size; i++)
        {
            _shopInventory.Add(new ShopSlot());
        }
    }

    public void AddToShop(InventoryItemData data, int amount)
    {
        if (ContainsItem(data, out ShopSlot shopSlot))
        {
            shopSlot.AddToStack(amount);
            return;
        }

        var freeSlot = GetFreeSlot();
        freeSlot.AssignItem(data, amount);
    }

    private ShopSlot GetFreeSlot()
    {
        var freeSlot = _shopInventory.FirstOrDefault(i => i.ItemData == null);

        if (freeSlot == null)
        {
            freeSlot = new ShopSlot();
            _shopInventory.Add(freeSlot);
        }

        return freeSlot;
    }
    
    public bool ContainsItem(InventoryItemData itemToAdd, out ShopSlot shopSlot) // Do any of our slots have the item to add in them?
    {
        shopSlot = _shopInventory.Find(i => i.ItemData == itemToAdd);
        return shopSlot != null;
    }

    public void PurchaseItem(InventoryItemData data, int amount)
    {
        if (!ContainsItem(data, out ShopSlot slot)) return;
        
        slot.RemoveFromStack(amount);
    }

    public void GainGold(int basketTotal)
    {
        _availableGold += basketTotal;
    }

    public void SellItem(InventoryItemData kvpKey, int kvpValue, int price)
    {
        AddToShop(kvpKey, kvpValue);
        ReduceGold(price);
    }

    private void ReduceGold(int price)
    {
        _availableGold -= price;
    }
}
