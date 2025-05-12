using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemSlot : ISerializationCallbackReceiver
{
    [NonSerialized] protected InventoryItemData itemData; // Reference to the data
    [SerializeField] protected int _itemID = -1;
    [SerializeField] protected int stackSize; // Current stack size - how many of the data do we have?

    public InventoryItemData ItemData => itemData;
    public int StackSize => stackSize;
    
    public void ClearSlot() // Clears the slot
    {
        itemData = null;
        _itemID = -1;
        stackSize = -1;
    }
    
    public void AssignItem(InventorySlot invSlot) // Assigns an item to the slot
    {
        if (itemData == invSlot.ItemData) AddToStack(invSlot.stackSize); // Does the slot contain the same item? Add to stack if so.
        else // Overwrite slot with the inventory slot that we're passing in.
        {
            itemData = invSlot.itemData;
            _itemID = itemData.ID;
            stackSize = 0;
            AddToStack(invSlot.stackSize);
        }
    }

    public void AssignItem(InventoryItemData data, int amount)
    {
        if (itemData == data) AddToStack(amount);
        else
        {
            itemData = data;
            _itemID = data.ID;
            stackSize = 0;
            AddToStack(amount);
        }
    }
    
    public void AddToStack(int amount)
    {
        stackSize += amount;
    }

    public void RemoveFromStack(int amount)
    {
        stackSize -= amount;
        if (stackSize <= 0) ClearSlot();
    }
    
    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        if (_itemID == -1) return;

        var db = Resources.Load<Database>("Database");
        itemData = db.GetItem(_itemID);
    }
}
