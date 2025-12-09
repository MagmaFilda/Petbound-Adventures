using UnityEngine;
using UnityEngine.InputSystem;

public class Shopkeeper : MonoBehaviour
{
    public Canvas openUI;
    public Transform mainUI;
    public Transform shopkeeperUI;

    private void Update()
    {
        MouseHover();

        if (openUI.enabled && Keyboard.current.eKey.wasPressedThisFrame)
        {
            MainUI uiScript = mainUI.GetComponent<MainUI>();
            uiScript.OpenPanel(shopkeeperUI);
            uiScript.SetOffers();
        }
    }
    private void MouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                openUI.enabled = true;
            }
            else
            {
                openUI.enabled = false;
            }
        }
        else
        {
            openUI.enabled = false;
        }
    }
}
