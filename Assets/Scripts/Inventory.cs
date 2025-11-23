using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public void OpenInv()
    {
        gameObject.SetActive(true);
    }
    public void CloseInv()
    {
        gameObject.SetActive(false);
    }
    
    public void SortInventoryByDamage()
    {
        PlayerStats.PetsInInventory.Sort((a, b) => b.damage.CompareTo(a.damage));

        int n = 0;
        foreach (PetInInventory p in PlayerStats.PetsInInventory)
        {
            p.SetLayoutOrder(n);
                n++;
        }
    }
}
