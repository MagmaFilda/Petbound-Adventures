using UnityEngine;
using UnityEngine.InputSystem;

public class QuestNPC : MonoBehaviour
{
    public QuestTemplate[] quests;
    public Canvas openUI;
    private void Update()
    {
        MouseHover();

        if (openUI.enabled && Keyboard.current.eKey.wasPressedThisFrame)
        {
            QuestInteract();
        }
    }

    private void QuestInteract()
    {
        //PlayerStats.ActiveQuests.Find();
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
