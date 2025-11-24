using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AndroApps;
using Best.SocketIO;
using Best.SocketIO.Events;
using DG.Tweening;
using EasyUI.Toast;
using Mkey;
using Newtonsoft.Json;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AndarBaharManager : MonoBehaviour
{
    private string CustomNamespace = "/ander_bahar";

    [Header("last winning Sprite Data")]
    public List<string> lastwinning;
    public List<Sprite> lastwinningsprite;
    public List<GameObject> lastwinningimagestoshow;
    private SocketManager Manager;

    public RootObject ABdata;
    public List<GameObject> cards;
    public List<GameObject> allcards;
    public TextMeshProUGUI timertext;
    public SpriteRenderer maincard;
    public SpriteRenderer maincard2;
    public bool start;
    public GameObject userprofile;
    public List<GameObject> profiles;
    public List<BotsUser> m_Bots = new List<BotsUser>();
    public string betamount;
    public string bet;
    public List<GameObject> btns;
    public GameObject buttonclicked;
    public List<GameObject> coins;
    public int num;
    public Image andar,
        bahar;
    public int totalcoinsleft,
        randomecount,
        left1,
        left2;
    public List<Sprite> profileimages;
    public GameObject Reconn_Canvas;
    public newLogInOutputs LogInOutput;
    public TextMeshProUGUI andaramount,
        baharamount;
    public int andaramountint,
        baharamountint;
    public bool check,
        showrecord,
        online;
    public GameObject bl2;
    public Animator onlineuser;
    public AndarBaharBetResult abresultdata;
    public GameObject historyprediction;
    public List<string> lastwinningprediction;
    public List<GameObject> lastwinningpredictionimagestoshow;
    public float andarint,
        baharint;
    public TextMeshProUGUI andaramounttext,
        baharamounttext,
        andarpredictiontext,
        baharpredictiontext;
    public Slider abpredictionslider;
    public bool reconnected,
        invoke;
    public int timetoinvoke;

    #region music and sounds

    public Button[] buttons;

    #endregion
    public Toggle soundToggle;
    public Toggle musicToggle;

    public GameObject showstop;
    private bool gamestart;
    public Text stoptext;
    public Image UserProfilePic;
    public Text UserWalletText;
    public Text UserNameText;

    public Transform startPosition;
    public Transform[] endPositions;
    public List<Transform> cardsRotateList;
    private int poolSize = 50;
    // Map bet amount (e.g., 50, 100) to coin prefab index to keep UI coin consistent with amount
    private readonly Dictionary<int, int> amountToCoinIndex = new Dictionary<int, int>();
    private int lastSelectedAmount = -1;
    private int lastSelectedCoinIndex = -1;
    #region Partical Syatems
    public ParticleSystem[] particleSystems;
    private ParticleSystem currentParticle;
    private bool showresult;

    public TextMeshProUGUI TotalBetText;
    #endregion

    public WinLosePopup winLosePopup;

    private void OnEnable()
    {
        GameBetUtil.initialScale = Vector3.one * 0.4f;
        GameBetUtil.targetScale = Vector3.one * 0.3f;
        UserNameText.text = Configuration.GetName();
        string walletString = Configuration.GetWallet();

        UserWalletText.text = CommonUtil.GetFormattedWallet();
        UserProfilePic.sprite = SpriteManager.Instance?.profile_image;

        var url = Configuration.BaseSocketUrl;
        CommonUtil.CheckLog("RES_CHECK Socket URL Andar bahar+ " + url);
        Manager = new SocketManager(new Uri(url));
        var customNamespace = Manager.GetSocket(CustomNamespace);
        customNamespace.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        customNamespace.On(SocketIOEventTypes.Disconnect, OnDisconnected);
        customNamespace.On<string>("ander_bahar_timer", Onander_bahar_timerResponse);
        customNamespace.On<string>("ander_bahar_status", Onander_bahar_statusResponse);
        customNamespace.On<string>("leave-table", leave);
        Manager.Open();

        musicToggle.isOn = Configuration.GetMusic() == "on";
        soundToggle.isOn = Configuration.GetSound() == "on";

        // Add listeners for toggle changes
        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        showresult = false;
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

    void Start()
    {
        foreach (var coin in coins)
        {
            var pool = coin.AddComponent<ObjectPoolUtil>();
            pool.InitializePool(coin, 10);
        }
        GameBetUtil.OnButtonClickParticle(num, particleSystems.ToList(), ref currentParticle);
        GameBetUtil.UpdateButtonInteractability(
            Configuration.GetWallet(),
            buttons.ToList(),
            particleSystems.ToList(),
            ref buttonclicked,
            ref currentParticle,
            gamestart,
            buttonclick,
            clickedbutton,
            coininstantate,
            ref betamount
        );
    }

    #region  minimize

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            CommonUtil.CheckLog("RES_Check + resume");
            RequestGameStateUpdate();
        }
        else
        {
            //StopCoroutine(aibet());
            // StopCoroutine(GameBetUtil.OnlineBet(coins, m_ColliderList, onlineuser, ABdata.game_data[0].status, timertext.text, m_DummyObjects));
        }
    }

    private void RequestGameStateUpdate()
    {
        invoke = false;
        var url = Configuration.BaseSocketUrl;
        CommonUtil.CheckLog("URL+ " + url);
        Manager = new SocketManager(new Uri(url));
        var customNamespace = Manager.GetSocket(CustomNamespace);
        customNamespace.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        customNamespace.On(SocketIOEventTypes.Disconnect, OnDisconnected);
        customNamespace.On<string>("ander_bahar_timer", Onander_bahar_timerResponse);
        customNamespace.On<string>("ander_bahar_status", Onander_bahar_statusResponse);
        customNamespace.On<string>("leave-table", leave);
        Manager.Open();
        GetTimer();
        reconnected = true;
        timetoinvoke = 0;
        StartCoroutine(invokefortime());
    }

    IEnumerator invokefortime()
    {
        for (int i = 0; i < 2; i++)
        {
            timetoinvoke++;
            yield return new WaitForSeconds(1);
        }

        invoke = true;
    }

    private void ResetAmounts(bool isSameGame)
    {
        if (isSameGame)
        {
            andaramount.text = andaramountint.ToString();
            baharamount.text = baharamountint.ToString();
        }
        else
        {
            andaramountint = baharamountint = 0;
            andaramount.text = andaramountint.ToString();
            baharamount.text = baharamountint.ToString();
            for (int i = 0; i < m_colidertext.Count; i++)
            {
                m_colidertext[i].text = "0";
            }
        }
    }

    private void ClearGameObjects()
    {
        foreach (GameObject coin in m_DummyObjects)
        {
            Destroy(coin);
        }
        m_DummyObjects.Clear();
    }

    private void ClearObjectList(List<GameObject> objectList)
    {
        foreach (GameObject obj in objectList)
        {
            Destroy(obj);
        }
        objectList.Clear();
    }

    #endregion

    void OnConnected(ConnectResponse resp)
    {
        invoke = true;
        CommonUtil.CheckLog("RES_CHECK On - Connected + " + resp.sid);
        GetTimer();
    }

    public void showtoastmessage(string message)
    {
        Toast.Show(message, 3f);
    }

    #region Connection/Disconnection Socket

    private void leave(string args)
    {
        CommonUtil.CheckLog("get-table Json :" + args);
        try { }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public void disconn()
    {
        //CALL_Leave_table();
        SceneManager.LoadSceneAsync("Main");
    }

    public void CALL_Leave_table()
    {
        StopAllCoroutines();
    }

    public void OnDisconnected()
    {
        AudioManager._instance.StopEffect();
        //AudioManager._instance.StopAudio();
        //Reconn_Canvas.SetActive(true);
    }

    public void Disconnect()
    {
        Manager.Close();
        var customNamespaceSocket = Manager.GetSocket(CustomNamespace);
        customNamespaceSocket.Disconnect();
        AudioManager._instance.StopEffect();
        SceneLoader.Instance.LoadDynamicScene("HomePage.unity");
        //LoaderUtil.instance.LoadScene("HomePage");
    }

    #endregion

    private List<GameObject> m_DummyObjects = new List<GameObject>();
    public List<Collider2D> m_ColliderList = new List<Collider2D>();
    public List<TextMeshProUGUI> m_colidertext = new List<TextMeshProUGUI> { };

    #region status related functions
    public void ShowLast10Win()
    {
        lastwinning.Clear();
        for (int i = 0; i < ABdata.last_winning.Length; i++)
        {
            lastwinning.Add(ABdata.last_winning[i].winning);
        }

        for (int i = 0; i < lastwinning.Count; i++)
        {
            //CommonUtil.CheckLog("RES_Check + Last winning");
            if (i < lastwinningimagestoshow.Count)
            {
                if (lastwinning[i] == "0")
                    lastwinningimagestoshow[i].transform.GetComponent<Image>().sprite =
                        lastwinningsprite[0];
                else if (lastwinning[i] == "1")
                    lastwinningimagestoshow[i].transform.GetComponent<Image>().sprite =
                        lastwinningsprite[1];
            }
        }
    }

    public async void updateprofile()
    {
        if (!check)
        {
            UserNameText.text = Configuration.GetName();
            UserWalletText.text = CommonUtil.GetFormattedWallet();
            check = true;
        }

        for (int i = 0; i < m_Bots.Count; i++)
        {
            m_Bots[i].BotName.text = ABdata.bot_user[i].name;
            //m_Bots[i].BotCoin.text = ABdata.bot_user[i].coin;
            m_Bots[i].ProfileImage.sprite = await ImageUtil.Instance.GetSpriteFromURLAsync(
                Configuration.ProfileImage + ABdata.bot_user[i].avatar
            );
        }

        /* for (int i = 0; i < profiles.Count; i++)
        {
            profiles[i].transform.gameObject.SetActive(true);
            //CommonUtil.CheckLog("Res_Check 1");
            profiles[i].transform.GetChild(0).gameObject.SetActive(true);
            profiles[i].transform.GetChild(2).gameObject.SetActive(false);


            profiles[i].transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().text = ABdata.bot_user[i].name;
            profiles[i].transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = FormatNumber(ABdata.bot_user[i].coin);
            Image img = profiles[i].transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>();
            img.sprite = await ImageUtil.Instance.GetSpriteFromURLAsync(Configuration.ProfileImage + ABdata.bot_user[i].avatar);
            //StartCoroutine(DownloadImage(ABdata.bot_user[i].avatar, img));
        } */
    }

    #endregion

    #region ABHistoryPrediction

    public void ShowHistoryPrediction()
    {
        lastwinningprediction.Clear();
        andarint = 0;
        baharint = 0;
        for (int i = 0; i < ABdata.last_winning.Length; i++)
        {
            //lastwinningprediction.Add(ABdata.last_winning[i].winning);
            if (ABdata.last_winning[i].winning == "0")
            {
                lastwinningpredictionimagestoshow[i].transform.GetComponent<Image>().sprite =
                    lastwinningsprite[0];
                andarint++;
            }
            else if (ABdata.last_winning[i].winning == "1")
            {
                lastwinningpredictionimagestoshow[i].transform.GetComponent<Image>().sprite =
                    lastwinningsprite[1];
                baharint++;
            }
        }

        andaramounttext.text = andarint.ToString();
        baharamounttext.text = baharint.ToString();

        float value = (andarint / 20);
        float percentage = value * 100;

        CommonUtil.CheckLog("RES_Check + Andar amount " + andarint);
        CommonUtil.CheckLog("RES_Check + Andar value " + value);
        CommonUtil.CheckLog("RES_Check + Andar Percentage " + percentage);

        abpredictionslider.value = percentage;

        andarpredictiontext.text = percentage.ToString() + "%";
        baharpredictiontext.text = (100 - percentage).ToString() + "%";

        historyprediction.SetActive(true);
    }

    #endregion

    #region socket status
    private void Onander_bahar_statusResponse(string args)
    {
        CommonUtil.CheckLog("RES_CHECK Onander_bahar_statusResponse ");
        CommonUtil.CheckLog("RES_VALUE Status Game Json :" + args);

        try
        {
            ABdata = JsonUtility.FromJson<RootObject>(args);
            if (!showresult)
            {
                showresult = true;
                ShowLast10Win();
            }
            updateprofile();
            if (!start)
            {
                if (ABdata.game_data != null)
                {
                    Debug.Log("status" + ABdata.game_data[0].status);
                    foreach (var name in allcards)
                    {
                        if (name.name == ABdata.game_data[0].main_card.ToLower())
                        {
                            /*  gameaudio.clip = Patti;
                             gameaudio.Play();
  */
                            DOVirtual.DelayedCall(
                                .2f,
                                () =>
                                {
                                    AudioManager._instance.PlayCardFlipSound();
                                }
                            );
                            //StartCoroutine(CommonUtil.LoadAndPlayAudio(Patti, gameaudio));

                            //maincard.sprite = name;
                            maincard.sprite = name.GetComponent<SpriteRenderer>().sprite;
                            maincard.enabled = true;
                            start = true;
                        }
                    }
                }
                StartCoroutine(anim());
            }
            cards.Clear();
            //            Debug.LogError("STATUS RESPONSE:" + ABdata.game_data[0].status);
            if (ABdata.game_data[0].status == "0")
            {
                cardsRotateList.ForEach(x => x.gameObject.SetActive(false));
                cardsRotateList.ForEach(x => Destroy(x.gameObject));
                Debug.Log("status" + ABdata.game_data[0].status);
                if (!online)
                {
                    PlaceBetPopup();
                    online = true;
                    for (int i = 0; i < m_Bots.Count; i++)
                    {
                        m_Bots[i].NoUser.SetActive(false);
                        m_Bots[i].UserCome.SetActive(true);
                    }
                    DOVirtual.DelayedCall(
                        .2f,
                        () =>
                        {
                            StartCoroutine(
                                GameBetUtil.StartBet(
                                    coins,
                                    m_ColliderList,
                                    profiles,
                                    onlineuser,
                                    m_DummyObjects,
                                    m_colidertext,
                                    isAI: true
                                )
                            );
                            // StartCoroutine(
                            //     GameBetUtil.StartBet(
                            //         coins,
                            //         m_ColliderList,
                            //         profiles,
                            //         onlineuser,
                            //         m_DummyObjects,
                            //         m_colidertext,
                            //         isAI: false
                            //     )
                            // );
                        }
                    );
                }

                string game_id = Configuration.getabid();
                bool isSameGame = game_id == ABdata.game_data[0].id;

                if (reconnected)
                {
                    ResetAmounts(isSameGame);
                    ClearGameObjects();
                    reconnected = false;
                }
                else
                {
                    ResetAmounts(isSameGame);
                }
                PlayerPrefs.SetString("abid", ABdata.game_data[0].id);
            }
            else
            {
                Debug.Log("status" + ABdata.game_data[0].status);
                Debug.Log("STOP BET:");
                if (online)
                {
                    online = false;
                    GameBetUtil.StopBet();
                }
            }

            foreach (var name in allcards)
            {
                if (name.name == ABdata.game_data[0].main_card.ToLower())
                {
                    //maincard.sprite = name;
                    maincard.sprite = name.GetComponent<SpriteRenderer>().sprite;
                }
            }
            if (ABdata.game_cards.Length != 0)
            {
                for (int i = 0; i < ABdata.game_cards.Length; i++)
                {
                    string cardname = ABdata.game_cards[i].card.ToLower();
                    bool found = false;
                    if (cardname == "bl2")
                    {
                        // Debug.LogError("Added");
                        cards.Add(bl2);
                        found = true;
                    }
                    else
                    {
                        for (int j = 0; j < allcards.Count; j++)
                        {
                            if (allcards[j].name.ToLower() == cardname) // Comparing names in a case-insensitive manner
                            {
                                cards.Add(allcards[j]);
                                found = true; // Set flag to true if a matching card is found
                            }
                        }
                    }
                    if (!found) // Check if a matching card was not found
                        CommonUtil.CheckLog("RES_CHECK Card not found: " + cardname);
                }
                int num = 0;
                if (timertext.text == "1" && invoke)
                {
                    // CommonUtil.ShowToast(
                    //     "winning: "
                    //         + ABdata.game_data[0].winning
                    //         + " , Game Id: "
                    //         + ABdata.game_data[0].id
                    // );
                    timertext.text = "0";
                    onlineuser.enabled = false; //(onlineuser is Animator)
                    if (ABdata.game_data[0].status == "1")
                    {
                        for (int i = 0; i < cards.Count; i++)
                        {
                            StartCoroutine(changetext());
                            GameObject card = Instantiate(cards[i], endPositions[i]);
                            card.GetComponent<Animator>().enabled = false;
                            card.SetActive(false);
                            card.GetComponent<SpriteRenderer>().sortingOrder = num;
                            cardsRotateList.Add(card.transform);
                            num++;
                        }
                        StartCoroutine(gameover());
                        gamestart = false;
                        GameBetUtil.UpdateButtonInteractability(
                            Configuration.GetWallet(),
                            buttons.ToList(),
                            particleSystems.ToList(),
                            ref buttonclicked,
                            ref currentParticle,
                            gamestart,
                            buttonclick,
                            clickedbutton,
                            coininstantate,
                            ref betamount
                        );

                        start = false;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    IEnumerator gameover()
    {
        foreach (var obj in buttons)
        {
            obj.interactable = false;
        }
        AudioManager._instance.PlayStopBetSound();
        stoptext.text = "STOP BET";
        showstop.SetActive(true);
        yield return new WaitForSeconds(2);
        showstop.SetActive(false);

        //StartCoroutine(highlightwin());
        AndarBaharManager andarBaharManagerInstance = FindObjectOfType<AndarBaharManager>();
        StartCoroutine(
            CardUtil.MoveAllCards(
                cardsRotateList,
                endPositions,
                startPosition,
                andarBaharManagerInstance,
                andarBaharManagerInstance.highlightwin()
            )
        );
        //StartCoroutine(patti.MoveAllCards());
        //StartCoroutine(startcards());
    }

    public IEnumerator changetext()
    {
        //yield return new WaitForSeconds(1);
        timertext.text = "0";
        yield return null;
    }

    IEnumerator anim()
    {
        maincard.gameObject.SetActive(false);
        maincard2.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        maincard.gameObject.SetActive(true);
        maincard2.gameObject.SetActive(false);
    }
    #endregion

    #region Timer
    private void GetTimer()
    {
        var customNamespace = Manager.GetSocket(CustomNamespace);
        try
        {
            customNamespace.Emit("ander_bahar_timer", "ander_bahar_timer");

            CommonUtil.CheckLog("RES_CHECK" + " EMIT-ander_bahar_timer ");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void Onander_bahar_timerResponse(string args)
    {
        CommonUtil.CheckLog("RES_CHECK Timmer:" + args);
        Stopshowwait();
        try
        {
            timertext.text = args;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public void Stopshowwait()
    {
        if (!gamestart)
        {
            foreach (var btn in buttons)
            {
                btn.interactable = true;
            }
            //PlaceBetPopup();

            gamestart = true;
            GameBetUtil.UpdateButtonInteractability(
                Configuration.GetWallet(),
                buttons.ToList(),
                particleSystems.ToList(),
                ref buttonclicked,
                ref currentParticle,
                gamestart,
                buttonclick,
                clickedbutton,
                coininstantate,
                ref betamount
            );
            //UpdateButtonInteractability(Configuration.GetWallet());
        }
    }
    #endregion


    public void PlaceBetPopup()
    {
        stoptext.text = "PLACE BET";
        showstop.SetActive(true);
        AudioManager._instance.PlayPlaceBetSound();
        DOVirtual.DelayedCall(
            2f,
            () =>
            {
                showstop.SetActive(false);
            }
        );
    }

    #region win functions
    IEnumerator highlightwin()
    {
        updatedata();
        if (ABdata.game_data[0].winning == "0")
        {
            /*   gameaudio.clip = roundwinner;
              gameaudio.Play(); */
            //StartCoroutine(CommonUtil.LoadAndPlayAudio(roundwinner, gameaudio));
            AudioManager._instance.PlayHighlightWinSound();
            andar.color = Color.white;
            yield return new WaitForSeconds(0.5f);
            andar.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            andar.color = Color.white;
            yield return new WaitForSeconds(0.5f);
            andar.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            andar.color = Color.white;
            yield return new WaitForSeconds(0.5f);
            andar.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            andar.color = Color.white;
        }
        else
        {
            /* gameaudio.clip = roundwinner;
            gameaudio.Play(); */
            //StartCoroutine(CommonUtil.LoadAndPlayAudio(roundwinner, gameaudio));
            AudioManager._instance.PlayHighlightWinSound();
            bahar.color = Color.white;
            yield return new WaitForSeconds(0.5f);
            bahar.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            bahar.color = Color.white;
            yield return new WaitForSeconds(0.5f);
            bahar.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            bahar.color = Color.white;
            yield return new WaitForSeconds(0.5f);
            bahar.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            bahar.color = Color.white;
        }
        totalcoinsleft = 0;

        GetResult();
        //MoveAllcoinsintoTop();

        baharamount.text = "0";
        andaramount.text = "0";
        totalbet = 0;
        TotalBetText.text = "0";
        for (int i = 0; i < m_colidertext.Count; i++)
        {
            m_colidertext[i].text = "0";
        }
        showrecord = false;
        check = false;
        yield return new WaitForSeconds(2f);
        andaramountint = 0;
        baharamountint = 0;
        ShowLast10Win();
    }

    IEnumerator startcards()
    {
        yield return new WaitForSeconds(0);
        if (cardsRotateList.Count != 0)
        {
            CommonUtil.CheckLog("Enter 2");
            //foreach (Transform cards in patti.cards)
            //    patti.gameObject.GetComponent<ReversePatti>().cards.Add(cards);
            //StartCoroutine(patti.gameObject.GetComponent<ReversePatti>().MoveAllCards());
            //yield return new WaitForSeconds(2);

            cardsRotateList.ForEach(x => x.gameObject.SetActive(false));
            cardsRotateList.ForEach(x => Destroy(x.gameObject));
            cardsRotateList.Clear();

            foreach (GameObject coin in m_DummyObjects)
            {
                Destroy(coin);
            }
            m_DummyObjects.Clear();
        }
    }

    #endregion

    void OnDestroy()
    {
        if (cardsRotateList.Count != 0)
        {
            CommonUtil.CheckLog("Enter 2");
            cardsRotateList.ForEach(x => x.gameObject.SetActive(false));
            cardsRotateList.ForEach(x => Destroy(x.gameObject));
            cardsRotateList.Clear();

            foreach (GameObject coin in m_DummyObjects)
            {
                Destroy(coin);
            }
            m_DummyObjects.Clear();
        }
    }

    #region buttons_Functionality

    private int totalbet = 0;
    public async void PlaceBet()
    {
        string url = AndarBaharConfig.AndarBaharPutBet;

        CommonUtil.CheckLog("RES_Check + API-Call + PlaceBet" + ABdata.game_data[0].id + ":Betamount:" + betamount);

        var formData = new Dictionary<string, string>
        {
            { "user_id", Configuration.GetId() },
            { "token", Configuration.GetToken() },
            { "game_id", ABdata.game_data[0].id },
            { "bet", bet },
            { "amount", betamount },
        };
        string json = JsonConvert.SerializeObject(formData, Formatting.Indented);

        // Debug.Log("url:" + url);
        // Debug.Log("PUTBET: " + json);

        JsonResponse jsonResponse = new JsonResponse();
        jsonResponse = await APIManager.Instance.Post<JsonResponse>(url, formData);

        if (jsonResponse.code == 406 || jsonResponse.code == 402)
        {
            showtoastmessage(jsonResponse.message);
        }
        else
        {
            if (jsonResponse.code == 200)
            {

                totalbet += int.Parse(betamount);
                TotalBetText.text = totalbet.ToString();
                string walletString = jsonResponse.wallet;
                PlayerPrefs.SetString("wallet", jsonResponse.wallet);
                UserWalletText.text = CommonUtil.GetFormattedWallet();
                if (bet == "1")
                {
                    baharamountint += int.Parse(betamount);
                    //  m_colidertext[0].text = int.Parse(m_colidertext[0].text) + betamount + "";
                    baharamount.text = baharamountint.ToString();
                }
                else
                {
                    andaramountint += int.Parse(betamount);
                    //  m_colidertext[1].text = int.Parse(m_colidertext[1].text) + betamount + "";
                    andaramount.text = andaramountint.ToString();
                }
            }
            GameBetUtil.UpdateButtonInteractability(
                Configuration.GetWallet(),
                buttons.ToList(),
                particleSystems.ToList(),
                ref buttonclicked,
                ref currentParticle,
                gamestart,
                buttonclick,
                clickedbutton,
                coininstantate,
                ref betamount
            );
            //UpdateButtonInteractability(Configuration.GetWallet());
        }
    }
    public void Cancel_Bet()
    {
        if (ABdata.game_data[0].status == "0")
        {
            StartCoroutine(CancelBet());
        }
        else
        {
            showtoastmessage("Game not started");
        }
    }
    IEnumerator CancelBet()
    {
        string url = Configuration.AndarbaharCancelBet;
        WWWForm form = new WWWForm();

        form.AddField("user_id", Configuration.GetId());
        form.AddField("token", Configuration.GetToken());
        form.AddField("game_id", ABdata.game_data[0].id);
        Debug.Log("RES_Check + id: " + Configuration.GetId());
        Debug.Log("RES_Check + Token: " + Configuration.GetToken());
        Debug.Log("RES_Check + game_data_id: " + ABdata.game_data[0].id);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
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
                var betresp = JsonUtility.FromJson<RoulettePutBetResponse>(
                        request.downloadHandler.text
                    );
                if (betresp.code == 200)
                {
                    Debug.Log("RES_Check + Cancel + " + request.downloadHandler.text);
                    showtoastmessage(betresp.message);
                    string walletString = betresp.wallet;
                    //string walletString = Configuration.GetWallet();
                    if (double.TryParse(walletString, out double userCoins))
                    {
                        PlayerPrefs.SetString("wallet", userCoins.ToString("F2"));
                        PlayerPrefs.Save();
                        UserWalletText.text = Configuration.GetWallet();
                    }
                    andaramountint = baharamountint = 0;
                    baharamount.text = "0";
                    andaramount.text = "0";
                    totalbet = 0;
                    TotalBetText.text = totalbet.ToString();
                    //                    BetPool.Instance.Clear();
                }
                else
                {
                    showtoastmessage("No bet to cancel");
                }
            }
        }
    }
    public async void updatedata()
    {
        string url = Configuration.Url + Configuration.wallet;
        var formData = new Dictionary<string, string>
        {
            { "user_id", Configuration.GetId() },
            { "token", Configuration.GetToken() },
        };
        Wallet myResponse = await APIManager.Instance.Post<Wallet>(url, formData);
        if (myResponse.code == 200)
        {
            PlayerPrefs.SetString("wallet", myResponse.wallet);
            PlayerPrefs.Save();
            UserWalletText.text = CommonUtil.GetFormattedWallet();
        }
        else
        {
            CommonUtil.CheckLog("Error_new:" + myResponse.message);
        }
        /*       CommonUtil.CheckLog("RES+Message" + myResponse.message);
              CommonUtil.CheckLog("RES+Code" + myResponse.code); */
    }

    public void buttonclick(int num)
    {
        betamount = num.ToString();
        // If we already know which coin index matches this amount, sync selection
        if (amountToCoinIndex.TryGetValue(num, out var mappedIndex))
        {
            this.num = mappedIndex;
            lastSelectedAmount = num;
            lastSelectedCoinIndex = mappedIndex;
        }
    }

    public void clickedbutton(GameObject button)
    {
        foreach (GameObject buttons in btns)
        {
            buttons.transform.localScale = new Vector3(1, 1, 1);
        }
        button.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        buttonclicked = button;
    }

    public void coininstantate(int index)
    {
        num = index;
        // Learn the mapping between current amount and selected coin index
        if (int.TryParse(betamount, out var amount))
        {
            amountToCoinIndex[amount] = index;
            lastSelectedAmount = amount;
            lastSelectedCoinIndex = index;
        }
    }

    public void ClickedAndar()
    {
        CommonUtil.CheckLog("RES Check " + ABdata.game_data[0].status);
        if (betamount != "0")
        {
            if (ABdata.game_data[0].status == "0")
                {
                bet = "0";
                if (float.Parse(Configuration.GetWallet()) >= float.Parse(betamount))
                {
                    if (betamount != null)
                        PlaceBet();

                    var RandomCollider = m_ColliderList[0]; // means andar

                    /*   var poolManager = coins[num].GetComponent<ObjectPoolUtil>();
                      var coin = poolManager.GetObject(); */
                    // Resolve coin index based on current bet amount for consistent UI
                    int coinIndex = num;
                    if (int.TryParse(betamount, out var amountAndar))
                    {
                        if (amountToCoinIndex.TryGetValue(amountAndar, out var mappedIndexAndar))
                        {
                            coinIndex = mappedIndexAndar;
                        }
                        else if (lastSelectedAmount == amountAndar && lastSelectedCoinIndex >= 0)
                        {
                            coinIndex = lastSelectedCoinIndex;
                        }
                    }
                    var coin = Instantiate(
                        coins[coinIndex],
                        userprofile.transform.GetChild(0).GetChild(0).GetChild(2)
                    );

                    AudioManager._instance.PlayCoinDrop();
                    coin.transform.localPosition = Vector3.zero;

                    m_DummyObjects.Add(coin.gameObject);
                    coin.transform.SetParent(RandomCollider.transform);
                    coin.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    coin.transform.DOLocalMove(
                            GameBetUtil.GetRandomPositionInCollider(RandomCollider),
                            0.8f
                        )
                        .OnComplete(() =>
                        {
                            coin.transform.DOScale(Vector3.one * 0.3f, 0.2f);
                        });
                }
                else
                {
                    showtoastmessage("Insufficient Balance");
                }
                // newAudioManager.PlayClip(newAudioManager.coinsoundclip);
            }
        }
        else
        {
            showtoastmessage("Insufficient balance");
        }
    }

    public void ClickedBahar()
    {
        if (betamount != "0")
        {
            if (ABdata.game_data[0].status == "0")
                {
                bet = "1";

                if (float.Parse(Configuration.GetWallet()) > float.Parse(betamount))
                {
                    if (betamount != null)
                        PlaceBet();

                    var RandomCollider = m_ColliderList[1]; // means bahar
                    // Resolve coin index based on current bet amount for consistent UI
                    int coinIndex = num;
                    if (int.TryParse(betamount, out var amountBahar))
                    {
                        if (amountToCoinIndex.TryGetValue(amountBahar, out var mappedIndexBahar))
                        {
                            coinIndex = mappedIndexBahar;
                        }
                        else if (lastSelectedAmount == amountBahar && lastSelectedCoinIndex >= 0)
                        {
                            coinIndex = lastSelectedCoinIndex;
                        }
                    }
                    var coin = Instantiate(
                        coins[coinIndex],
                        userprofile.transform.GetChild(0).GetChild(0).GetChild(2)
                    );
                    /*    var poolManager = coins[num].GetComponent<ObjectPoolUtil>();
                       var coin = poolManager.GetObject(); */

                    coin.transform.localPosition = Vector3.zero;
                    AudioManager._instance.PlayCoinDrop();
                    m_DummyObjects.Add(coin.gameObject);
                    coin.transform.SetParent(RandomCollider.transform);
                    coin.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    coin.transform.DOLocalMove(
                            GameBetUtil.GetRandomPositionInCollider(RandomCollider),
                            0.8f
                        )
                        .OnComplete(() =>
                        {
                            coin.transform.DOScale(Vector3.one * 0.3f, 0.2f);
                        });
                }
                else
                {
                    showtoastmessage("Insufficient Balance");
                }
            }
        }
        else
        {
            showtoastmessage("Insufficient balance");
        }
    }
    #endregion

    #region result

    bool IsUserWin = false;

    public async void GetResult()
    {
        string Url = AndarBaharConfig.andarbaharResult;
        CommonUtil.CheckLog("RES_Check + API-Call + Result");

        var formData = new Dictionary<string, string>
        {
            { "user_id", Configuration.GetId() },
            { "token", Configuration.GetToken() },
            { "game_id", ABdata.game_data[0].id },
        };
        CommonUtil.CheckLog(
            "RES_Check + userid + "
                + Configuration.GetId()
                + " token "
                + Configuration.GetToken()
                + " "
                + " gameid "
                + ABdata.game_data[0].id
        );
        //AndarBaharBetResult andarbaharresult = new AndarBaharBetResult();
        abresultdata = await APIManager.Instance.Post<AndarBaharBetResult>(Url, formData);

        CommonUtil.CheckLog("Result Message" + abresultdata.message);

        if (abresultdata.code == 102)
        {
            AudioManager._instance.PlayWinSound();
            winLosePopup.gameObject.SetActive(true);
            if (abresultdata.win_amount > 0)
            {
                winLosePopup.SetText("Congratulation!! You Won : " + abresultdata.win_amount);
            }
            else
            {
                winLosePopup.SetText("You Lose, Try Again");
            }

            //showtoastmessage("congratulation!! You Won " + abresultdata.win_amount);
        }
        else if (abresultdata.code == 103)
        {
            if (abresultdata.win_amount > 0)
            {
                IsUserWin = true;
                AudioManager._instance.PlayWinSound();
                //  winLosePopup.gameObject.SetActive(true);
                //showtoastmessage("congratulation!! You Won " + abresultdata.win_amount);
                // if (abresultdata.win_amount > 0)
                // {
                //     winLosePopup.SetText("Congratulation!! You Won : " + abresultdata.win_amount);
                // }
                // else
                // {
                //     winLosePopup.SetText("You Lose, Try Again");
                // }
            }
            else
            {
                IsUserWin = false;
                AudioManager._instance.PlayLoseSound();
                //     winLosePopup.gameObject.SetActive(true);
                // if (abresultdata.win_amount > 0)
                // {
                //     winLosePopup.SetText("Congratulation!! You Won : " + abresultdata.win_amount);
                // }
                // else
                // {
                //     winLosePopup.SetText("You Lose, Try Again");
                // }
                //winLosePopup.SetText("Better Luck Next Time, You Lost : " + abresultdata.diff_amount);
                // showtoastmessage("Better Luck Next Time, You Lost " + abresultdata.diff_amount);
            }
        }
        else
        {
            ///showtoastmessage(abresultdata.message);
        }
        if (abresultdata.bet_amount > 0)
        {
            winLosePopup.gameObject.SetActive(true);
            winLosePopup.SetText(
      "Bet Amount: " + abresultdata.bet_amount + "\n" +
      "Win Amount: " + abresultdata.win_amount + "\n" +
      "Loss Amount: " + (abresultdata.diff_amount > 0 ? 0 : abresultdata.diff_amount)
  );
        }
        GameBetUtil.MoveAllCoinsIntoTop(
           m_DummyObjects,
           m_FirstMovePosition,
           m_ColliderList,
           ABdata.game_data[0].winning,
           andaramountint,
           baharamountint,
           userprofile,
           profiles,
           () =>
           {
               StartCoroutine(startcards());
           }, IsUserWin
       );
    }

    public Transform m_FirstMovePosition;

    #endregion
}
