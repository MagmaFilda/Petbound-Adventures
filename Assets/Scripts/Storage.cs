using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Storage : MonoBehaviour
{
    [Header("Object")]
    public Transform openUI;

    [Header("StorageUI")]
    public Transform storagePanel;
    public Transform storageContainer;   
    public Transform storageInv;

    [Header("Transfer")]
    public Transform transferPanel;

    [Header("ResourceUI")]
    public Transform resourceContainer;
    public Transform ResourceUITemplate;
    public Transform resourceInv;

    private MainUI mainUI;
    private PlayerStats playerStats;

    private TextMeshProUGUI fromText;
    private TextMeshProUGUI toText;

    private void Awake()
    {
        mainUI = FindFirstObjectByType<MainUI>();

        fromText = transferPanel.Find("From").GetComponent<TextMeshProUGUI>();
        toText = transferPanel.Find("To").GetComponent<TextMeshProUGUI>();
    }
    private void Start()
    {
        playerStats = PlayerStats.Instance;
    }
    private void Update()
    {
        if (openUI.gameObject.activeSelf && Keyboard.current.eKey.wasPressedThisFrame && playerStats.canShowInteract)
        {
            SetStorageInv();
            mainUI.OpenPanel(storagePanel);
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

    private void SetStorageInv()
    {
        mainUI.OpenPanel(resourceInv);
        mainUI.SetResourceInv();
        resourceInv.Find("ExitBtn").gameObject.SetActive(false);
        resourceInv.GetComponent<RectTransform>().localScale = new Vector2(0.6f, 0.6f);
        resourceInv.GetComponent<RectTransform>().anchoredPosition = new Vector3(-201, -6.2f, 0);
        mainUI.ClearAllChilds(storageContainer);

        int resCount = 0;
        foreach (Resource res in playerStats.StorageResources.Keys.ToArray())
        {
            if (playerStats.StorageResources[res] > 0)
            {
                Transform newResourceUI = Instantiate(ResourceUITemplate, storageContainer);
                newResourceUI.Find("ResourceName").GetComponent<TextMeshProUGUI>().text = res.ToString();
                newResourceUI.Find("Amount").GetComponent<TextMeshProUGUI>().text = playerStats.StorageResources[res].ToString();
                resCount += playerStats.StorageResources[res];

                string path = "ResourceIcons/" + res.ToString();
                mainUI.SetImage(newResourceUI.Find("Image").GetComponent<Image>(), path);

                Button resBtn = newResourceUI.GetComponent<Button>();
                resBtn.enabled = true;
                resBtn.onClick.AddListener(() => SetTransfromPanel(false, newResourceUI.Find("ResourceName").GetComponent<TextMeshProUGUI>().text));
                resBtn.onClick.AddListener(() => mainUI.OpenPanel(transferPanel));
            }
        }
        storageInv.Find("Limit").GetComponent<TextMeshProUGUI>().text = resCount + "/" + playerStats.storageCapacity;

        foreach (Transform resUI in resourceContainer)
        {
            Button resBtn = resUI.GetComponent<Button>();
            resBtn.enabled = true;
            resBtn.onClick.AddListener(() => SetTransfromPanel(true, resUI.Find("ResourceName").GetComponent<TextMeshProUGUI>().text));
            resBtn.onClick.AddListener(() => mainUI.OpenPanel(transferPanel));
        }
    }
    private void SetTransfromPanel(bool toStorage, string resName)
    {
        mainUI.ClosePanel(resourceInv);
        mainUI.ClosePanel(storageInv);

        Dictionary<Resource, int> findingDictionary;

        transferPanel.Find("InputFrom").Find("Info").GetComponent<Text>().text = resName;

        if (toStorage)
        {
            findingDictionary = playerStats.StorageResources;
            fromText.text = "Inventory";
            toText.text = "Storage";
        }
        else
        {
            findingDictionary = playerStats.PlayerResources;
            fromText.text = "Storage";
            toText.text = "Inventory";
        }
        Resource transferingRes = mainUI.GetResourceFromString(resName, findingDictionary);

        string path = "ResourceIcons/" + transferingRes;
        mainUI.SetImage(transferPanel.Find("ResImg").GetComponent<Image>(), path);

        transferPanel.Find("ActualCount").Find("Text").GetComponent<TextMeshProUGUI>().text = findingDictionary[transferingRes].ToString();
        findingDictionary = null;
    }
}
