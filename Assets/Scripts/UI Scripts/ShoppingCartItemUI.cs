using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShoppingCartItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _itemText;

    public void SetItemText(string newString)
    {
        _itemText.text = newString;
    }
}
