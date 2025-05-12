using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    [SerializeField] private ShopDisplay _shopDisplay;

    private void Awake()
    {
        _shopDisplay.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ShopKeeper.OnShopWindowRequested += DisplayShopWindow;
    }

    private void OnDisable()
    {
        ShopKeeper.OnShopWindowRequested -= DisplayShopWindow;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) _shopDisplay.gameObject.SetActive(false);
    }

    private void DisplayShopWindow(ShopSystem shopSystem, PlayerInventoryHolder playerInventory)
    {
        _shopDisplay.gameObject.SetActive(true);
        _shopDisplay.DisplayShopWindow(shopSystem, playerInventory);
    }
}
