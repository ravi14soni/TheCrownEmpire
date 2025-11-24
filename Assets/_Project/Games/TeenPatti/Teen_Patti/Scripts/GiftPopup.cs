using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using NUnit.Framework;
using System.Linq;
using DG.Tweening;
using Best.SocketIO;
using Best.SocketIO.Events;
using UnityEngine.Networking;
using System;
using Best.HTTP.SecureProtocol.Org.BouncyCastle.Crypto.Prng.Drbg;

public enum GameScene
{
    teenPatti,
    pointRummy,
}

public class GiftPopup : MonoBehaviour
{
    TeenPattiSocketManager manger;
    public GameScene gameScene = GameScene.teenPatti;
    public string name_space = "/teenpatti";
    private SocketManager Manager;
    public GameObject parentProfile;
    List<GameObject> profiles = new List<GameObject>();
    public GameObject ParentObj, popupObj;
    GameObject targetProfile;
    GameObject moveObj;
    List<Button> giftButtons;
    public List<GameObject> giftTargetObj = new List<GameObject>();
    List<Button> targetButtons = new List<Button>();
    public List<Sprite> allSprite;
    Sprite selectedSprite;
    string selectedGiftName; // Store the gift name for audio
    bool giftMoveStart = false;

    [Header("Audio")]
    public EmojiAudioManager audioManager;

    void OnEnable()
    {
        var url = Configuration.BaseSocketUrl;
        Manager = new SocketManager(new Uri(url));
        var customNamespace = Manager.GetSocket(name_space);
        customNamespace.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        customNamespace.On(SocketIOEventTypes.Disconnect, OnDisconnected);
        customNamespace.On<string>("gifts-send", OnGiftsResponse);
        
        Manager.Open();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (audioManager == null)
            audioManager = FindObjectOfType<EmojiAudioManager>();

        for (int i = 0; i < parentProfile.transform.childCount; i++)
        {
            profiles.Add(parentProfile.transform.GetChild(i).gameObject);
        }

        giftButtons = ParentObj.transform.GetComponentsInChildren<Button>().ToList(); //assign all gift buttons
        for (int i = 0; i < giftButtons.Count; i++)
        {
            GameObject obj = giftButtons[i].gameObject;
            giftButtons[i].onClick.AddListener(() => OnGiftButtonClick(obj));
        }

        for (int i = 0; i < giftTargetObj.Count; i++)
        {
            targetButtons.Add(giftTargetObj[i].transform.GetChild(0).GetChild(0).GetComponent<Button>());
        }

        for (int i = 0; i < targetButtons.Count; i++)
        {
            GameObject obj = profiles[i].gameObject;
            targetButtons[i].onClick.AddListener(() => OnTargetButtonClick(obj));
        }
        //moveObj = giftTargetObj[0].transform.GetChild(1).gameObject;

    }

    void OnConnected(ConnectResponse resp)
    {
Debug.Log("Gifts Connected "+resp);
    }

    private void OnDisconnected()
    {

    }

    private void SendGifts(string targetUserID, string giftID)
    {
        Debug.Log("Gift send targetUserID:" + targetUserID + "<giftID>" + giftID);
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
                { "to_id", targetUserID },
                { "gift_id", giftID }
            };

            // Serialize using Unity JSON (Dictionary not supported — so use Newtonsoft.Json instead)
            string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            //ebug.Log("📤 Gifts JSON: " + jsonStr);

            // Emit to socket
            customNamespace.Emit("gifts-send", jsonStr);

            //Debug.Log("✅ RES_VALUE: EMIT-Gifts Send " + jsonStr);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Socket Emit Error: " + e);
        }
    }

    private void OnGiftsResponse(string args)
    {
        Debug.Log("Gift receive + On_Gifts Json :" + args);

        try
        {
            giftResponseAll responseData = JsonUtility.FromJson<giftResponseAll>(args);
            Debug.Log("gift parse:" + responseData.gift_data[0].from_id);
            StartCoroutine(ResponseWaitGiftMove(responseData.gift_data[0]));
            
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }
    

    void OnDisable()
    {
        Manager.Close();
    }

    public void OnTargetButtonClick(GameObject obj)
    {
        if (giftMoveStart)
            return;

        targetProfile = obj;
        popupObj.SetActive(true);
    }

    public void OnGiftButtonClick(GameObject obj)
    {
        selectedSprite = obj.transform.GetChild(0).GetComponent<Image>().sprite;
        selectedGiftName = obj.name.Replace("Button", "");
        popupObj.SetActive(false);
        //StartCoroutine(WaitGiftMove());
        string targetID = "";
        if (gameScene == GameScene.teenPatti)
        {
            targetID = targetProfile.GetComponent<ChaalSlider>().id;
        }
        else if (gameScene == GameScene.pointRummy)
        {
            targetID = targetProfile.GetComponent<PointRummyChaalSlider>().id;
        }
        else
        {
            Debug.LogError("Gift target id is not found.");
        }

        SendGifts(targetID, selectedSprite.name);
    }

    // IEnumerator WaitGiftMove()
    // {
    //     giftMoveStart = true;
    //     moveObj.GetComponent<SpriteRenderer>().sprite = selectedSprite;
    //     Vector3 movePos = moveObj.transform.position;
    //     moveObj.SetActive(true);
    //     yield return new WaitForSeconds(0.5f);
    //     moveObj.transform.DOMove(targetProfile.transform.position, 1);
    //     yield return new WaitForSeconds(2);
    //     moveObj.SetActive(false);
    //     moveObj.transform.position = movePos;
    //     targetProfile = null;
    //     giftMoveStart = false;
    // }

    IEnumerator ResponseWaitGiftMove(giftResponse response)
    {

        giftMoveStart = true;
        moveObj = giftTargetObj[GetProfileIndex(response.from_id)].transform.GetChild(1).gameObject;
        moveObj.GetComponent<SpriteRenderer>().sprite = GetSprite(response.gift_id);
        Vector3 targetPos = giftTargetObj[GetProfileIndex(response.to_id)].transform.position;
        Vector3 movePos = moveObj.transform.position;
        moveObj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        moveObj.transform.DOMove(targetPos, 1);
        yield return new WaitForSeconds(2);
        // Play the gift-specific sound on arrival
        // if (audioManager != null)
        // {
        //     audioManager.PlayGiftSound(selectedGiftName);
        // }
        moveObj.SetActive(false);
        moveObj.transform.position = movePos;
        targetProfile = null;
        giftMoveStart = false;
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

        // if (profiles[0].GetComponent<ChaalSlider>() != null)
        // {
        //     return GetIndexFromComponent<ChaalSlider>(id);
        // }
        // else if (profiles[0].GetComponent<PointRummyChaalSlider>() != null)
        // {
        //     return GetIndexFromComponent<PointRummyChaalSlider>(id);
        // }
    }

    // Example of a generic method to get a component of a specific type and compare its id
    // Usage: GetIndexFromComponent<ChaalSlider>(id)
    int GetIndexFromComponent<T>(string id) where T : Component
    {
        for (int i = 0; i < profiles.Count; i++)
        {
            T comp = profiles[i].GetComponent<T>();
            if (comp != null)
            {
                var idProp = comp.GetType().GetProperty("id");
                if (idProp != null && idProp.GetValue(comp, null) as string == id)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    Sprite GetSprite(string id)
    {
        for (int i = 0; i < allSprite.Count; i++)
        {
            if (allSprite[i].name == id)
                return allSprite[i];
        }
        return allSprite[0];//default
    }

    public void OnCloseClick()
    {
        //StopCoroutine(WaitForUserResponseGift());
        popupObj.SetActive(false);
        giftMoveStart = false;
        targetProfile = null;
    }
}

[Serializable]
public class giftResponseAll
{
    public string message;
    public List<giftResponse> gift_data;
}

[Serializable]
public class giftResponse
{
    public string from_id;
    public string to_id;
    public string gift_id;
}
