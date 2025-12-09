using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public Transform btns;
    public Transform coinsPanel;
    public Transform resourceContainer;
    public Transform conversationPanel;

    public Transform OfferTemplate;

    private PlayerStats playerStats = PlayerStats.Instance;

    private void Start()
    {
        UpdateCoins();
    }
    public void OpenPanel(Transform openingPanel)
    {     
        btns.gameObject.SetActive(false);
        coinsPanel.parent.gameObject.SetActive(false);
        openingPanel.gameObject.SetActive(true);
    }
    public void ClosePanel(Transform closingPanel)
    {
        closingPanel.gameObject.SetActive(false);
        btns.gameObject.SetActive(true);
        coinsPanel.parent.gameObject.SetActive(true);
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

    public void SetOffers()
    {
        foreach (Transform child in resourceContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Resource offerName in playerStats.PlayerResources.Keys)
        {
            if (playerStats.PlayerResources[offerName] > 0)
            {
                Transform newOffer = Instantiate(OfferTemplate, resourceContainer);
                newOffer.Find("ResourceName").GetComponent<Text>().text = offerName.ToString();
                newOffer.Find("SellingPanel").Find("MaxAmountText").GetComponent<Text>().text = "/" + playerStats.PlayerResources[offerName].ToString();

                Button sellBtn = newOffer.Find("SellBtn").GetComponent<Button>();
                Transform amountTransform = newOffer.Find("SellingPanel").Find("SellAmount").Find("Text");
                sellBtn.onClick.AddListener(() => SellOffer(offerName, amountTransform));
            }           
        }
    }

    public void Conversation(bool status, string textInConversation)
    {
        conversationPanel.parent.gameObject.SetActive(status);

        conversationPanel.GetComponent<Text>().text = textInConversation;
    }

    private void SellOffer(Resource resourceName, Transform amountTransform)
    {
        int sellAmount = int.Parse(amountTransform.GetComponent<Text>().text);
        if (sellAmount <= playerStats.PlayerResources[resourceName])
        {
            playerStats.PlayerResources[resourceName] -= sellAmount;
            playerStats.coins += sellAmount;  
            SetOffers();
        }
    }
}
