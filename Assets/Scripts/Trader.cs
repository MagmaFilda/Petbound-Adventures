using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Trader : MonoBehaviour
{
    [Header("MainUI")]
    public Transform mainUI;

    [Header("Character")]
    public Transform openUI;   
    public Transform hitbox;

    [Header("TraderUI")]
    public Transform traderUI;
    public Transform offerContainer;
    public Transform OfferTemplate;

    private MainUI uiScript;
    private PlayerStats playerStats;

    private Dictionary<Resource, int> tradeValues = new Dictionary<Resource, int>();

    private void Awake()
    {
        tradeValues.Add(Resource.Dirt, 1);
        tradeValues.Add(Resource.Grass, 2);

        uiScript = mainUI.GetComponent<MainUI>();
    }
    private void Start()
    {
        playerStats = PlayerStats.Instance;
    }
    private void Update()
    {
        if (openUI.gameObject.activeSelf && Keyboard.current.eKey.wasPressedThisFrame && playerStats.canShowInteract)
        {
            uiScript.OpenPanel(traderUI);
            SetOffers(tradeValues);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(false);
        }
    }

    private void SetOffers(Dictionary<Resource, int> values)
    {
        uiScript.ClearAllChilds(offerContainer);

        foreach (Resource offerName in playerStats.PlayerResources.Keys)
        {
            if (playerStats.PlayerResources[offerName] > 0)
            {
                Transform newOffer = Instantiate(OfferTemplate, offerContainer);
                newOffer.Find("ResourceName").GetComponent<TextMeshProUGUI>().text = offerName.ToString();
                newOffer.Find("SellingPanel").Find("MaxAmountText").GetComponent<TextMeshProUGUI>().text = "/" + playerStats.PlayerResources[offerName].ToString();

                Image img = newOffer.Find("Image").GetComponent<Image>();
                string path = "ResourceIcons/" + offerName;
                uiScript.SetImage(img, path);

                Button sellBtn = newOffer.Find("SellBtn").GetComponent<Button>();
                Button maxBtn = newOffer.Find("MaxBtn").GetComponent<Button>();
                TMP_InputField inputAmount = newOffer.Find("SellingPanel").Find("SellAmount").GetComponent<TMP_InputField>();
                inputAmount.transform.Find("Info").GetComponent<Text>().text = offerName.ToString();

                sellBtn.onClick.AddListener(() => SellOffer(offerName, inputAmount, values));
                maxBtn.onClick.AddListener(() => uiScript.MaxResources(inputAmount.transform));
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
                uiScript.ShowWarning("Špatné èíslo");
            }
        }
        else
        {
            inputAmount.text = string.Empty;
            uiScript.ShowWarning("Špatný datový typ");
        }
    }
}
