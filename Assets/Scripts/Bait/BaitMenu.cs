using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class BaitMenu : MonoBehaviour
{
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

    [Tooltip("Drag in the XRI Left Interaction → X Button action here")]
    public InputActionReference toggleMenuAction;

    private int nbOfBaits1 = 1; // Change this to the number of baits in inventory
    private int nbOfBaits2 = 3;
    private int nbOfBaits3 = 2;
    private int nbOfBaits4 = 5;
    private int nbOfBaits5 = 4;
    private int nbOfBaits6 = 5;
    private int nbOfBaits7 = 2;
    private int nbOfBaits8 = 1;
    private int nbOfBaits9 = 0;
    

    void OnEnable()
    {
        // wire up the Input Action
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.performed += OnToggleMenu;
            toggleMenuAction.action.Enable();
        }
        
        // Update the bait count text when the menu is enabled
        baitCountText1.text = "In stock: x" + nbOfBaits1;
        baitCountText2.text = "In stock: x" + nbOfBaits2;
        baitCountText3.text = "In stock: x" + nbOfBaits3;
        baitCountText4.text = "In stock: x" + nbOfBaits4;
        baitCountText5.text = "In stock: x" + nbOfBaits5;
        baitCountText6.text = "In stock: x" + nbOfBaits6;
        baitCountText7.text = "In stock: x" + nbOfBaits7;
        baitCountText8.text = "In stock: x" + nbOfBaits8;
        baitCountText9.text = "In stock: x" + nbOfBaits9;
    }
    
    private void OnDisable()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.performed -= OnToggleMenu;
            toggleMenuAction.action.Disable();
        }
    }
    
    private void OnToggleMenu(InputAction.CallbackContext ctx)
    {
        // flip the menu on/off
        menuUI.SetActive(!menuUI.activeSelf);
    }
    
    public void OnBaitButtonClick1()
    {
        if (nbOfBaits1 > 0)
        {
            Debug.Log("Bait button 1 clicked!");

            // Add the correct bait prebaf to the fishing rod

            nbOfBaits1--; // Decrease the number of baits in inventory by 1
            menuUI.SetActive(false); //close bait menu
            
        }
        else
        {
            Debug.Log("No bait 1 available!");
            // nothing to do
        }
    }
    public void OnBaitButtonClick2()
    {
        if (nbOfBaits2 > 0)
        {
            Debug.Log("Bait button 2 clicked!");

            // Add the correct bait prebaf to the fishing rod
            
            nbOfBaits2--;
            menuUI.SetActive(false);
            
        }
        else
        {
            Debug.Log("No bait 2 available!");
        }
    }
    public void OnBaitButtonClick3()
    {
        if (nbOfBaits3 > 0)
        {
            Debug.Log("Bait button 3 clicked!");

            // Add the correct bait prebaf to the fishing rod
            
            nbOfBaits3--;
            menuUI.SetActive(false);
            
        }
        else
        {
            Debug.Log("No bait 3 available!");
        }
    }
    public void OnBaitButtonClick4()
    {
        if (nbOfBaits4 > 0)
        {
            Debug.Log("Bait button 4 clicked!");

            // Add the correct bait prebaf to the fishing rod
            
            nbOfBaits4--;
            menuUI.SetActive(false);
            
        }
        else
        {
            Debug.Log("No bait 4 available!");
        }
    }
    public void OnBaitButtonClick5()
    {
        if (nbOfBaits5 > 0)
        {
            Debug.Log("Bait button 5 clicked!");

            // Add the correct bait prebaf to the fishing rod
            
            nbOfBaits5--;
            menuUI.SetActive(false);
            
        }
        else
        {
            Debug.Log("No bait 5 available!");
        }
    }
    public void OnBaitButtonClick6()
    {
        if (nbOfBaits6 > 0)
        {
            Debug.Log("Bait button 6 clicked!");

            // Add the correct bait prebaf to the fishing rod
            
            nbOfBaits6--;
            menuUI.SetActive(false);
            
        }
        else
        {
            Debug.Log("No bait 6 available!");
        }
    }
    public void OnBaitButtonClick7()
    {
        if (nbOfBaits7 > 0)
        {
            Debug.Log("Bait button 7 clicked!");

            // Add the correct bait prebaf to the fishing rod
            
            nbOfBaits7--;
            menuUI.SetActive(false);
            
        }
        else
        {
            Debug.Log("No bait 7 available!");
        }
    }
    public void OnBaitButtonClick8()
    {
        if (nbOfBaits8 > 0)
        {
            Debug.Log("Bait button 8 clicked!");

            // Add the correct bait prebaf to the fishing rod
            
            nbOfBaits8--;
            menuUI.SetActive(false);
            
        }
        else
        {
            Debug.Log("No bait 8 available!");
        }
    }
    public void OnBaitButtonClick9()
    {
        if (nbOfBaits9 > 0)
        {
            Debug.Log("Bait button 9 clicked!");

            // Add the correct bait prebaf to the fishing rod
            
            nbOfBaits9--;
            menuUI.SetActive(false);
            
        }
        else
        {
            Debug.Log("No bait 9 available!");
        }
    }
}
