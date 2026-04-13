using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.IO.IsolatedStorage;

public class Trader : MonoBehaviour
{
    [Header("Character")]
    public Transform openUI;   

    [Header("TraderUI")]
    public Transform traderUI;
    public Transform offerContainer;
    public Transform OfferTemplate;

    [Header("Offers")]
    public Resource[] availableResources;
    public int[] minReward;
    public int[] maxReward;

    private MainUI mainUI;
    private PlayerStats playerStats;

    private WaitForSeconds wait120s = new WaitForSeconds(120);

    private Dictionary<Resource, int> tradeValues = new Dictionary<Resource, int>();

    private void Awake()
    {
        StartCoroutine(GenerateValues());
        for (int i = 0; i< availableResources.Length; i++)
        {
            tradeValues[availableResources[i]] = minReward[i];
        }
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
            SetOffers();
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

    private void SetOffers()
    {
        mainUI.ClearAllChilds(offerContainer);

        foreach (Resource offerName in tradeValues.Keys)
        {
            if (playerStats.PlayerResources[offerName] > 0)
            {
                Transform newOffer = Instantiate(OfferTemplate, offerContainer);
                newOffer.Find("ResourceName").GetComponent<TextMeshProUGUI>().text = offerName.ToString();
                newOffer.Find("SellingPanel").Find("MaxAmountText").GetComponent<TextMeshProUGUI>().text = "/" + playerStats.PlayerResources[offerName].ToString();

                string valueText = " coinù";
                if (tradeValues[offerName] == 1)
                {
                    valueText = " coin";
                }
                else if (tradeValues[offerName] >= 2 && tradeValues[offerName] <= 4)
                {
                    valueText = " coiny";
                }
                newOffer.Find("TradeValue").GetComponent<TextMeshProUGUI>().text = "za " + tradeValues[offerName] + valueText;

                Image img = newOffer.Find("Image").GetComponent<Image>();
                string path = "ResourceIcons/" + offerName;
                mainUI.SetImage(img, path);

                Button sellBtn = newOffer.Find("SellBtn").GetComponent<Button>();
                Button maxBtn = newOffer.Find("MaxBtn").GetComponent<Button>();
                TMP_InputField inputAmount = newOffer.Find("SellingPanel").Find("SellAmount").GetComponent<TMP_InputField>();
                inputAmount.transform.Find("Info").GetComponent<Text>().text = offerName.ToString();

                sellBtn.onClick.AddListener(() => SellOffer(offerName, inputAmount));
                maxBtn.onClick.AddListener(() => mainUI.MaxResources(inputAmount.transform));
            }
        }
    }

    private void SellOffer(Resource resourceName, TMP_InputField inputAmount)
    {
        int sellAmount;
        if(int.TryParse(inputAmount.text, out sellAmount))
        {
            if (sellAmount <= playerStats.PlayerResources[resourceName] && sellAmount >= 0)
            {
                playerStats.PlayerResources[resourceName] -= sellAmount;
                playerStats.coins += sellAmount * tradeValues[resourceName];
                SetOffers();
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

    private IEnumerator GenerateValues()
    {
        while (true)
        {
            for (int i = 0; i < availableResources.Length; i++)
            {
                int newValue = Random.Range(minReward[i], maxReward[i] + 1);
                tradeValues[availableResources[i]] = newValue;
            }
            if (traderUI.gameObject.activeSelf)
            {
                SetOffers();
            }          
            yield return wait120s;
        }
    }
}
