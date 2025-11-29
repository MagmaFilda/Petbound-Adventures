using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public Transform Inventory;
    public Transform coinsPanel;

    private PlayerStats playerStats = PlayerStats.Instance;

    private void Start()
    {
        UpdateCoins();
    }
    public void OpenInv()
    {
        Inventory.gameObject.SetActive(true);
    }
    public void CloseInv()
    {
        Inventory.gameObject.SetActive(false);
    }
    
    public void SortInventoryByDamage()
    {
        playerStats.PetsInInventory.Sort((a, b) => b.damage.CompareTo(a.damage));

        int n = 0;
        foreach (PetInInventory p in playerStats.PetsInInventory)
        {
            p.SetLayoutOrder(n);
                n++;
        }
    }

    //Mimo inv UI veci

    public void UpdateCoins()
    {
        Text coinsText = coinsPanel.GetComponent<Text>();
        coinsText.text = "Coins: "+playerStats.coins.ToString();
    }
}
