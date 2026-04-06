using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Crafting : MonoBehaviour
{
    [Header("Templates")]
    public Transform craftingTemplateUI;
    public Transform craftingSlotTemplate;
    [Header("Transforms")]
    public Transform craftingContainer;
    public Transform craftingPanel;
    public Transform openUI;

    [Header("Craftings")]
    public CraftingTemplate[] craftings;

    private MainUI mainUI;
    private PlayerStats playerStats;

    private void Start()
    {
        mainUI = FindFirstObjectByType<MainUI>();
        playerStats = PlayerStats.Instance;
    }
    private void Update()
    {
        if (openUI.gameObject.activeSelf && Keyboard.current.eKey.wasPressedThisFrame && playerStats.canShowInteract)
        {
            SetupCrafting();
            mainUI.OpenPanel(craftingPanel);
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

    private void SetupCrafting()
    {
        mainUI.ClearAllChilds(craftingContainer);
        foreach (var crafting in craftings)
        {
            Transform newCraftingUI = Instantiate(craftingTemplateUI, craftingContainer);
            Transform resourceNeedUI = newCraftingUI.Find("ResourcesNeed");
            float needResSize = 60 + 70 * (crafting.needResources.Length - 1);
            resourceNeedUI.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, needResSize);
            for (int i = 0; i < crafting.needResources.Length; i++)
            {
                Transform craftingNeedResUI = Instantiate(craftingSlotTemplate, resourceNeedUI);
                craftingNeedResUI.Find("SlotName").GetComponent<TextMeshProUGUI>().text = crafting.needResources[i].ToString();
                craftingNeedResUI.Find("Amount").GetComponent<TextMeshProUGUI>().text = crafting.needAmounts[i].ToString();
                string path = "ResourceIcons/" + crafting.needResources[i].ToString();
                mainUI.SetImage(craftingNeedResUI.Find("Image").GetComponent<Image>(), path);
            }

            Transform craftingResult = Instantiate(craftingSlotTemplate, newCraftingUI.Find("CraftingResult"));
            craftingResult.GetComponent<Button>().enabled = true;
            craftingResult.GetComponent<Button>().onClick.AddListener(() => CanCraft(crafting.needResources, crafting.needAmounts, crafting.resultResource, crafting.resultItem));
            craftingResult.Find("Amount").GetComponent<TextMeshProUGUI>().text = "1";
            if (crafting.resultResource != Resource.Nothing)
            {
                craftingResult.Find("SlotName").GetComponent<TextMeshProUGUI>().text = crafting.resultResource.ToString();
                string path = "ResourceIcons/" + crafting.resultResource.ToString();
                mainUI.SetImage(craftingResult.Find("Image").GetComponent<Image>(), path);
            }
            else //musim donastavit v Resources
            {
                craftingResult.Find("SlotName").GetComponent<TextMeshProUGUI>().text = crafting.resultItem.itemName;
                string path = "ResourceIcons/" + crafting.resultItem.itemName;
                mainUI.SetImage(craftingResult.Find("Image").GetComponent<Image>(), path);
            }
        }
    }

    private void CanCraft(Resource[] needResources, int[] needAmounts, Resource resultResource, ItemTemplate resultItem)
    {
        if (resultItem)
        {
            if (playerStats.OwnedItems.Contains(resultItem)) { return; }
        }
        for (int i = 0; i < needResources.Length; i++)
        {
            if (needAmounts[i] > playerStats.PlayerResources[needResources[i]])
            {
                return;
            }
        }

        for (int i = 0; i < needResources.Length; i++)
        {
            playerStats.PlayerResources[needResources[i]] -= needAmounts[i];
        }

        if (resultResource != Resource.Nothing)
        {
            playerStats.PlayerResources[resultResource] += 1;
        }
        else
        {
            playerStats.OwnedItems.Add(resultItem);
        }
    }
}
