using AndroApps;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class PokerRoom
{
    public string id;
    public string boot_value;
    public string city;
    public string image;
    public string image_bg;
    public string blind_1;
    public string blind_2;
    public string maximum_blind;
    public string chaal_limit;
    public string pot_limit;
    public string added_date;
    public string updated_date;
    public string isDeleted;
    public string online_members;
}

[Serializable]
public class PokerRoomData
{
    public List<PokerRoom> table_data;
    public int code;
}

public class PokerTableListing : MonoBehaviour
{

    [Header("Poker Table Data")] public PokerRoomData pokertablelisting;
    [Header("Poker Data")] public PokerData data;
    [Header("Poker Table prefab")] public GameObject pokertableprefab;
    public Transform tablecontent;
    public List<GameObject> listofroom;
    public List<string> blind1valuelist;

    void Start()
    {
        StartCoroutine(PostRequestgettablelisting(Configuration.poker_get_table_master));
    }


    public void ClickPocker()
    {
        StartCoroutine(PostRequestgettablelisting(Configuration.poker_get_table_master));
    }

    #region Show Poker Table Listing

    IEnumerator PostRequestgettablelisting(string url)
    {
        if (listofroom.Count > 0)
        {
            foreach (GameObject item in listofroom)
            {
                Destroy(item);
            }
            listofroom.Clear();

        }
        WWWForm form = new WWWForm();
        form.AddField("user_id", Configuration.GetId());
        form.AddField("token", Configuration.GetToken());

        Debug.Log("RES_Check + user_id: " + Configuration.GetId() + " token: " + Configuration.GetToken());

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            if (Configuration.TokenLoginHeader != null)
            {
                request.SetRequestHeader("Token", Configuration.TokenLoginHeader);
            }
            else
            {
                Debug.LogError("Error: TokenLogIn is null.");
                yield break;
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                string response = request.downloadHandler.text;
                Debug.Log("RES_Chck + Poker table_list Response: " + response);
                pokertablelisting = JsonUtility.FromJson<PokerRoomData>(response);

                ////InternetCheck.Instance.checkinvalid(pokertablelisting.code);

                if (pokertablelisting.code == 200)
                {
                    for (int i = 0; i < pokertablelisting.table_data.Count; i++)
                    {
                        GameObject obj = Instantiate(pokertableprefab, tablecontent);
                        listofroom.Add(obj);
                        blind1valuelist.Add(pokertablelisting.table_data[i].blind_1);
                    }

                    for (int i = 0; i < listofroom.Count; i++)
                    {
                        int roomindex = i;
                        StartCoroutine(DownloadImage(pokertablelisting.table_data[i].image, listofroom[i].transform.GetChild(0).GetComponent<Image>()));
                        listofroom[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = pokertablelisting.table_data[i].city;
                        listofroom[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Bet starts from " + FormatNumber(pokertablelisting.table_data[i].boot_value);
                        listofroom[i].transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => Pokerscene(blind1valuelist[roomindex]));
                    }
                }
                else if (pokertablelisting.code == 205)
                {
                    //SceneManager.LoadScene("PokerGamePlay");
                    this.GetComponent<GameSelection>().loaddynamicscenebyname("PokerGamePlay.unity");

                }
            }
        }
    }

    #endregion

    #region download Table URL Image and format wallet 

    public IEnumerator DownloadImage(string ProfileAvatar, Image img)
    {
        string Url = Configuration.ProfileImage;

        Debug.Log("Res_Check_Profile_URL " + Url + ProfileAvatar);

        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(Url + ProfileAvatar))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("DownloadImage In ");
                Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(webRequest);
                img.sprite = Sprite.Create(downloadedTexture, new Rect(0, 0, downloadedTexture.width, downloadedTexture.height), Vector2.zero);
            }
            else
            {
                Debug.Log("Error: " + webRequest.error);
            }
            // long UsedMemory;
            // UsedMemory = System.Diagnostics.Process.GetCurrentProcess().PagedMemorySize64;
            // if (UsedMemory > 1073741824)
            // {
            //     GC.Collect();
            // }
            yield return null;
        }
    }

    string FormatNumber(string number)
    {
        if (float.Parse(number) >= 1000 && float.Parse(number) < 10000)
        {
            return (float.Parse(number) / 1000f).ToString("0.0") + "k";
        }
        else if (float.Parse(number) >= 10000)
        {
            return (float.Parse(number) / 1000).ToString("0.#") + "k";
        }
        else
        {
            return number.ToString();
        }
    }

    #endregion

    #region adding function to table buttons

    public void Pokerscene(string number)
    {
        //NewAudioManager.instance.PlayButtonSound();
        //data.Blind1_Value = number;
        //   CommonUtil.ShowToastDebug("Number By List" + number);
        //  Debug.Log("Number By List DATA : " + data.Blind1_Value);

        PlayerPrefs.SetString("PokerDataTablePerClick", number);
        data.Blind1_Value = PlayerPrefs.GetString("PokerDataTablePerClick");
        //SceneManager.LoadScene("PokerGamePlay");
        this.GetComponent<GameSelection>().loaddynamicscenebyname("PokerGamePlay.unity");

    }
    public void Close()
    {
        this.GetComponent<GameSelection>().loaddynamicscenebyname("HomePage.unity");

    }

    #endregion
}
