using System;
using System.Collections;
using System.Collections.Generic;
using AndroApps;
using DG.Tweening;
using Mkey;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class TeenPattiTableData
{
    public string id;
    public string boot_value;
    public string maximum_blind;
    public string chaal_limit;
    public string pot_limit;
    public string added_date;
    public string updated_date;
    public string isDeleted;
    public string online_members;
    public string min_amount;
}

[System.Serializable]
public class TeenPattiResponseData
{
    public string message;
    public List<TeenPattiTableData> table_data;
    public int code;
}

public class TeenPattiGetTable : MonoBehaviour
{
    public GameObject PlayerSelection;
    public TabManager tabManager;
    public PrivateTablePopup popup;
    private string lobbiesurl = Configuration.TeenPattiGettablemaster;
    public TeenPattiResponseData responseData;
    public GameObject tableprefab;

    public Transform tableparent;

    public List<GameObject> listofroom;

    public TeenPattiData data;

    public Button[] buttons;

    public GameObject addCashPopupPanel;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("TeenPattiGetTable started ✅");
        popup = FindObjectOfType<PrivateTablePopup>();
        if (popup == null)
        {
            Debug.LogError("❌ Popup not found in scene! Please add PrivatePopup prefab.");
        }
        else
        {
            Debug.Log("Popup reference OK: " + popup.gameObject.name);
        }

        StartCoroutine(PostRequest());
    }

    public void OnTeenPattiClick()
    {
        if (listofroom.Count > 0)
        {
            for (int i = 0; i < listofroom.Count; i++)
            {
                Destroy(listofroom[i]);
            }
            listofroom.Clear();
        }
        foreach (Transform item in tableparent.transform.transform)
        {
            Destroy(item.gameObject);
        }

        // StartCoroutine(PostRequest()); 
    }

    IEnumerator PostRequest()
    {
        Debug.Log("Start PostRequest");
        WWWForm form = new WWWForm();
        form.AddField("user_id", Configuration.GetId());
        form.AddField("no_of_players", "5");
        form.AddField("token", Configuration.GetToken());
        using (UnityWebRequest request = UnityWebRequest.Post(lobbiesurl, form))
        {
            request.SetRequestHeader("Token", Configuration.TokenLoginHeader);

            yield return request.SendWebRequest();

            if (
                request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError
            )
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                string response = request.downloadHandler.text;
                Debug.Log("table_list Response: " + response);

                responseData = JsonUtility.FromJson<TeenPattiResponseData>(response);

                if (responseData.code == 411)
                {
                    //InternetCheck.Instance.checkinvalid(responseData.code);
                }

                if (responseData.code == 205)
                {
                    PlayerPrefs.SetString("Gettpboot", "10");
                    Debug.Log("CHECK ALREADY IN TABLE: Loading game scene after delay.");
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        this.GetComponent<GameSelection>()
                            .loaddynamicscenebyname("TeenPatti_GamePlay.unity");
                    });
                    yield break; // Stop execution if already in a table
                }

                // Get player's current wallet balance from Configuration
                float currentWallet;
                if (!float.TryParse(Configuration.GetWallet(), out currentWallet))
                {
                    Debug.LogWarning("Failed to parse wallet amount from Configuration.GetWallet(). Defaulting to 0.");
                    currentWallet = 0f;
                }
                Debug.Log($"Player Current Wallet Balance: {currentWallet}");

                int num = responseData.table_data.Count;

                // Instantiate all table prefabs
                for (int i = 0; i < num; i++)
                {
                    GameObject roomDataGO = Instantiate(tableprefab);
                    roomDataGO.transform.SetParent(tableparent); // Use SetParent for safer UI handling
                    roomDataGO.transform.localScale = Vector3.one;
                    listofroom.Add(roomDataGO);
                }

                // Setup each table UI and button logic
                for (int i = 0; i < listofroom.Count; i++)
                {
                    int roomindex = i;
                    Transform row = listofroom[i].transform;
                    TeenPattiTableData tableData = responseData.table_data[i];

                    // Update Table UI Texts
                    row.GetChild(0).GetComponent<Text>().text = tableData.boot_value; // Boot Value
                    row.GetChild(1).GetComponent<Text>().text = tableData.min_amount; // Min Amount (Text to check against)
                    row.GetChild(2).GetComponent<Text>().text = tableData.pot_limit; // Pot Limit
                    row.GetChild(3).GetComponent<Text>().text = tableData.online_members; // Online Members

                    // Parse Min Amount to float for comparison
                    float minAmount;
                    if (!float.TryParse(tableData.min_amount, out minAmount))
                    {
                        minAmount = float.MaxValue;
                        Debug.LogError($"Failed to parse min_amount '{tableData.min_amount}' for table {tableData.id}");
                    }

                    Button playNowBtn = row.GetChild(4).GetComponent<Button>();
                    Button addCashBtn = row.GetChild(5).GetComponent<Button>();

                    // Check if player has minimum amount (still needed for VISIBILITY check)
                    bool canPlay = currentWallet >= minAmount;

                    // Set button visibility based on wallet vs minimum amount
                    // This logic remains to show EITHER Play Now OR Add Cash
                    playNowBtn.gameObject.SetActive(canPlay);
                    addCashBtn.gameObject.SetActive(!canPlay);
                    Debug.Log($"Table {tableData.id} (Min: {minAmount}): Can Play = {canPlay}");


                    playNowBtn.onClick.RemoveAllListeners();
                    playNowBtn.onClick.AddListener(() =>
                    {
                        Debug.Log("Play Now clicked: TableID = " + tableData.id + " (Attempting to load scene)");
                        TeenPattiScene(row);
                    });

                    // Configure Add Cash Button
                    addCashBtn.onClick.RemoveAllListeners();
                    if (!canPlay)
                    {
                        addCashBtn.onClick.AddListener(() =>
                        {
                            Debug.Log("Add Cash clicked for table: TableID = " + tableData.id + ", Minimum Needed: " + minAmount);
                            OnAddCashClicked();
                        });
                    }
                }
            }
        }
    }

    public void OnAddCashClicked()
    {
        Debug.Log("Initiating Add Cash process... Opening pop-up panel.");

        if (addCashPopupPanel != null)
        {
            addCashPopupPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Add Cash Popup Panel is not assigned in the Inspector!");
        }
    }

    public void CloseAddCashPopup()
    {
        Debug.Log("Closing Add Cash pop-up panel.");

        if (addCashPopupPanel != null)
        {
            addCashPopupPanel.SetActive(false);
        }
    }

    public void OnLoadMainMenu()
    {
        this.GetComponent<GameSelection>().loaddynamicscenebyname("HomePage.unity");
    }

    public void TeenPattiScene(Transform parent)
    {
        // Boot value save
        PlayerPrefs.SetString("Gettpboot", parent.GetChild(0).GetComponent<Text>().text);

        // Check tab from PlayerPrefs
        int isPrivate = PlayerPrefs.GetInt("SelectedTab", 0); // 0 = Public, 1 = Private

        if (isPrivate == 1)
        {
            Debug.Log("Loading Private Scene...");
            popup.OpenTablePopup();
        }
        else
        {
            Debug.Log("Loading Public Scene...");
            this.GetComponent<GameSelection>().loaddynamicscenebyname("TeenPatti_GamePlay.unity");
        }
    }
}