using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public GameObject continueBtn;

    private string path;

    private void Start()
    {
        path = Application.persistentDataPath + "/data.json";
        if (File.Exists(path))
        {
            continueBtn.SetActive(true);
        }
    }

    public void NewGame()
    {
        SaveData clearData = new SaveData();

        string json = JsonUtility.ToJson(clearData, true);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        string encoded = System.Convert.ToBase64String(bytes);
        File.WriteAllText(path, encoded);

        SceneManager.LoadScene("GameScene");
    }
    public void ContinueGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
