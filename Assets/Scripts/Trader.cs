using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Trader : MonoBehaviour
{
    public Canvas openUI;
    public Transform mainUI;
    public Transform traderUI;

    private Dictionary<Resource, int> tradeValues = new Dictionary<Resource, int>();

    private void Awake()
    {
        tradeValues.Add(Resource.Dirt, 1);
        tradeValues.Add(Resource.Grass, 2);
    }
    private void Update()
    {
        MouseHover();

        if (openUI.enabled && Keyboard.current.eKey.wasPressedThisFrame)
        {
            MainUI uiScript = mainUI.GetComponent<MainUI>();
            uiScript.OpenPanel(traderUI);
            uiScript.SetOffers(tradeValues);
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
