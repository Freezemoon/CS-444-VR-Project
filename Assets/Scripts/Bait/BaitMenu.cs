using Game;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class BaitMenu : MonoBehaviour
{
    [Header("Buttons")]
    [Tooltip("The button prefabs for inventory elements.")]
    [SerializeField] private GameObject baitButton1;
    [SerializeField] private GameObject baitButton2;
    [SerializeField] private GameObject baitButton3;
    [SerializeField] private GameObject baitButton4;
    [SerializeField] private GameObject baitButton5;
    [SerializeField] private GameObject baitButton6;
    [SerializeField] private GameObject baitButton7;
    [SerializeField] private GameObject baitButton8;
    [SerializeField] private GameObject baitButton9;
    [SerializeField] private GameObject dynamiteButton;
    
    [Header("Counts")]
    public GameObject menuUI;
    public TextMeshProUGUI baitCountText1;
    public TextMeshProUGUI baitCountText2;
    public TextMeshProUGUI baitCountText3;
    public TextMeshProUGUI baitCountText4;
    public TextMeshProUGUI baitCountText5;
    public TextMeshProUGUI baitCountText6;
    public TextMeshProUGUI baitCountText7;
    public TextMeshProUGUI baitCountText8;
    public TextMeshProUGUI baitCountText9;
    public TextMeshProUGUI dynamiteCountText;

    [Tooltip("Drag in the XRI Left Interaction → X Button action here")]
    public InputActionReference toggleMenuAction;

    void OnEnable()
    {
        // wire up the Input Action
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.performed += OnToggleMenu;
            toggleMenuAction.action.Enable();
        }

        UpdateUI();
    }
    
    private void OnDisable()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.performed -= OnToggleMenu;
            toggleMenuAction.action.Disable();
        }
    }

    private void UpdateUI()
    {
        baitButton1.SetActive(GameManager.instance.State.BBBaitCount > 0);
        baitButton2.SetActive(GameManager.instance.State.BGBaitCount > 0);
        baitButton3.SetActive(GameManager.instance.State.BRBaitCount > 0);
        baitButton4.SetActive(GameManager.instance.State.GBBaitCount > 0);
        baitButton5.SetActive(GameManager.instance.State.GGBaitCount > 0);
        baitButton6.SetActive(GameManager.instance.State.GRBaitCount > 0);
        baitButton7.SetActive(GameManager.instance.State.RBBaitCount > 0);
        baitButton8.SetActive(GameManager.instance.State.RGBaitCount > 0);
        baitButton9.SetActive(GameManager.instance.State.RRBaitCount > 0);
        dynamiteButton.SetActive(GameManager.instance.GetDynamiteAmount() > 0);
        
        // Update the bait count text when the menu is enabled
        baitCountText1.text = "In stock: x" + GameManager.instance.State.BBBaitCount;
        baitCountText2.text = "In stock: x" + GameManager.instance.State.BGBaitCount;
        baitCountText3.text = "In stock: x" + GameManager.instance.State.BRBaitCount;
        baitCountText4.text = "In stock: x" + GameManager.instance.State.GBBaitCount;
        baitCountText5.text = "In stock: x" + GameManager.instance.State.GGBaitCount;
        baitCountText6.text = "In stock: x" + GameManager.instance.State.GRBaitCount;
        baitCountText7.text = "In stock: x" + GameManager.instance.State.RBBaitCount;
        baitCountText8.text = "In stock: x" + GameManager.instance.State.RGBaitCount;
        baitCountText9.text = "In stock: x" + GameManager.instance.State.RRBaitCount;
        dynamiteCountText.text = "In stock: x" + GameManager.instance.State.DynamiteAmount;
    }
    
    private void OnToggleMenu(InputAction.CallbackContext ctx)
    {
        UpdateUI();
        menuUI.SetActive(!menuUI.activeSelf);
    }
    
    public void OnBaitButtonClick1()
    {
        // TODO
        menuUI.SetActive(false);
    }
    public void OnBaitButtonClick2()
    {
        // TODO
        menuUI.SetActive(false);
    }
    public void OnBaitButtonClick3()
    {
        // TODO
        menuUI.SetActive(false);
    }
    public void OnBaitButtonClick4()
    {
        // TODO
        menuUI.SetActive(false);
    }
    public void OnBaitButtonClick5()
    {
        // TODO
        menuUI.SetActive(false);
    }
    public void OnBaitButtonClick6()
    {
        // TODO
        menuUI.SetActive(false);
    }
    public void OnBaitButtonClick7()
    {
        // TODO
        menuUI.SetActive(false);
    }
    public void OnBaitButtonClick8()
    {
        // TODO
        menuUI.SetActive(false);
    }
    public void OnBaitButtonClick9()
    {
        // TODO
        menuUI.SetActive(false);
    }
    
    public void OnDynamiteButtonClick()
    {
        // TODO
        menuUI.SetActive(false);
    }
}
