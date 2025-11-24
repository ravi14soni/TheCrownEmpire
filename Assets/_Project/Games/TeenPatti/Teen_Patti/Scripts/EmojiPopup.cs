using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Mkey;
using NUnit.Framework;
using System.Linq;
using DG.Tweening;
using Best.SocketIO;
using Best.SocketIO.Events;
using UnityEngine.Networking;
using System;
using Best.HTTP.SecureProtocol.Org.BouncyCastle.Crypto.Prng.Drbg;
// using System.Text.Json.Serialization;
using Newtonsoft.Json;

public class EmojiPopup : MonoBehaviour
{   
    TeenPattiSocketManager manger;
    public GameScene gameScene = GameScene.teenPatti;
    public string name_space = "/teenpatti";
    private SocketManager Manager;
    public GameObject PopupObj, OpenObj;
    public List<Button> emojiButtons, textButtons;
    //public TeenPattiSocketManager socketManager;
    //public TeenPattiPrivateSocketManager socketPrivateManager;
    public GameObject parentProfile;
    List<GameObject> profiles = new List<GameObject>();
    public List<GameObject> emojiObj = new List<GameObject>(); // For text
    //public List<GameObject> emojiTextObj = new List<GameObject>();
    //Sprite selectedSprite;
    string selectedText;

    public List<GameObject> emojiPrefabs;

    void OnEnable()
    {
        var url = Configuration.BaseSocketUrl;
        Manager = new SocketManager(new Uri(url));
        var customNamespace = Manager.GetSocket(name_space);
        customNamespace.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        customNamespace.On(SocketIOEventTypes.Disconnect, OnDisconnected);
        customNamespace.On<string>("chat-send", OnEmojisResponse);
        
        Manager.Open();
    }

    void Start()
    {
        //isPrivateSocket = PlayerPrefs.GetInt("SelectedTab") == 1 ? true : false;

        for (int i = 0; i < parentProfile.transform.childCount; i++)
        {
            profiles.Add(parentProfile.transform.GetChild(i).gameObject);
        }

        // Setup emoji buttons with index based listeners
        for (int i = 0; i < emojiButtons.Count; i++)
        {
            int index = i;
            emojiButtons[i].onClick.AddListener(() => OnEmojiButtonClick(index));
        }

        for (int i = 0; i < textButtons.Count; i++)
        {
            GameObject obj = textButtons[i].gameObject;
            textButtons[i].onClick.AddListener(() => OnTextButtonClick(obj));
        }
    }
     void OnConnected(ConnectResponse resp)
    {
Debug.Log("Emoji Connected "+resp);
    }
    
     private void OnDisconnected()
    {

    }

    void OnDisable()
    {
        Manager.Close();
    }

    private void OnEmojisResponse(string args)
    {
        Debug.Log("Gift receive + On_Gifts Json :" + args);

        try
        {
            emojiResponseAll responseData = JsonUtility.FromJson<emojiResponseAll>(args);
            Debug.Log("gift parse:" + responseData.gift_data[0].from_id);
            if (responseData.gift_data[0].to_id == "emoji")
            {
                int index = int.Parse(responseData.gift_data[0].gift_id);
                StartCoroutine(WaitEmoji(index, responseData.gift_data[0]));
            }
            else
            {
                StartCoroutine(WaitText(responseData.gift_data[0].gift_id, responseData.gift_data[0]));
            }
            
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }
private void ShowEmoji(string name,int giftID)
    {
        
        var customNamespace = Manager.GetSocket(name_space);
        if (customNamespace == null)
        {
            Debug.LogError("⚠️ Socket not connected or namespace invalid!");
            return;
        }

        try
        {
            var data = new Dictionary<string, object>()
            {
                { "user_id", Configuration.GetId() },
                { "token", Configuration.GetToken() },
                { "to_id", name },
                { "gift_id", giftID }
            };

            // Serialize using Unity JSON (Dictionary not supported — so use Newtonsoft.Json instead)
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            //ebug.Log("📤 Gifts JSON: " + jsonStr);

            // Emit to socket
            customNamespace.Emit("chat-send", jsonStr);

            //Debug.Log("✅ RES_VALUE: EMIT-Gifts Send " + jsonStr);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Socket Emit Error: " + e);
        }
    }
    private void ShowText(string name ,string text)
    {
        
        var customNamespace = Manager.GetSocket(name_space);
        if (customNamespace == null)
        {
            Debug.LogError("⚠️ Socket not connected or namespace invalid!");
            return;
        }

        try
        {
            var data = new Dictionary<string, object>()
            {
                { "user_id", Configuration.GetId() },
                { "token", Configuration.GetToken() },
                { "to_id", name },
                { "gift_id", text }
            };

            // Serialize using Unity JSON (Dictionary not supported — so use Newtonsoft.Json instead)
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            //ebug.Log("📤 Gifts JSON: " + jsonStr);

            // Emit to socket
            customNamespace.Emit("chat-send", jsonStr);

            //Debug.Log("✅ RES_VALUE: EMIT-Gifts Send " + jsonStr);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Socket Emit Error: " + e);
        }
    }
    // Takes index to select prefab
    public void OnEmojiButtonClick(int index)
    {
        if (index < 0 || index >= emojiPrefabs.Count)
        {
            Debug.LogError("Invalid emoji index: " + index);
            OnCloseClick();
            return;
        }

        Debug.Log("Emoji selected: " + emojiPrefabs[index].name);
        OnCloseClick();
        ShowEmoji("emoji",index);
        
    }

    // play prefab animation
    IEnumerator WaitEmoji(int index,emojiResponse response)
    {
        //Vector3 targetPosition = profiles.Count > 0 ? profiles[GetProfileIndex(response.from_id)].transform.position : transform.position;
        Vector3 targetPosition = profiles.Count > 0 ? emojiObj[GetProfileIndex(response.from_id)].transform.position : transform.position;
        GameObject emojiInstance = Instantiate(emojiPrefabs[index], targetPosition, Quaternion.identity);
        emojiInstance.SetActive(true);
        yield return new WaitForSeconds(2f);
        Destroy(emojiInstance);
    }

    public void OnTextButtonClick(GameObject obj)
    {
        selectedText = obj.transform.GetChild(0).GetComponent<TMP_Text>().text;
        Debug.Log("name string:" + selectedText);
        OnCloseClick();
        ShowText("text",selectedText);
        
    }

    IEnumerator WaitText(string selectedname,emojiResponse response)
    {
        // for (int i = 0; i < socketManager.TPresponseData.game_users.Count; i++)
        // {
        //     for (int j = 0; j < socketManager.profiles.Count; j++)
        //     {
        //         if (socketManager.profiles[j].GetComponent<ChaalSlider>().id == socketManager.TPresponseData.game_users[i].user_id)
        //         {
        //             socketManager.profiles[j].transform.GetChild(0).GetChild(11).GetComponent<Image>().sprite = selectedSprite;
        //         }
        //     }
        // }

        emojiObj[GetProfileIndex(response.from_id)].transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = selectedname;
        emojiObj[GetProfileIndex(response.from_id)].transform.GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        emojiObj[GetProfileIndex(response.from_id)].transform.GetChild(1).gameObject.SetActive(false);
    }

    public void OnOpenClick()
    {
        PopupObj.SetActive(true);
        OpenObj.SetActive(false);
    }

    public void OnCloseClick()
    {
        PopupObj.SetActive(false);
        OpenObj.SetActive(true);
    }

    void fun1(ref int I)
    {
        Debug.Log("name sp:" + emojiButtons[I].name);
    }

    void Update()
    {

    }
    int GetProfileIndex(string id)
    {
        for (int i = 0; i < profiles.Count; i++)
        {
            if (profiles[i].GetComponent<ChaalSlider>() != null)
                if (profiles[i].GetComponent<ChaalSlider>().id == id)
                {
                    return i;
                }
        }
        return 0;

    }
}
[Serializable]
public class emojiResponseAll
{
    public string message;
    public List<emojiResponse> gift_data;
}
[Serializable]
public class emojiResponse
{
    public string from_id;
    public string to_id;
    public string gift_id;
}

