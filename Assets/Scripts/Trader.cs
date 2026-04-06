using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Trader : MonoBehaviour
{
    [Header("Character")]
    public Transform openUI;   

    [Header("TraderUI")]
    public Transform traderUI;
    public Transform offerContainer;
    public Transform OfferTemplate;

    private MainUI mainUI;
    private PlayerStats playerStats;

    private Dictionary<Resource, int> tradeValues = new Dictionary<Resource, int>();

    private void Awake()
    {
        tradeValues.Add(Resource.Dirt, 1);
        tradeValues.Add(Resource.Grass, 2);
        tradeValues.Add(Resource.Brick, 50);
        tradeValues.Add(Resource.Ceramic, 750);      
    }
    private void Start()
    {
        playerStats = PlayerStats.Instance;
        mainUI = FindFirstObjectByType<MainUI>();
    }
    private void Update()
    {
        if (openUI.gameObject.activeSelf && Keyboard.current.eKey.wasPressedThisFrame && playerStats.canShowInteract)
        {
            mainUI.OpenPanel(traderUI);
            SetOffers(tradeValues);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(true);
            transform.Find("NpcIcon").gameObject.SetActive(false);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(false);
            transform.Find("NpcIcon").gameObject.SetActive(true);
        }
    }

    private void SetOffers(Dictionary<Resource, int> values)
    {
        mainUI.ClearAllChilds(offerContainer);

        foreach (Resource offerName in tradeValues.Keys)
        {
            if (playerStats.PlayerResources[offerName] > 0)
            {
                Transform newOffer = Instantiate(OfferTemplate, offerContainer);
                newOffer.Find("ResourceName").GetComponent<TextMeshProUGUI>().text = offerName.ToString();
                newOffer.Find("SellingPanel").Find("MaxAmountText").GetComponent<TextMeshProUGUI>().text = "/" + playerStats.PlayerResources[offerName].ToString();

                Image img = newOffer.Find("Image").GetComponent<Image>();
                string path = "ResourceIcons/" + offerName;
                mainUI.SetImage(img, path);

                Button sellBtn = newOffer.Find("SellBtn").GetComponent<Button>();
                Button maxBtn = newOffer.Find("MaxBtn").GetComponent<Button>();
                TMP_InputField inputAmount = newOffer.Find("SellingPanel").Find("SellAmount").GetComponent<TMP_InputField>();
                inputAmount.transform.Find("Info").GetComponent<Text>().text = offerName.ToString();

                sellBtn.onClick.AddListener(() => SellOffer(offerName, inputAmount, values));
                maxBtn.onClick.AddListener(() => mainUI.MaxResources(inputAmount.transform));
            }
        }
    }

    private void SellOffer(Resource resourceName, TMP_InputField inputAmount, Dictionary<Resource, int> values)
    {
        int sellAmount;
        if(int.TryParse(inputAmount.text, out sellAmount))
        {
            if (sellAmount <= playerStats.PlayerResources[resourceName] && sellAmount >= 0)
            {
                playerStats.PlayerResources[resourceName] -= sellAmount;
                playerStats.coins += sellAmount * values[resourceName];
                SetOffers(values);
            }
            else
            {
                inputAmount.text = string.Empty;
                mainUI.ShowWarning("Špatné èíslo");
            }
        }
        else
        {
            inputAmount.text = string.Empty;
            mainUI.ShowWarning("Špatný datový typ");
        }
    }
}
