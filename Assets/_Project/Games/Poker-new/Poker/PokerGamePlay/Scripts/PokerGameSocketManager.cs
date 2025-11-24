using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Best.SocketIO;
using Best.SocketIO.Events;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;
using System.Linq;
using System.Data;
using AndroApps;
using EasyUI.Toast;
using UnityEngine.SocialPlatforms.Impl;
using Mkey;
using DG.Tweening;
using static PokerResponses;

public class PokerGameSocketManager : MonoBehaviour
{
    [Header("Controller details")] private string CustomNamespace = "/poker";
    [Header("Table Details")] public string TableID;
    [Header("Game Details")] public string GameID;

    public PokerData PData;
    public List<GameObject> profiles;
    public bool gamestart;
    public string gameID;
    [Header("Socket Details")] private SocketManager Manager;
    [Header("Leave Table")] public PokerLeaveTableResponse leavetable;
    [Header("Join Table")] public PokerJoinTableResponse jointable;
    public TextMeshProUGUI waittext;
    public GameObject waitingtext;
    [Header("Game Data Class")] public PokerGameData gamedata;
    public List<PokerGameDataTableUser> playersactiveontable;
    public int chaaltimer;
    public string chaalID;
    public PattiAnim animateCards;
    public List<Sprite> deck;
    public SpriteRenderer mycard1, mycard2;
    public GameObject buttonspanel, buttonspanelcheck, totalbet;
    public int raiseclicked = 0;
    public float amount;
    public bool gameended = false, middle3anim = false, middle4anim = false, middle5anim = false;
    public TextMeshProUGUI calltext, raisetext, raisetext2;
    public int role;
    public SpriteRenderer middlecard1, middlecard2, middlecard3, middlecard4, middlecard5;
    public List<string> evaluatecards = new List<string>();
    public List<int> mycardvalues = new List<int>();
    public List<string> mycardsuits = new List<string>();
    public Slider raiseSlider, raiseSlider2;
    public TextMeshProUGUI slidertext, slidettext2, typetext;
    public float slideramount;
    #region music and sounds
    public Button[] buttons;
    #endregion
    public Toggle soundToggle;
    public Toggle musicToggle;
    public bool isstarted = false;

    public Image profileImage;
    public TextMeshProUGUI UserWalletText;
    void Start()
    {
        if (raiseSlider != null)
        {
            raiseSlider.onValueChanged.AddListener(SliderRaise);
            raiseSlider2.onValueChanged.AddListener(SliderRaise);
        }

    }

    private void OnEnable()
    {
        profiles[0].transform.GetChild(0).gameObject.SetActive(true);
        profiles[0].transform.GetChild(1).gameObject.SetActive(false);
        profiles[0].transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = Configuration.GetName();

        string walletString = Configuration.GetWallet();
        UserWalletText.text = walletString;
        profileImage.sprite = SpriteManager.Instance.profile_image;

        var url = Configuration.BaseSocketUrl;
        Debug.Log("URL+ " + url);
        Manager = new SocketManager(new Uri(url));
        var customNamespace = Manager.GetSocket(CustomNamespace);
        customNamespace.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        customNamespace.On(SocketIOEventTypes.Disconnect, OnDisconnected);
        customNamespace.On<string>("leave-table", OnLeaveTableResponse);
        customNamespace.On<string>("table-users", OnGetTableResponse);
        customNamespace.On<string>("start-game", OnStartGameResponse);
        customNamespace.On<string>("trigger", OnTriggerResponse);
        customNamespace.On<string>("pack-game", OnPackResponse);
        customNamespace.On<string>("chaal", OnChaalResponse);
        customNamespace.On<string>("poker_timer", OnTimerResponse);
        Manager.Open();

        musicToggle.isOn = Configuration.GetMusic() == "on";
        soundToggle.isOn = Configuration.GetSound() == "on";

        // Add listeners for toggle changes
        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
    }
    private void OnMusicToggleChanged(bool isOn)
    {
        Debug.Log($"Music: {isOn}");

        PlayerPrefs.SetString("music", isOn ? "on" : "");
        PlayerPrefs.Save();

        if (isOn)
        {
            AudioManager._instance.PlayBackgroundAudio();
        }
        else
        {
            AudioManager._instance.StopBackgroundAudio();
        }
    }

    private void OnSoundToggleChanged(bool isOn)
    {
        Debug.Log($"Sound: {isOn}");

        PlayerPrefs.SetString("sound", isOn ? "on" : "");
        PlayerPrefs.Save();
        if (!isOn)
            AudioManager._instance.StopEffect();
    }

    #region data over buttons

    public void showamountovercall()
    {
        string amount = FormatNumber(gamedata.table_amount.ToString());

        calltext.text = "CALL " + amount;
    }

    public void showamountoverraise()
    {
        string doubleamount = (float.Parse(gamedata.table_amount.ToString()) + float.Parse(gamedata.table_amount.ToString())).ToString();

        Debug.Log("RES_Check + doubleamount + " + doubleamount);

        string amount = FormatNumber(doubleamount);

        raisetext.text = "RAISE " + amount;
        raisetext2.text = "RAISE";
    }
    #endregion

    #region timer

    private void OnTimerResponse(string args)
    {
        Debug.Log("RES_Check + socket timer Json :" + args);
        chaaltimer = int.Parse(args);
    }

    #endregion

    #region Connect/Disconnect

    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Res_Check + Connected + " + resp.sid);
        GetTable();
    }

    private void OnDisconnected()
    {
        Debug.Log("RES_Check + disconnected");
    }

    #endregion

    #region Join Table

    private void GetTable()
    {
        PData.Blind1_Value = PlayerPrefs.GetString("PokerDataTablePerClick");
        var customNamespace = Manager.GetSocket(CustomNamespace);
        Debug.Log("RES_Check + Call Get Table");
        PokerTableData jsonData = new PokerTableData();
        try
        {
            jsonData.user_id = Configuration.GetId();

            jsonData.token = Configuration.GetToken();

            jsonData.blind_1 = PData.Blind1_Value;
            //jsonData.blind_1 = PlayerPrefs.GetString("PokerDataTablePerClick");

            string jsonStr = JsonUtility.ToJson(jsonData);

            customNamespace.Emit("table-users", jsonData);

            Debug.Log("RES_CHECK + Emit-table-users  : DATA : " + PData.Blind1_Value);
            Debug.Log("RES_VALUE" + " EMIT-table-users" + jsonStr);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void OnGetTableResponse(string args)
    {
        Debug.Log("Res_Check on table-users Json :" + args);
        try
        {
            jointable = JsonUtility.FromJson<PokerJoinTableResponse>(args);
            if (jointable.code == 406)
            {
                showtoastmessage(jointable.message);
                Invoke(nameof(home), 3);
                return;
            }

            TableID = jointable.table_data[0].poker_table_id;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public void home()
    {
        if (Leavetable) return;
        Manager.Close();
        var customNamespaceSocket = Manager.GetSocket(CustomNamespace);
        customNamespaceSocket.Disconnect();
        Leavetable = true;
        //SceneManager.LoadSceneAsync("OPTHomePage");
        SceneLoader.Instance.LoadDynamicScene("HomePage.unity");
    }

    public void showtoastmessage(string message)
    {
        Toast.Show(message, 3f, ToastColor.Green);
    }

    #endregion

    #region start game

    IEnumerator StartGameDelay()
    {
        yield return new WaitForSeconds(2);
        waitingtext.SetActive(false);
        waittext.gameObject.SetActive(true);

        for (int i = 5; i > 0; i--)
        {
            waittext.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        StartGame();

        for (int i = 0; i < profiles.Count; i++)
        {
            profiles[i].transform.GetChild(0).GetChild(0).GetChild(3).gameObject.SetActive(true);
        }
    }

    private void StartGame()
    {
        gamestart = true;
        var customNamespace = Manager.GetSocket(CustomNamespace);

        PokerStartGame jsonData = new PokerStartGame();
        try
        {
            jsonData.user_id = Configuration.GetId();

            jsonData.token = Configuration.GetToken();

            jsonData.blind_1 = PData.Blind1_Value;
            //  jsonData.blind_1 = PlayerPrefs.GetString("PokerDataTablePerClick"); ;


            jsonData.table_id = TableID;

            string jsonStr = JsonUtility.ToJson(jsonData);
            customNamespace.Emit("start-game", jsonData);

            Debug.Log("RES_CHECK + Emit-start-game");
            Debug.Log("RES_VALUE" + " EMIT-start-game" + jsonStr);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void OnStartGameResponse(string args)
    {
        Debug.Log("Start Game Json :" + args);
    }

    #endregion

    #region call check raise

    public void Call()
    {
        var customNamespace = Manager.GetSocket(CustomNamespace);

        PokerUserChaalData jsonData = new PokerUserChaalData();
        try
        {
            jsonData.user_id = Configuration.GetId();

            jsonData.token = Configuration.GetToken();

            jsonData.plus = "0";

            jsonData.rule = role.ToString();

            jsonData.value = mycardvalues.Sum().ToString();

            jsonData.game_id = gameID;

            jsonData.chaal_type = "2";

            jsonData.amount = "0";

            string jsonStr = JsonUtility.ToJson(jsonData);

            customNamespace.Emit("chaal", jsonData);
            profiles[0].GetComponent<PokerChaalSlider>().ischaal = false;
            Debug.Log("RES_CHECK + Emit-Chaal");
            Debug.Log("RES_VALUE" + "Emit-chaal " + jsonStr);

            raiseclicked = 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public void Check()
    {
        var customNamespace = Manager.GetSocket(CustomNamespace);

        PokerUserChaalData jsonData = new PokerUserChaalData();
        try
        {
            jsonData.user_id = Configuration.GetId();

            jsonData.token = Configuration.GetToken();

            jsonData.plus = "0";

            jsonData.rule = role.ToString();

            jsonData.value = mycardvalues.Sum().ToString();

            jsonData.game_id = gameID;

            jsonData.chaal_type = "1";

            jsonData.amount = "0";

            string jsonStr = JsonUtility.ToJson(jsonData);

            customNamespace.Emit("chaal", jsonData);
            profiles[0].GetComponent<PokerChaalSlider>().ischaal = false;
            Debug.Log("RES_CHECK + Emit-Chaal");
            Debug.Log("RES_VALUE" + "Emit-chaal " + jsonStr);

            raiseclicked = 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public void Raise()
    {
        amount = float.Parse(gamedata.game_amount) + float.Parse(gamedata.game_amount);

        var customNamespace = Manager.GetSocket(CustomNamespace);

        PokerUserChaalData jsonData = new PokerUserChaalData();
        try
        {
            jsonData.user_id = Configuration.GetId();

            jsonData.token = Configuration.GetToken();

            jsonData.plus = "0";

            jsonData.rule = role.ToString();

            jsonData.value = mycardvalues.Sum().ToString();

            jsonData.game_id = gameID;

            jsonData.chaal_type = "3";

            jsonData.amount = amount.ToString();

            string jsonStr = JsonUtility.ToJson(jsonData);

            customNamespace.Emit("chaal", jsonData);
            profiles[0].GetComponent<PokerChaalSlider>().ischaal = false;
            Debug.Log("RES_CHECK + Emit-Chaal");
            Debug.Log("RES_VALUE" + "Emit-chaal " + jsonStr);

            raiseclicked = 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public void SliderRaise(float value)
    {
        float percentage = value / 100f;
        float newAmount = float.Parse(gamedata.game_users[0].total_amount) * (1 + percentage);
        Debug.Log("RES_Check + table amount + " + gamedata.game_users[0].total_amount);
        if (slidertext != null)
        {
            slidertext.text = newAmount.ToString("F2");
            slidettext2.text = newAmount.ToString("F2");
        }

        slideramount = newAmount;
    }

    public void sliderminamount(float value)
    {
        float percentage = value / 100f;
        float newAmount = float.Parse(gamedata.game_users[0].total_amount) * (1 + percentage);
        if (slidertext != null)
        {
            slidertext.text = newAmount.ToString("F2");
            slidettext2.text = newAmount.ToString("F2");
        }
        slideramount = newAmount;
    }

    public void SliderRaiseButton()
    {
        var customNamespace = Manager.GetSocket(CustomNamespace);

        PokerUserChaalData jsonData = new PokerUserChaalData();
        try
        {
            jsonData.user_id = Configuration.GetId();

            jsonData.token = Configuration.GetToken();

            jsonData.plus = "0";

            jsonData.rule = role.ToString();

            jsonData.value = mycardvalues.Sum().ToString();

            jsonData.game_id = gameID;

            jsonData.chaal_type = "3";

            jsonData.amount = slideramount.ToString();

            string jsonStr = JsonUtility.ToJson(jsonData);

            customNamespace.Emit("chaal", jsonData);
            profiles[0].GetComponent<PokerChaalSlider>().ischaal = false;
            raiseSlider.value = 0;
            raiseSlider2.value = 0;
            Debug.Log("RES_CHECK + Emit-Chaal");
            Debug.Log("RES_VALUE" + "Emit-chaal " + jsonStr);

            raiseclicked = 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void OnChaalResponse(string args)
    {
        Debug.Log("Chaal Json :" + args);
        //try
        //{
        //    gameresponse = JsonUtility.FromJson<GameResponse>(args);
        //}
        //catch (System.Exception ex)
        //{
        //    Debug.LogError(ex.ToString());
        //}
    }

    #endregion

    #region trigger

    private void OnTriggerResponse(string args)
    {
        Debug.Log("RES_Check + trigger " + args);
        if (args == "call_status")
        {
            API_CALL_status();
        }
        else
        {
            gameID = args;
            Debug.Log("Game ID : " + args);
            API_CALL_status();
        }
    }

    public void API_CALL_status()
    {
        StartCoroutine(Request());
    }

    IEnumerator Request()
    {
        string url = Configuration.Poker_Status;
        WWWForm form = new WWWForm();
        form.AddField("user_id", Configuration.GetId());
        if (TableID != null)
            form.AddField("poker_table_id", TableID);
        else
            form.AddField("poker_table_id", "");

        if (gameID != null)
        {
            form.AddField("game_id", gameID);
        }
        else
            form.AddField("game_id", "");

        form.AddField("token", Configuration.GetToken());

        Debug.Log("RES_Check + call request trigger  + gameid: " + gameID + " tableid: " + TableID + " user_id: + " + Configuration.GetId()
         + " Token: " + Configuration.GetToken());

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            request.SetRequestHeader("Token", Configuration.TokenLoginHeader);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                Debug.Log("RES_Value + CallStatus: " + request.downloadHandler.text);
                gamedata = JsonUtility.FromJson<PokerGameData>(request.downloadHandler.text);

                addactivetableusers();

                #region check and start game
                if (!gamestart)
                {
                    if (playersactiveontable.Count > 1)
                    {
                        StartCoroutine(StartGameDelay());
                    }
                    else
                        waitingtext.SetActive(true);
                }
                #endregion


                if (gamestart)
                {
                    if (!isstarted)
                    {
                        waittext.gameObject.SetActive(false);
                        animateCards.enabled = true;
                        isstarted = true;
                    }
                    totalbet.SetActive(true);

                    showmiddlecards();

                    evaluatemycards();

                    string totalbetnum = FormatNumber(gamedata.game_amount);

                    totalbet.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Total Bet: " + totalbetnum;

                    #region display cards

                    showmycards();

                    #endregion

                    #region show slider for chaal and buttons

                    chaalID = gamedata.chaal;

                    foreach (GameObject chaaluser in profiles)
                    {
                        if (chaalID == chaaluser.GetComponent<PokerChaalSlider>().id)
                        {
                            showamountovercall();
                            showamountoverraise();
                            chaaluser.GetComponent<PokerChaalSlider>().ischaal = true;
                        }
                        else
                        {
                            chaaluser.GetComponent<PokerChaalSlider>().ischaal = false;
                        }
                    }

                    if (chaalID == Configuration.GetId())
                    {
                        if (gamedata.check == 0)
                            buttonspanel.SetActive(true);
                        else
                            buttonspanel.SetActive(false);
                        if (gamedata.check == 1)
                            buttonspanelcheck.SetActive(true);
                        else
                            buttonspanelcheck.SetActive(false);
                    }
                    else
                    {
                        buttonspanel.SetActive(false);
                        buttonspanelcheck.SetActive(false);
                    }
                    #endregion

                    #region if game ended

                    if (gamedata.game_status == 2 && !gameended)
                    {
                        gameended = true;
                        for (int i = 0; i < gamedata.game_users.Count; i++)
                        {
                            ShowCards(gamedata.game_users[i]);
                        }
                        StartCoroutine(showwinner());
                    }

                    #endregion
                }
            }
        }
    }

    public void showmiddlecards()
    {
        for (int i = 0; i < gamedata.middle_card.Count; i++)
        {
            if (gamedata.middle_card.Count == 3 && !middle3anim)
            {
                StartCoroutine(animateCards.MoveAllCards(animateCards.middlecards, animateCards.middlecardsendPositions));
                middlecard1.sprite = FindSpriteByName(gamedata.middle_card[0].card);
                middlecard2.sprite = FindSpriteByName(gamedata.middle_card[1].card);
                middlecard3.sprite = FindSpriteByName(gamedata.middle_card[2].card);
                middle3anim = true;
            }
            else if (gamedata.middle_card.Count == 4 && !middle4anim)
            {
                StartCoroutine(animateCards.MoveAllCards(animateCards.fourthcard, animateCards.fourthcardendPositions));
                middlecard4.sprite = FindSpriteByName(gamedata.middle_card[0].card);
                middle4anim = true;
            }
            else if (gamedata.middle_card.Count == 5 && !middle5anim)
            {
                StartCoroutine(animateCards.MoveAllCards(animateCards.fifthcard, animateCards.fifthendPositions));
                middlecard5.sprite = FindSpriteByName(gamedata.middle_card[0].card);
                middle5anim = true;
            }
        }
    }

    public void addactivetableusers()
    {
        List<PokerGameDataTableUser> user = new List<PokerGameDataTableUser>();

        foreach (PokerGameDataTableUser tableuser in gamedata.table_users)
        {
            if (tableuser.user_id == Configuration.GetId())
                user.Add(tableuser);
        }

        if (user.Count == 0)
        {
            // SceneManager.LoadSceneAsync("OPTHomePage");
            if (Leavetable) return;
            SceneLoader.Instance.LoadDynamicScene("HomePage.unity");
        }

        playersactiveontable.Clear();
        for (int i = 0; i < gamedata.table_users.Length; i++)
        {
            if (gamedata.table_users[i].id != "0")
            {
                playersactiveontable.Add(gamedata.table_users[i]);
            }
        }

        for (int i = 0; i < profiles.Count; i++)
        {
            profiles[i].transform.GetChild(0).gameObject.SetActive(false);
            profiles[i].transform.GetChild(1).gameObject.SetActive(true);
        }

        showTableUsers();

        for (int i = 0; i < gamedata.game_users.Count; i++)
        {
            foreach (GameObject obj in profiles)
            {
                if (obj.GetComponent<PokerChaalSlider>().id == gamedata.game_users[i].user_id)
                    obj.transform.GetChild(0).GetChild(0).GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = gamedata.game_users[i].total_amount;
            }
        }
    }

    public void showTableUsers()
    {
        Debug.Log("Res_Check" + " showTableUsers");

        int index = playersactiveontable.FindIndex(x => x.user_id == Configuration.GetId());

        if (index > 0)
        {
            PokerGameDataTableUser userData = playersactiveontable[index];
            playersactiveontable.RemoveAt(index);
            playersactiveontable.Insert(0, userData);
        }

        for (int i = 0; i < playersactiveontable.Count; i++)
        {
            profiles[i].transform.GetChild(0).gameObject.SetActive(true);
            profiles[i].transform.GetChild(1).gameObject.SetActive(false);
            profiles[i].GetComponent<PokerChaalSlider>().id = playersactiveontable[i].user_id;
            profiles[i].transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = playersactiveontable[i].name;
            profiles[i].transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = FormatNumber(playersactiveontable[i].wallet);
            Image img = profiles[i].transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>();
            StartCoroutine(DownloadImage(playersactiveontable[i].profile_pic, img));
        }
    }

    #endregion

    #region my cards

    public void showmycards()
    {
        mycard1.sprite = FindSpriteByName(gamedata.cards[0].card1);
        mycard2.sprite = FindSpriteByName(gamedata.cards[0].card2);
    }

    public Sprite FindSpriteByName(string nameToFind)
    {
        foreach (Sprite go in deck)
        {
            if (go.name.ToLower() == nameToFind.ToLower())
            {
                Debug.Log("RES_Check + go + " + go.name.ToLower());
                return go;
            }
        }
        Debug.Log("RES_Check + null " + nameToFind.ToLower());
        return null;
    }

    #endregion

    #region Declare Winner

    public void ShowCards(PokerGameDataGameUser gameuser)
    {
        for (int i = 0; i < profiles.Count; i++)
        {
            if (profiles[i].GetComponent<PokerChaalSlider>().id == gameuser.user_id)
            {
                profiles[i].GetComponent<PokerChaalSlider>().card1.sprite = FindSpriteByName(gameuser.card1);
                profiles[i].GetComponent<PokerChaalSlider>().card2.sprite = FindSpriteByName(gameuser.card2);
            }
        }
    }

    public IEnumerator showwinner()
    {
        GameObject profile = new GameObject();

        for (int i = 0; i < profiles.Count; i++)
        {
            if (profiles[i].GetComponent<PokerChaalSlider>().id == gamedata.winner_user_id)
            {
                profile = profiles[i];
            }
        }

        if (Configuration.GetId() == gamedata.winner_user_id)
        {
            // newAudioManager.PlayClip(newAudioManager.roundWinsoundclip);
        }
        else
        {
            //  newAudioManager.PlayClip(newAudioManager.roundLostsoundclip);
        }

        profile.transform.GetChild(0).GetChild(5).gameObject.SetActive(true);

        yield return new WaitForSeconds(2);

        profile.transform.GetChild(0).GetChild(5).gameObject.SetActive(false);

        ResetEverythingAndStart();
    }

    #endregion

    #region Reset to Start

    public void ResetEverythingAndStart()
    {
        typetext.text = string.Empty;
        gamestart = false;
        gameended = false;
        totalbet.SetActive(false);
        animateCards.enabled = false;
        isstarted = false;
        middle3anim = false;
        middle4anim = false;
        middle5anim = false;
        for (int i = 0; i < profiles.Count; i++)
        {
            profiles[i].GetComponent<PokerChaalSlider>().card1.transform.localScale = Vector3.zero;
            profiles[i].GetComponent<PokerChaalSlider>().card2.transform.localScale = Vector3.zero;
        }

        for (int i = 0; i < profiles.Count; i++)
        {
            profiles[i].transform.GetChild(0).GetChild(0).GetChild(3).gameObject.SetActive(false);
        }

        middlecard1.transform.localScale = Vector3.zero;
        middlecard2.transform.localScale = Vector3.zero;
        middlecard3.transform.localScale = Vector3.zero;
        middlecard4.transform.localScale = Vector3.zero;
        middlecard5.transform.localScale = Vector3.zero;

        if (!gamestart)
        {
            if (playersactiveontable.Count > 1)
            {
                StartCoroutine(StartGameDelay());
            }
            else
                waitingtext.SetActive(true);
        }
    }

    #endregion

    #region Pack Game

    public void Emit_Pack()
    {
        var customNamespace = Manager.GetSocket(CustomNamespace);

        PokerPackData jsonData = new PokerPackData();
        try
        {
            jsonData.user_id = Configuration.GetId();
            jsonData.token = Configuration.GetToken();
            jsonData.timeout = "0";

            string jsonStr = JsonUtility.ToJson(jsonData);
            customNamespace.Emit("pack-game", jsonData);
            Debug.Log("RES_Check + Emit-pack-game");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void OnPackResponse(string args)
    {
        Debug.Log("RES_Check + pack-game Json :" + args);
        try
        {

        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    #endregion

    #region Leave Table

    public void CALL_Leave_table()
    {
        var customNamespace = Manager.GetSocket(CustomNamespace);
        PokerTableData jsonData = new PokerTableData();
        try
        {
            jsonData.user_id = Configuration.GetId();
            jsonData.token = Configuration.GetToken();

            string jsonStr = JsonUtility.ToJson(jsonData);
            customNamespace.Emit("leave-table", jsonData);
            Debug.Log("RES_Check + Emit-Leave-table :" + jsonStr);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private bool Leavetable = false;
    private void OnLeaveTableResponse(string args)
    {
        Debug.Log("Res_Check onleavetable :" + args);
        if (Leavetable) return;
        try
        {
            leavetable = null;
            leavetable = JsonUtility.FromJson<PokerLeaveTableResponse>(args);

            DOVirtual.DelayedCall(.3f, () =>
                 {
                     this.GetComponent<GameSelection>().loaddynamicscenebyname("HomePage.unity");
                     Leavetable = true;
                 });
            // if (leavetable.code == 200)
            // {
            //     //SceneManager.LoadScene("OPTHomePage");
            //     //SceneManager.LoadSceneAsync("OPTHomePage");

            // }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    #endregion

    #region Format number and download images 

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
        return number.ToString();
        // if (float.Parse(number) >= 1000 && float.Parse(number) < 10000)
        // {
        //     return (float.Parse(number) / 1000f).ToString("0.0") + "k";
        // }
        // else if (float.Parse(number) >= 10000)
        // {
        //     return (float.Parse(number) / 1000).ToString("0.#") + "k";
        // }
        // else
        // {
        //     return number.ToString();
        // }
    }

    #endregion

    #region Toast Message

    #endregion

    #region My card type

    public void evaluatemycards()
    {
        getmyallcards();
        role = Evaluatecard(evaluatecards);
    }

    public void getmyallcards()
    {
        evaluatecards.Clear();

        string card1 = RemoveUnderscores(gamedata.cards[0].card1);
        evaluatecards.Add(card1.ToLower());
        string card2 = RemoveUnderscores(gamedata.cards[0].card2);
        evaluatecards.Add(card2.ToLower());
        if (gamedata.middle_card.Count > 0)
        {
            for (int i = 0; i < gamedata.middle_card.Count; i++)
            {
                string middlecard = RemoveUnderscores(gamedata.middle_card[i].card);
                evaluatecards.Add(middlecard.ToLower());
            }
        }
    }

    public string RemoveUnderscores(string input)
    {
        if (input.Contains("_"))
        {
            return input.Replace("_", "");
        }
        else
        {
            return input;
        }
    }

    private static readonly Dictionary<string, int> cardValues = new Dictionary<string, int>
    {
        {"a", 14}, {"k", 13}, {"q", 12}, {"j", 11}, {"10", 10}, {"9", 9}, {"8", 8},
        {"7", 7}, {"6", 6}, {"5", 5}, {"4", 4}, {"3", 3}, {"2", 2}
    };


    public int Evaluatecard(List<string> cards)
    {
        mycardvalues.Clear();
        mycardsuits.Clear();

        if (cards.Count == 2)
        {
            return EvaluatePartialCardGroup(cards);
        }

        int besttype = Configuration.HIGH_CARD;

        besttype = EvaluateFiveCard(cards);

        return besttype;
    }

    public int EvaluateFiveCard(List<string> cards)
    {
        foreach (string card in cards)
        {
            int cardValue = GetCardValue(card);
            mycardvalues.Add(cardValue);
        }

        foreach (string card in cards)
        {
            string cardSuits = GetCardSuit(card);
            mycardsuits.Add(cardSuits);
        }

        mycardvalues.Sort();

        bool royalflush = RoyalFlush();
        if (royalflush)
            return Configuration.ROYAL_FLUSH;

        bool straightflush = false;
        if (flush(mycardsuits) && IsInSequence())
            straightflush = true;
        if (straightflush)
        {
            typetext.text = "STRAIGHT FLUSH";
            return Configuration.STRAIGHT_FLUSH;
        }

        bool fourofakind = Getfourofakind();
        if (fourofakind)
        {
            typetext.text = "FOUR OF A KIND";
            return Configuration.FOUR_OF_A_KIND;
        }

        bool fullhouse = false;
        if (GetThreeOfKind() && GetPAIR())
            fullhouse = true;
        if (fullhouse)
        {
            typetext.text = "FULL HOUSE";
            return Configuration.FULL_HOUSE;
        }

        bool Flush = flush(mycardsuits);
        if (Flush)
        {
            typetext.text = "FLUSH";
            return Configuration.FLUSH;
        }

        bool straight = IsInSequence();
        if (straight)
        {
            typetext.text = "STRAIGHT";
            return Configuration.STRAIGHT;
        }

        bool TwoPair = HasTwoPairs(mycardvalues);
        if (TwoPair)
        {
            typetext.text = "TWO PAIR";
            return Configuration.TWO_PAIR;
        }

        bool threeofkind = GetThreeOfKind();
        if (threeofkind)
        {
            typetext.text = "THREE OF KIND";
            return Configuration.THREE_OF_KIND;
        }

        bool Pair = GetPAIR();
        if (Pair)
        {
            typetext.text = "PAIR";
            return Configuration.PAIR;
        }

        typetext.text = "HIGH CARD";
        return Configuration.HIGH_CARD;
    }

    public bool RoyalFlush()
    {
        mycardvalues.Sort();

        if (flush(mycardsuits) && IsInSequence())
        {
            if (mycardvalues[mycardvalues.Count - 1] == 14)
                return true;
            else
                return false;
        }

        return false;
    }

    public bool flush(List<string> cardsuits)
    {
        string suit = cardsuits[0].Substring(0, Mathf.Min(2, cardsuits[0].Length));

        foreach (string str in cardsuits)
        {
            if (str.Substring(0, 2) != suit)
            {
                return false;
            }
        }

        return true;
    }

    public bool HasTwoPairs(List<int> numbers)
    {
        numbers.Sort();

        int pairCount = 0;

        for (int i = 0; i < numbers.Count - 1; i++)
        {
            if (numbers[i] == numbers[i + 1])
            {
                pairCount++;
                i++;
            }
        }
        return pairCount >= 2;
    }

    public bool IsInSequence()
    {
        mycardvalues.Sort();

        for (int i = 1; i < mycardvalues.Count; i++)
        {
            if (mycardvalues[i] != mycardvalues[i - 1] + 1)
            {
                return false;
            }
        }

        return true;
    }

    public bool GetPAIR()
    {
        HashSet<int> uniqueElements = new HashSet<int>();
        HashSet<int> duplicateElements = new HashSet<int>();

        foreach (int num in mycardvalues)
        {
            if (!uniqueElements.Add(num))
            {
                duplicateElements.Add(num);
            }
        }

        if (duplicateElements.Count > 0)
        {
            Debug.Log("RES_Check Duplicates found:");
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool GetThreeOfKind()
    {

        bool hasThreeTimes = false;

        foreach (int num in mycardvalues)
        {
            int count = 0;
            foreach (int value in mycardvalues)
            {
                if (value == num)
                {
                    count++;
                }
            }
            if (count == 3)
            {
                hasThreeTimes = true;
                break;
            }
        }

        Debug.Log("ReS_Check + paircount + " + hasThreeTimes);

        if (hasThreeTimes)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Getfourofakind()
    {

        bool hasFourTimes = false;

        foreach (int num in mycardvalues)
        {
            int count = 0;
            foreach (int value in mycardvalues)
            {
                if (value == num)
                {
                    count++;
                }
            }
            if (count == 4)
            {
                hasFourTimes = true;
                break;
            }
        }

        Debug.Log("ReS_Check + paircount + " + hasFourTimes);

        if (hasFourTimes)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private int EvaluatePartialCardGroup(List<string> cards)
    {
        List<int> values = new List<int>();

        foreach (string card in cards)
        {
            int cardValue = GetCardValue(card);

            values.Add(cardValue);
        }

        if (values[0] == values[1])
        {
            typetext.text = "PAIR";
            return Configuration.PAIR;
        }
        typetext.text = "HIGH CARD";
        return Configuration.HIGH_CARD;
    }

    private static int GetCardValue(string card)
    {
        string rank = card.Substring(2);
        if (cardValues.ContainsKey(rank))
        {
            return cardValues[rank];
        }
        throw new ArgumentException($"Invalid card rank: {rank}");
    }

    private static string GetCardSuit(string card)
    {
        string firstTwoLetters = card.Substring(0, Mathf.Min(2, card.Length));
        return firstTwoLetters;
    }

    #endregion
}
