using UnityEngine;
using UnityEngine.UI;
using TMPro; // agar TextMeshPro input field use kar rahe ho
using EasyUI.Toast;
using UnityEngine.SceneManagement;

public class PrivateTablePopup : MonoBehaviour
{
    [Header("Popup UI")]
    public GameObject popupPanel;   // 👈 assign karo Inspector me
    [Header("Create Table Popup UI")]
    public GameObject CreateTablePop;   // 👈 assign karo Inspector me
    public TMP_InputField joinInputField;
    public Button joinNowBtn;
    public Button createBtn;
    public Button closeBtn;
    public Button closeBtnTable;
    

    void Start()
    {
        
        popupPanel.SetActive(false); // by default hidden
        CreateTablePop.SetActive(false);

        joinNowBtn.onClick.AddListener(OnJoinNow);
        createBtn.onClick.AddListener(OnCreateTable);
        closeBtn.onClick.AddListener(ClosePopup);
        closeBtnTable.onClick.AddListener(TableClosePopup);
    }

    // 🔥 Popup Open karne ka function
    public void OpenPopup()
    {
        popupPanel.SetActive(true);
    }
    public void OpenTablePopup()
    {
        CreateTablePop.SetActive(true);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        FindObjectOfType<TabManager>().SelectTab(1); // 1 = Private
    }
    public void TableClosePopup()
    {
        CreateTablePop.SetActive(false);
        
    }

    public void OnJoinNow()
    {
        string tableId = joinInputField.text.Trim();
        Toast.Show(tableId, 3f);
        if (tableId != "")
        {
            PlayerPrefs.SetString("join_table_id", tableId);
            PlayerPrefs.SetInt("join", 1);
            PlayerPrefs.Save();

            Debug.Log("Joining Table ID: " + tableId);
            //FindObjectOfType<GameSelection>().loaddynamicscenebyname("TeenPatti_GamePlay_newPrivate.unity");
            FindObjectOfType<GameSelection>().loaddynamicscenebyname("TeenPatti_GamePlay.unity");

        }
        else
        {
            Toast.Show("⚠️ Room Code is empty!", 3f);

        }
    }

    public void OnCreateTable()
    {
        PlayerPrefs.SetInt("create", 1);
        PlayerPrefs.DeleteKey("join");
        PlayerPrefs.Save();

        Debug.Log("Creating new private table...");
        //FindObjectOfType<GameSelection>().loaddynamicscenebyname("TeenPatti_GamePlay_newPrivate.unity");
        FindObjectOfType<GameSelection>().loaddynamicscenebyname("TeenPatti_GamePlay.unity");
        
    }
}
