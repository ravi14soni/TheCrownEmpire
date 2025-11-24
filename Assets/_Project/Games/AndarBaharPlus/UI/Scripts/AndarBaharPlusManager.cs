using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Best.SocketIO;
using Best.SocketIO.Events;
using DG.Tweening;
using EasyUI.Toast;
using Mkey;
using Newtonsoft.Json;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AndarBaharPlusManager : MonoBehaviour
{
    private string CustomNamespace = "/ander_bahar_plus";
    private SocketManager Manager;
    public TimerFillController timerFillController;
    public GameObject showwaitanim;
    public TextMeshProUGUI timer_text;
    public Text UserWallet;
    public Text UserWinAmount;
    public Root abp;
    public List<Sprite> all_cards;
    public List<Image> history_images;
    public List<string> last_history;
    public CardsAnimate middle_card;
    public CardsAnimate andar_animate_card;
    public CardsAnimate bahar_animate_card;
    public Animator modelAnimation;
    public bool middle_card_animate;
    private int card_highlight;
    public List<Image> highlight_card;
    public List<Button> coin_btn;
    public List<GameObject> m_coins = new List<GameObject>();
    public List<Button> m_BetButton = new List<Button>();
    public List<GameObject> m_AllHightlights = new List<GameObject>();
    public List<BoxCollider2D> m_BoxCollider2D = new List<BoxCollider2D>();
    public List<Animator> m_buttonAnimator = new List<Animator>();

    public Toggle soundToggle;
    public Toggle musicToggle;
    private bool StartGame = false;

    public GameObject showstop;
    public Text stoptext;

    private void OnEnable()
    {
        float startingTime = 15f;
        timerFillController.SetTimer(startingTime);
        var url = Configuration.BaseSocketUrl;
        Debug.Log("RES_CHECK Socket URL Andar bahar+ " + url);
        Manager = new SocketManager(new Uri(url));
        var customNamespace = Manager.GetSocket(CustomNamespace);
        customNamespace.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        customNamespace.On<string>("ander_bahar_plus_timer", Onandar_bahar_plus_timerResponse);
        customNamespace.On<string>("ander_bahar_plus_status", Onander_bahar_plus_statusResponse);
        Manager.Open();

        m_BetButton.ForEach(button =>
        {
            var buttonName = button.gameObject.name; // Capture the name in a local variable
            button.onClick.AddListener(() => OnBetButtonClick(buttonName, button));
        });

        coin_btn.ForEach(button =>
        {
            var buttonName = button.gameObject.name;
            button.onClick.AddListener(() => OnCoinSelected(buttonName, button));
        });


        OnCoinSelected("10", coin_btn[0]);

        UserWallet.text = Configuration.GetWallet();
        if (Configuration.GetMusic() == "on")
        {
            musicToggle.isOn = false;
        }
        else
        {
            musicToggle.isOn = true;
        }
        if (Configuration.GetSound() == "on")
        {
            soundToggle.isOn = false;
        }
        else
        {
            soundToggle.isOn = true;
        }
    }

    void Start()
    {
        // musicToggle.onValueChanged.AddListener(NewAudioManager.instance.ToggleMusic);
        soundToggle.onValueChanged.AddListener(changesound);
    }

    public void changesound(bool ison)
    {
        if (ison)
        {
            PlayerPrefs.SetString("sound", "");
        }
        else
        {
            PlayerPrefs.SetString("sound", "on");
        }
    }

    private string betamount;
    public Transform m_SpawnInitialPosition;

    private void OnCoinSelected(string buttonName, Button button)
    {
        if (!StartGame)
            return;
        HandleButtonClick(button);
        betamount = buttonName;
        Debug.Log("Coin Selected::" + betamount + " == Button Name == " + buttonName);
        m_SpawnInitialPosition = button.transform;

    }

    private int buttonIndex;

    public void ButtonSelect(int ba)
    {
        m_buttonAnimator.ForEach(x => x.enabled = false);
        m_buttonAnimator[ba].enabled = true;
        buttonIndex = ba;
    }

    private string bet;
    private List<GameObject> m_DummyObjects = new List<GameObject>();

    private void OnBetButtonClick(string buttonName, Button button)
    {
        if (!StartGame)
            return;
        GetCardValue(buttonName);
        //NewAudioManager.instance.PlayClip(NewAudioManager.instance.coinsoundclip);
        AudioManager._instance.PlayCoinDrop();
        var collider = button.GetComponentInChildren<BoxCollider2D>();
        Debug.Log($"Index Number::{GetCardValue(buttonName)}" + "Clicked Button Name::" + buttonName);
        bet = GetCardValue(buttonName).ToString();

        var coin = Instantiate(m_coins[buttonIndex], m_SpawnInitialPosition);
        coin.transform.localScale = Vector3.one;
        /*   var poolManager = coins[num].GetComponent<ObjectPoolUtil>();
          var coin = poolManager.GetObject(); */

        /*  coin.GetComponent<GetCoinBack>().enabled = false;
         coin.GetComponent<PlaceCoin>().enabled = false; */
        coin.transform.localPosition = Vector3.zero;

        m_DummyObjects.Add(coin.gameObject);
        coin.transform.SetParent(collider.transform);
        coin.transform.localRotation = Quaternion.identity;
        coin.transform.localScale = new Vector3(24f, 24f, 24f);
        coin.transform.DOLocalMove(GetRandomPositionInCollider(collider), 0.8f)
            .OnComplete(() =>
            {
                coin.transform.DOScale(Vector3.one * 23f, 0.2f);
            });
        StartCoroutine(PlaceBet());
    }

    public IEnumerator PlaceBet()
    {
        string url = Configuration.BaseUrl + "/api/AnderBaharPlus/place_bet";
        WWWForm form = new WWWForm();

        form.AddField("user_id", Configuration.GetId());
        form.AddField("token", Configuration.GetToken());
        form.AddField("game_id", abp.game_data[0].id);
        form.AddField("bet", bet);
        form.AddField("amount", betamount);
        Debug.Log(Configuration.GetId() + " PutBet ID Data");
        Debug.Log(Configuration.GetToken() + " PutBet Token Data");
        Debug.Log(abp.game_data[0].id + " PutBet GameID Data");
        Debug.Log(bet + " PutBet Bet Data");
        Debug.Log(betamount + " PutBet Betamount Data");

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
                Debug.Log("RES_CHECK PutBet: " + request.downloadHandler.text);
                JsonResponse jsonResponse = JsonUtility.FromJson<JsonResponse>(
                    request.downloadHandler.text
                );
                if (jsonResponse.code == 406)
                {
                    showtoastmessage(jsonResponse.message);
                }
                else
                {
                    string walletString = jsonResponse.wallet;
                    //string walletString = Configuration.GetWallet();
                    if (decimal.TryParse(walletString, out decimal userCoins))
                    {
                        PlayerPrefs.SetString("wallet", userCoins.ToString("F2"));
                        UserWallet.text = PlayerPrefs.GetString("wallet");
                        PlayerPrefs.Save();
                    }
                }
            }
        }
    }

    /*    if (jsonResponse.code == 200)
       {
           if (bet == "1")
           {
               baharamountint += int.Parse(betamount);
               baharamount.text = baharamountint.ToString() + " / " + ABdata.bahar_bet;
           }
           else
           {
               andaramountint += int.Parse(betamount);
               andaramount.text = andaramountint.ToString() + " / " + ABdata.ander_bet;
           }
       } */
    // var formData = new Dictionary<string, string>
    // {
    //     { "user_id", Configuration.GetId() },
    //     { "token", Configuration.GetToken() },
    //     { "game_id", abp.game_data[0].id },
    //     { "bet", bet },
    //     { "amount", betamount },
    // };
    // JsonResponse jsonResponse = new JsonResponse();
    // jsonResponse = await APIManager.Instance.Post<JsonResponse>(url, formData);

    // if (jsonResponse.code == 406)
    // {
    //     showtoastmessage(jsonResponse.message);
    // }
    // else
    // {
    //     string walletString = jsonResponse.wallet;
    //     PlayerPrefs.SetString("wallet", jsonResponse.wallet);
    //     UserWallet.text = CommonUtil.GetFormattedWallet();
    //     if (jsonResponse.code == 200)
    //     {
    //         /*  if (bet == "1")
    //          {
    //              baharamountint += int.Parse(betamount);
    //              baharamount.text = baharamountint.ToString() + " / " + ABdata.bahar_bet;
    //          }
    //          else
    //          {
    //              andaramountint += int.Parse(betamount);
    //              andaramount.text = andaramountint.ToString() + " / " + ABdata.ander_bet;
    //          } */
    //     }
    //     //UpdateButtonInteractability(Configuration.GetWallet());
    // }
    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("RES_CHECK On - Connected + " + resp.sid);
        GetTimer();
    }

    #region Handle Selected Buttons
    public void HandleButtonClick(Button btn)
    {
        foreach (var buttons in coin_btn)
        {
            if (buttons != btn)
            {
                btn.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                btn.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            }
        }
    }
    #endregion
    #region Timer
    private void GetTimer()
    {
        var customNamespace = Manager.GetSocket(CustomNamespace);
        try
        {
            customNamespace.Emit("ander_bahar_plus_timer", "ander_bahar_plus_timer");

            Debug.Log("RES_CHECK" + " EMIT-ander_bahar_plus_timer ");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void Onandar_bahar_plus_timerResponse(string args)
    {
        //   Debug.Log("RES_CHECK Timmer:" + args);
        Stopshowwait();
        timerFillController.SetTimer(float.Parse(args));
        timer_text.text = args;
        UserWinAmount.text = "0.00";
    }

    public void Stopshowwait()
    {
        if (!StartGame)
        {
            Debug.Log("RES_Check + Entered Stopshowwait");
            showwaitanim.SetActive(false);
            StartGame = true;
            OnCoinSelected("10", coin_btn[0]);
            stoptext.text = "PLACE BET";
            StartCoroutine(Showplacebet());
        }
    }
    IEnumerator Showplacebet()
    {
        showstop.SetActive(true);
        yield return new WaitForSeconds(2f);
        showstop.SetActive(false);
    }

    #endregion
    #region Status
    bool belPlacePopUp = false;
    private void Onander_bahar_plus_statusResponse(string args)
    {
        Debug.Log("Json :" + args);
        try
        {
            abp = JsonUtility.FromJson<Root>(args);
            if (!middle_card_animate && abp.game_data[0].main_card != string.Empty)
            {
                StartGame = true;
                Debug.Log("Main Card:" + abp.game_data[0].main_card);
                middle_card.main_card = GetCard(abp.game_data[0].main_card);
                middle_card_animate = true;
                middle_card.StartSlotAnimation();
                //modelAnimation.gameObject.SetActive(true);
                Invoke(nameof(AnimateCards), 2f);
            }
            if (abp.game_data[0].status == "1")
            {
                StartGame = false;
                timerFillController.SetTimer(0);
                UserWinAmount.text = "0.00";
                timer_text.text = "0";
                stoptext.text = "STOP BET";
                belPlacePopUp = false;
                showstop.SetActive(true);
                Invoke(nameof(StopBet), 2f);
                Invoke(nameof(ShowCardWinner), 4f);

            }
            if (abp.game_data[0].status == "0")
            {
                //PlaceBetPopup();

                StartGame = true;
            }
            ShowLast10Win();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    #endregion

    void StopBet() => showstop.SetActive(false);

    public void PlaceBetPopup()
    {
        if (belPlacePopUp) return;
        belPlacePopUp = true;
        OnCoinSelected("10", coin_btn[0]);
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
    #region Get One Card

    public Sprite GetCard(string card_name)
    {
        foreach (Sprite sprite in all_cards)
        {
            if (card_name.ToLower() == sprite.name)
            {
                return sprite;
            }
        }
        return null;
    }

    #endregion
    #region Assign Andar and Bahar Card

    public void AnimateCards()
    {
        modelAnimation.gameObject.SetActive(true);
        andar_animate_card.main_card = GetCard(
                   abp.game_cards[abp.game_cards.Count - 2].card.ToLower()
               );
        andar_animate_card.SetMiddleSprite();
        Invoke(nameof(SetAndarBaharCard), 2f);
    }

    void SetAndarBaharCard()
    {
        bahar_animate_card.main_card = GetCard(
            abp.game_cards[abp.game_cards.Count - 1].card.ToLower()
        );
        bahar_animate_card.SetMiddleSprite();
        modelAnimation.gameObject.SetActive(false);
    }

    #endregion
    #region Show Winner
    public void ShowCardWinner()
    {
        middle_card_animate = false;
        string winner_card = abp.game_cards[abp.game_cards.Count - 1].card.ToLower();
        Debug.Log("winner_card:" + winner_card);
        string suffix = winner_card.ToLower().Substring(2).ToLower();
        //Debug.Log("suffix CardValue:" + suffix);
        var card = highlight_card.Find(x => x.name == suffix).transform.GetChild(0).gameObject;
        List<string> winner = new List<string>();
        winner.Clear();
        winner.Add(abp.game_data[0].winning);
        winner.Add(abp.game_data[0].winning_red_black);
        winner.Add(abp.game_data[0].winning_shape);
        winner.Add(abp.game_data[0].winning_ak);
        winner.Add(abp.game_data[0].winning_up_down);

        Debug.Log("Winner:" + winner.Count);

        List<GameObject> matchingHighlights = m_AllHightlights
            .Where(highlight => winner.Contains(highlight.name))
            .ToList();

        Debug.Log("MatchingHighlights: Count" + matchingHighlights.Count);

        for (int i = 0; i < matchingHighlights.Count; i++)
        {
            StartCoroutine(BlinkImage(matchingHighlights[i]));
        }
        StartCoroutine(BlinkImage(card));
    }

    public Vector3 GetRandomPositionInCollider(Collider2D collider2D)
    {
        // Get the bounds of the BoxCollider2D
        Bounds bounds = collider2D.bounds;

        // Generate random X and Y positions within the BoxCollider2D bounds
        float randomX = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
        float randomY = UnityEngine.Random.Range(bounds.min.y, bounds.max.y);

        // Convert world position to local position relative to the canvas
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            collider2D.transform.GetComponent<RectTransform>(),
            new Vector2(randomX, randomY),
            null,
            out localPoint
        );

        return localPoint; // Return local position in Canvas space
    }

    IEnumerator BlinkImage(GameObject target_image)
    {
        int blinkCount = 0;

        while (blinkCount < 6)
        {
            target_image.SetActive(true);
            yield return new WaitForSeconds(0.4f);

            target_image.SetActive(false);
            yield return new WaitForSeconds(0.4f);
            blinkCount++;
        }
        if (blinkCount == 6)
        {
            StartCoroutine(nameof(GetResult));
            showwaitanim.SetActive(true);
            m_DummyObjects.ForEach(x => Destroy(x));
            m_DummyObjects.Clear();
        }
    }

    public IEnumerator GetResult()
    {
        string Url = Configuration.BaseUrl + "/api/AnderBaharPlus/get_result";

        Debug.Log("RES_Check + API-Call + Andar bahar Result");
        WWWForm form = new WWWForm();
        form.AddField("user_id", Configuration.GetId());
        form.AddField("token", Configuration.GetToken());
        form.AddField("game_id", abp.game_data[0].id);
        UnityWebRequest www = UnityWebRequest.Post(Url, form);
        www.SetRequestHeader("Token", Configuration.TokenLoginHeader);
        Debug.Log(
            "RES_Check + userid + "
                + Configuration.GetId()
                + " token "
                + Configuration.GetToken()
                + " "
                + " gameid "
                + abp.game_data[0].id
        );
        Debug.Log("RES_Check + URL + " + Url);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            var responseText = www.downloadHandler.text;
            Debug.Log("Res_Value + Result: " + responseText);
            var abresultdata = JsonConvert.DeserializeObject<AndarBaharBetResult>(responseText);
            if (abresultdata.code == 101)
            {
                yield return null;
            }
            else if (abresultdata.code == 102)
            {
                /* gameaudio.clip = winaudio;
                gameaudio.Play(); */
                AudioManager._instance.PlayWinSound();
                //NewAudioManager.instance.PlayWin();
                UserWinAmount.text = abresultdata.win_amount.ToString();
                showtoastmessage("congratulation!! You Won " + abresultdata.win_amount);
            }
            else if (abresultdata.code == 103)
            {
                if (abresultdata.win_amount > 0)
                {
                    //NewAudioManager.instance.PlayWin();
                    UserWinAmount.text = abresultdata.win_amount.ToString();
                    AudioManager._instance.PlayWinSound();

                    showtoastmessage("congratulation!! You Won " + abresultdata.win_amount);
                }
                else
                {
                    //NewAudioManager.instance.Playlose();
                    UserWinAmount.text = "";
                    AudioManager._instance.PlayLoseSound();
                    showtoastmessage("Better Luck Next Time, You Lost " + abresultdata.diff_amount);
                }
            }
            DOVirtual.DelayedCall(.5f, () =>
            {
                Configuration.GetProfileWallet();
                UserWallet.text = PlayerPrefs.GetString("wallet");

            });
        }
    }

    /* Debug.Log("RES_Check + API-Call + Result");

    var formData = new Dictionary<string, string>
    {
        { "user_id", Configuration.GetId() },
        { "token", Configuration.GetToken() },
        { "game_id", abp.game_data[0].id },
    };
    Debug.Log(
            "RES_Check + userid + "
                + Configuration.GetId()
                + " token "
                + Configuration.GetToken()
                + " "
                + " gameid "
                + abp.game_data[0].id
        );
    // AndarBaharBetResult abresultdata = new AndarBaharBetResult();
    var abresultdata = await APIManager.Instance.Post<AndarBaharBetResult>(Url, formData);

    Debug.Log("Result Message" + abresultdata.message);

    if (abresultdata.code == 102)
    {
        //   AudioManager._instance.PlayWinSound();
        showtoastmessage("congratulation!! You Won " + abresultdata.win_amount);
    }
    else if (abresultdata.code == 103)
    {
        if (abresultdata.win_amount > 0)
        {
            //  AudioManager._instance.PlayWinSound();
            showtoastmessage("congratulation!! You Won " + abresultdata.win_amount);
        }
        else
        {
            //   AudioManager._instance.PlayLoseSound();
            showtoastmessage("Better Luck Next Time, You Lost " + abresultdata.diff_amount);
        }
    }
    else
    {
        ///showtoastmessage(abresultdata.message);
    } */
    public void showtoastmessage(string message)
    {
        //StartCoroutine(showtoastimage(message));
        Toast.Show(message, 3f);
    }

    public int GetCardValue(string card)
    {
        //string suffix = card.Substring(2).ToLower();
        // Debug.Log("CheckSuffix" + suffix);
        // Try to parse a number

        Debug.Log("ClickObjectName::" + card);
        if (int.TryParse(card, out int number))
        {
            return number; // Return the number if it's a valid integer
        }
        else
        {
            // If it's a letter, return the corresponding value
            switch (card)
            {
                case "spade":
                    return 18;
                case "heart":
                    return 17;
                case "club":
                    return 20;
                case "diamond":
                    return 19;
                case "black-cards":
                    return 16;
                case "red-cards":
                    return 15;
                case "8tok":
                    return 23;
                case "7ranks":
                    return 22;
                case "ato6":
                    return 21;
                case "bahar-btn":
                    return 1;
                case "anadr-btn":
                    return 0; // Ace
                case "a":
                    return 14; // Ace
                case "j":
                    return 11; // Jack
                case "q":
                    return 12; // Queen
                case "k":
                    return 13; // King
                default:
                    throw new ArgumentException("Invalid card value");
            }
        }
    }

    #endregion

    #region Show Last Winning

    public void ShowLast10Win()
    {
        last_history.Clear();
        for (int i = 0; i < abp.last_winning.Count; i++)
        {
            last_history.Add(abp.last_winning[i].main_card.ToLower());
        }

        for (int i = 0; i < history_images.Count; i++)
        {
            foreach (Sprite sprite in all_cards)
            {
                if (last_history[i] == sprite.name)
                {
                    history_images[i].sprite = sprite;
                }
            }
        }
    }

    #endregion

    public void Disconnect()
    {
        Manager.Close();
        var customNamespaceSocket = Manager.GetSocket(CustomNamespace);
        customNamespaceSocket.Disconnect();
        SceneLoader.Instance.LoadDynamicScene("HomePage.unity");
        //SceneManager.LoadSceneAsync("OPTHomePage");
    }
}
