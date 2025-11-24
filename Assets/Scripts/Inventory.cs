using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private void Start()
    {
        UpdateCoins();
    }
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

    //Mimo inv UI veci

    public void UpdateCoins()
    {
        Text coinsText = transform.parent.Find("CoinsPanel").Find("Text").GetComponent<Text>();
        coinsText.text = "Coins: "+PlayerStats.coins.ToString();
    }
}
