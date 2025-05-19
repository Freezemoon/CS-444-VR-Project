using Game;
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
    
    [Header("Components prefabs.")]
    [SerializeField] private GameObject redHook;
    [SerializeField] private GameObject blueHook;
    [SerializeField] private GameObject greenHook;
    [SerializeField] private GameObject redCork;
    [SerializeField] private GameObject blueCork;
    [SerializeField] private GameObject greenCork;
    
    [Header("Component spawn location.")]
    [SerializeField] private GameObject componentSpawnLocation;

    [Header("Shopping Cart")]
    [SerializeField] private TextMeshProUGUI _basketTotalText;
    [SerializeField] private TextMeshProUGUI _basketTotalInteger;
    [SerializeField] private TextMeshProUGUI _shoppingCartText;
    [SerializeField] private Button _buyButton;
    [SerializeField] private TextMeshProUGUI _buyButtonText;
    [SerializeField] private GameObject _cartBlueHookPrefab;
    [SerializeField] private GameObject _cartRedHookPrefab;
    [SerializeField] private GameObject _cartGreenHookPrefab;
    [SerializeField] private GameObject _cartRedCorkPrefab;
    [SerializeField] private GameObject _cartBlueCorkPrefab;
    [SerializeField] private GameObject _cartGreenCorkPrefab;
    [SerializeField] private GameObject _cartDynamitePrefab;
    
    [Header("Price and quantity sold")]
    [SerializeField] private int redCorkPrice = 60;
    [SerializeField] private int blueCorkPrice = 20;
    [SerializeField] private int greenCorkPrice = 40;
    [SerializeField] private int redHookPrice = 50;
    [SerializeField] private int blueHookPrice = 30;
    [SerializeField] private int greenHookPrice = 10;
    [SerializeField] private int dynamitePrice = 100;

    private int _redCorksInCart;
    private int _blueCorksInCart;
    private int _greenCorksInCart;
    private int _redHooksInCart;
    private int _blueHooksInCart;
    private int _greenHooksInCart;
    private int _dynamitesInCart;


    public void BuyItems()
    {
        int.TryParse(_basketTotalInteger.text, out int basketTotal);
        if (GameManager.instance.GetMoney() == 0 || GameManager.instance.GetMoney() < basketTotal)
        {
            _basketTotalText.SetText("Not enough money !");
            _basketTotalInteger.SetText("");
            return;
        }

        SpawnComponents();

        if (_dynamitesInCart > 0)
        {
            GameManager.instance.AddDynamiteAmount(_dynamitesInCart);
            GameManager.instance.SetDialogueState(GameManager.DialogueState.DynamiteBought);   
        }

        ResetCart();
        
        _basketTotalText.SetText("Shopping Cart Total: ");
        GameManager.instance.AddMoney(-basketTotal);
        _basketTotalInteger.SetText("0");
        _itemPreviewText.gameObject.SetActive(true);
        _itemPreviewText.SetText("Thanks for shopping with us !");
        foreach (var item in _itemPreviewObjects) item.SetActive(false); // remove any preview UI to display thanks 
        RefreshPlayerTotal();
    }

    private void ResetCart()
    {
        _blueHooksInCart = 0;
        _cartBlueHookPrefab.SetActive(false);
        
        _blueCorksInCart = 0;
        _cartBlueCorkPrefab.SetActive(false);
        
        _greenHooksInCart = 0;
        _cartGreenHookPrefab.SetActive(false);
        
        _greenCorksInCart = 0;
        _cartGreenCorkPrefab.SetActive(false);
        
        _redHooksInCart = 0;
        _cartRedHookPrefab.SetActive(false);
        
        _redCorksInCart = 0;
        _cartRedCorkPrefab.SetActive(false);
        
        _dynamitesInCart = 0;
        _cartDynamitePrefab.SetActive(false);
    }

    private void SpawnComponents()
    {
        SpawnComponent(blueHook, _blueHooksInCart);
        SpawnComponent(blueCork, _blueCorksInCart);
        SpawnComponent(greenHook, _greenHooksInCart);
        SpawnComponent(greenCork, _greenCorksInCart);
        SpawnComponent(redHook, _redHooksInCart);
        SpawnComponent(redCork, _redCorksInCart);
    }

    private void SpawnComponent(GameObject component, int amount)
    {
        if (componentSpawnLocation == null) return;
        var box = componentSpawnLocation.GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogWarning("No BoxCollider on componentSpawnLocation!");
            return;
        }

        var bounds = box.bounds;
        // compute padding in world‐units
        float padX = bounds.size.x * 0.35f;
        float padZ = bounds.size.z * 0.35f;

        // inner min/max
        float minX = bounds.min.x + padX;
        float maxX = bounds.max.x - padX;
        float minZ = bounds.min.z + padZ;
        float maxZ = bounds.max.z - padZ;

        for (int i = 0; i < amount; i++)
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);
            // a small random height above the top face
            float y = bounds.max.y + Random.Range(0.02f, 0.1f);

            var pos = new Vector3(x, y, z);
            var rot = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );

            Instantiate(component, pos, rot);
        }
    }

    public void SellFish()
    {
        int bucketValue = GameManager.instance.GetBucketValue();
        GameManager.instance.AddMoney(bucketValue);
        GameManager.instance.AddToBucket(-bucketValue);
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

    public void AddBlueHookInCart()
    {
        _blueHooksInCart++;
        RefreshCartTotal();
    }

    public void RemoveBlueHookInCart()
    {
        _blueHooksInCart--;
        if (_blueHooksInCart <= 0) _blueHooksInCart = 0;
        RefreshCartTotal();
    }
    
    public void AddGreenHookInCart()
    {
        _greenHooksInCart++;
        RefreshCartTotal();
    }

    public void RemoveGreenHookInCart()
    {
        _greenHooksInCart--;
        if (_greenHooksInCart <= 0) _greenHooksInCart = 0;
        RefreshCartTotal();
    }

    public void AddRedHookInCart()
    {
        _redHooksInCart++;
        RefreshCartTotal();
    }

    public void RemoveRedHookInCart()
    {
        _redHooksInCart--;
        if (_redHooksInCart <= 0) _redHooksInCart = 0;
        RefreshCartTotal();
    }

    public void AddBlueCorkInCart()
    {
        _blueCorksInCart++;
        RefreshCartTotal();
    }

    public void RemoveBlueCorkInCart()
    {
        _blueCorksInCart--;
        if (_blueCorksInCart <= 0) _blueCorksInCart = 0;
        RefreshCartTotal();
    }

    public void AddGreenCorkInCart()
    {
        _greenCorksInCart++;
        RefreshCartTotal();
    }

    public void RemoveGreenCorkInCart()
    {
        _greenCorksInCart--;
        if (_greenCorksInCart <= 0) _greenCorksInCart = 0;
        RefreshCartTotal();
    }

    public void AddRedCorkInCart()
    {
        _redCorksInCart++;
        RefreshCartTotal();
    }

    public void RemoveRedCorkInCart()
    {
        _redCorksInCart--;
        if (_redCorksInCart <= 0) _redCorksInCart = 0;
        RefreshCartTotal();
    }

    public void AddDynamiteToCart()
    {
        _dynamitesInCart ++;
        RefreshCartTotal();
    }
    
    public void RemoveDynamiteFromCart()
    {
        _dynamitesInCart --;
        if (_dynamitesInCart <= 0) _dynamitesInCart = 0;
        RefreshCartTotal();
    }

    public void RefreshCartTotal()
    {
        var shoppingCartSum = 0;
        
        if (_blueHooksInCart != 0)
        {
            UpdateCartPrefab(_cartBlueHookPrefab, blueHookPrice,_blueHooksInCart);
            _cartBlueHookPrefab.SetActive(true);
            shoppingCartSum += _blueHooksInCart * blueHookPrice;
        }
        else _cartBlueHookPrefab.SetActive(false);

        if (_blueCorksInCart != 0)
        {
            UpdateCartPrefab(_cartBlueCorkPrefab, blueCorkPrice, _blueCorksInCart);
            _cartBlueCorkPrefab.SetActive(true);
            shoppingCartSum += _blueCorksInCart * blueCorkPrice;
        }
        else _cartBlueCorkPrefab.SetActive(false);

        if (_greenHooksInCart != 0)
        {
            UpdateCartPrefab(_cartGreenHookPrefab, greenHookPrice, _greenHooksInCart);
            _cartGreenHookPrefab.SetActive(true);
            shoppingCartSum += _greenHooksInCart * greenHookPrice;
        }
        else _cartGreenHookPrefab.SetActive(false);

        if (_greenCorksInCart != 0)
        {
            UpdateCartPrefab(_cartGreenCorkPrefab, greenCorkPrice, _greenCorksInCart);
            _cartGreenCorkPrefab.SetActive(true);
            shoppingCartSum += _greenCorksInCart * greenCorkPrice;
        }  else _cartGreenCorkPrefab.SetActive(false);

        if (_redHooksInCart != 0)
        {
            UpdateCartPrefab(_cartRedHookPrefab, redHookPrice, _redHooksInCart);
            _cartRedHookPrefab.SetActive(true);
            shoppingCartSum += _redHooksInCart * redHookPrice;
        }  else _cartRedHookPrefab.SetActive(false);

        if (_redCorksInCart != 0)
        {
            UpdateCartPrefab(_cartRedCorkPrefab, redCorkPrice, _redCorksInCart);
            _cartRedCorkPrefab.SetActive(true);
            shoppingCartSum += _redCorksInCart * redCorkPrice;
        }  else _cartRedCorkPrefab.SetActive(false);

        if (_dynamitesInCart != 0)
        {
            UpdateCartPrefab(_cartDynamitePrefab, dynamitePrice,_dynamitesInCart);
            _cartDynamitePrefab.SetActive(true);
            shoppingCartSum += _dynamitesInCart * dynamitePrice;
        }
        else _cartDynamitePrefab.SetActive(false);
        
        _basketTotalText.SetText("Shopping Cart Total: ");
        _basketTotalInteger.SetText($"{shoppingCartSum}");
    }

    private void UpdateCartPrefab(GameObject cartPrefab, int price, int quantity)
    {
        cartPrefab.transform
            .Find("ItemAmount")
            .GetComponent<TextMeshProUGUI>()
            .text = $"{quantity}X";
        
        cartPrefab.transform
            .Find("ItemPrice")
            .GetComponent<TextMeshProUGUI>()
            .text = $"{quantity * price}";
    }
}
