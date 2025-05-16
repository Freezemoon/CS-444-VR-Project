using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _itemPrice;
    [SerializeField] private Button _addItemToCartButton;
    [SerializeField] private Button _removeItemFromCartButton;
    
    public ShopDisplay ParentDisplay { get; private set; }
    
    private void Awake()
    {   
        _addItemToCartButton?.onClick.AddListener(AddItemToCart);
        _removeItemFromCartButton?.onClick.AddListener(RemoveItemFromCart);
        ParentDisplay = transform.parent.GetComponentInParent<ShopDisplay>();
    }
    public void Init(ShopSlot slot, float markUp)
    {
    }

    public TextMeshProUGUI getPrice(){
        return _itemPrice;
    }

    private void Update(){
    }

    private void RemoveItemFromCart()
    {
        ParentDisplay.RemoveItemFromCart(this);
    }

    private void AddItemToCart()
    {
        ParentDisplay.AddItemToCart(this);
    }

    
}
