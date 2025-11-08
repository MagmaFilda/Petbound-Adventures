using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : MonoBehaviour
{
    public static int coins = 0;
    public static List<Transform> EquippedPets;

    private PlayerInput playerInput;
    private InputAction clickAction;
    private float clicked;
    private bool canClick = true;

    private Transform targetBreakable;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        clickAction = playerInput.actions.FindAction("Click");
        EquippedPets = new List<Transform>();
    }

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Breakable"))
            {
                if (targetBreakable != hit.transform)
                {
                    targetBreakable = hit.transform;
                }           
            }
        }
        else
        {
            if (targetBreakable != null)
            {
                targetBreakable = null;
            }
        }

        if (targetBreakable != null)
        {
            clicked = clickAction.ReadValue<float>();
            if (clicked > 0 && canClick)
            {
                foreach (Transform p in EquippedPets)
                {
                    Pet pet = p.GetComponent<Pet>();
                    if (pet.mode == "Follow")
                    {
                        pet.ChangeMode("Attack", targetBreakable);
                        break;
                    }                   
                }

                canClick = false;
                StartCoroutine(ResetClick());
            }
        }
    }

    private IEnumerator ResetClick()
    {
        while (clicked > 0)
        {
            yield return null;
        }        
        canClick = true;
    }
}
