using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BaitMenuManager : MonoBehaviour
{
    [Header("Menu")]
    public GameObject menuUI;
    public Button[] baitButtons;

    [Header("Input")]
    public InputActionReference openMenuAction; // e.g., bound to controller button (like Y or X)

    private void OnEnable()
    {
        if (openMenuAction != null)
            openMenuAction.action.performed += OnOpenMenu;

        if (openMenuAction != null)
            openMenuAction.action.Enable();
    }

    private void OnDisable()
    {
        if (openMenuAction != null)
            openMenuAction.action.performed -= OnOpenMenu;

        if (openMenuAction != null)
            openMenuAction.action.Disable();
    }

    private void Start()
    {
        // Assign listeners to bait buttons
        for (int i = 0; i < baitButtons.Length; i++)
        {
            int index = i; // local copy for lambda
            baitButtons[i].onClick.AddListener(() => OnBaitSelected(index));
        }

        CloseMenu(); // start hidden
    }

    private void OnOpenMenu(InputAction.CallbackContext context)
    {
        menuUI.SetActive(true);
    }

    private void OnBaitSelected(int index)
    {
        Debug.Log($"[BaitMenu] Bait selected: index {index}");
        CloseMenu();
    }

    public void CloseMenu()
    {
        menuUI.SetActive(false);
    }
}
