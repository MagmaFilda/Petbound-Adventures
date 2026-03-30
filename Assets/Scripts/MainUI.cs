using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [Header("MainPanels")]
    public Transform btns;
    public Transform coinsText;
    public Transform questPanel;
    public Transform conversationText;
    public Transform warningPanel;
    [Header("ResourceInv")]
    public Transform resourceContainer;
    public Transform ResourceUITemplate;
    public Transform resourceInv;
    [Header("TransferPanel")]
    public Transform transferPanel;

    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        if (playerStats != null) { UpdateCoins(); }  
    }

    //Main Panels
    public void OpenPanel(Transform openingPanel)
    {
        playerStats.canShowInteract = false;

        if (openingPanel == transform.Find("Inventory"))
        {
            transform.Find("Inventory").Find("MaxPets").GetComponent<TextMeshProUGUI>().text = (playerStats.PetsInInventory.Count + playerStats.EquippedPets.Count) + "/" + playerStats.maxPets;
        }

        btns.gameObject.SetActive(false);
        coinsText.parent.gameObject.SetActive(false);
        questPanel.gameObject.SetActive(false);
        openingPanel.gameObject.SetActive(true);
    }
    public void ClosePanel(Transform closingPanel)
    {
        playerStats.canShowInteract = true;

        closingPanel.gameObject.SetActive(false);
        btns.gameObject.SetActive(true);
        coinsText.parent.gameObject.SetActive(true);
        questPanel.gameObject.SetActive(true);

        SetDeleteMode(false); // vždy když se vypne pet inv -> tak, aby se automaticky vypnul deleteMode
    }
    public void UpdateCoins()
    {
        TextMeshProUGUI coinTxt = coinsText.GetComponent<TextMeshProUGUI>();
        coinTxt.text = "Coins: " + playerStats.coins.ToString();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    // Inventories
    public void SortInventoryByDamage()
    {
        if (playerStats)
        {
            playerStats.PetsInInventory.Sort((a, b) => b.damage.CompareTo(a.damage));

            int n = 0;
            foreach (PetInInventory p in playerStats.PetsInInventory)
            {
                p.SetLayoutOrder(n);
                n++;
            }
        }        
    }
    public void SetDeleteMode(bool state)
    {       
        if (state == playerStats.deleteMode)
        {
            playerStats.deleteMode = false;
        }
        else
        {
            playerStats.deleteMode = state;
        }

        TextMeshProUGUI btnTxt = this.transform.Find("Inventory").Find("DeleteBtn").Find("Text").GetComponent<TextMeshProUGUI>();
        if (playerStats.deleteMode)
        {
            btnTxt.text = "MAZÁNÍ";
        }
        else
        {
            btnTxt.text = "Smazat pety";
        }
    }
    public Color SetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Uncommon:
                return Color.darkGreen;
            case Rarity.Rare:
                return Color.darkBlue;
            case Rarity.Epic:
                return Color.purple;
            case Rarity.Legendary:
                return Color.orange;
            default:
                return Color.darkGray;
        } 
    }
    public void SetResourceInv()
    {
        resourceInv.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        resourceInv.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        resourceInv.Find("ExitBtn").gameObject.SetActive(true);
        ClearAllChilds(resourceContainer);

        foreach (Resource res in playerStats.PlayerResources.Keys)
        {
            if (playerStats.PlayerResources[res] > 0)
            {
                Transform newResourceUI = Instantiate(ResourceUITemplate, resourceContainer);
                newResourceUI.Find("ResourceName").GetComponent<TextMeshProUGUI>().text = res.ToString();
                newResourceUI.Find("Amount").GetComponent<TextMeshProUGUI>().text = playerStats.PlayerResources[res].ToString();

                string path = "ResourceIcons/" + res.ToString();
                SetImage(newResourceUI.Find("Image").GetComponent<Image>(), path);
            }             
        }
    }

    private TextMeshProUGUI fromText;
    private TMP_InputField inputField;

    public void MaxResources(Transform inputTransform)
    {
        fromText = transferPanel.Find("From").GetComponent<TextMeshProUGUI>();
        inputField = inputTransform.GetComponent<TMP_InputField>();
        Dictionary<Resource, int> fromDictionary = playerStats.PlayerResources;

        if (fromText.text == "Storage" && inputTransform.name == "InputFrom") // !NEPREJMENOVAVAT INPUTFIELD U TRANSFERU!
        {
            fromDictionary = playerStats.StorageResources;
        }
 
        Resource maxRes = GetResourceFromString(inputField.transform.Find("Info").GetComponent<Text>().text, fromDictionary);
        inputField.text = fromDictionary[maxRes].ToString();

        fromDictionary = null;
    }
    public void TransferResources()
    {
        fromText = transferPanel.Find("From").GetComponent<TextMeshProUGUI>();
        inputField = transferPanel.Find("InputFrom").GetComponent<TMP_InputField>();
        Dictionary<Resource, int> fromDictionary;
        Dictionary<Resource, int> toDictionary;

        if (fromText.text == "Inventory")
        {
            fromDictionary = playerStats.PlayerResources;
            toDictionary = playerStats.StorageResources;
        }
        else
        {
            fromDictionary = playerStats.StorageResources;
            toDictionary = playerStats.PlayerResources;
        }

        int playerResourceCount = 0;
        int playerStorageCount = 0;
        foreach (var resName in playerStats.PlayerResources)
        {
            playerResourceCount += playerStats.PlayerResources[resName.Key];
            playerStorageCount += playerStats.StorageResources[resName.Key];
        }

        Resource transferingRes = GetResourceFromString(inputField.transform.Find("Info").GetComponent<Text>().text, fromDictionary);
        int transferValue;
        if (int.TryParse(inputField.text, out transferValue))
        {
            if (transferValue <= fromDictionary[transferingRes] && transferValue >= 0)
            {
                if ((fromText.text == "Storage" && playerResourceCount + transferValue <= playerStats.resourceCapacity) || (fromText.text == "Inventory" && playerStorageCount + transferValue <= playerStats.storageCapacity))
                {
                    fromDictionary[transferingRes] -= transferValue;
                    toDictionary[transferingRes] += transferValue;
                    ClosePanel(transferPanel);
                    inputField.text = string.Empty;
                }
                else 
                { 
                    inputField.text = string.Empty;
                    ShowWarning("Nedostatek místa v požadovaném inventáøi");
                }

            }
            else 
            { 
                inputField.text = string.Empty;
                ShowWarning("Špatné èíslo");
            }
        }
        else
        {
            inputField.text = string.Empty;
            ShowWarning("Špatný datový typ");
        }
        

        fromDictionary = null;
        toDictionary = null;
    }

    //Other
    public void Conversation(bool status, string textInConversation)
    {
        conversationText.parent.gameObject.SetActive(status);

        conversationText.GetComponent<TextMeshProUGUI>().text = textInConversation;
    }
    public void ShowWarning(string warnText)
    {
        StartCoroutine(StartWarning(warnText));
    }

    private Sprite icon;
    public void SetImage(Image img, string path)
    {
        icon = Resources.Load<Sprite>(path);
        img.sprite = icon;
    }
    public Resource GetResourceFromString(string resName, Dictionary<Resource, int> searchDictionary)
    {
        foreach (Resource res in searchDictionary.Keys)
        {
            if (res.ToString() == resName) { return res; }
        }

        return Resource.Dirt;
    }
    public void ClearAllChilds(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator StartWarning(string warnText)
    {
        warningPanel.Find("WarningText").GetComponent<TextMeshProUGUI>().text = warnText;
        warningPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        warningPanel.gameObject.SetActive(false);
    }
}
