using System;
using System.Collections;
using System.Collections.Generic;
using Best.SocketIO;
using Best.SocketIO.Events;
using DG.Tweening;
using Mkey;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SlotManager : MonoBehaviour
{
    private string CustomNamespace = "/slot_game";
    public bool IsConnection;
    private SocketManager Manager;
    public Text timertext;
    public Slot_RootObject slotData;
    public SlotControls slotControls;
    public SlotController slotController;
    public List<int> IntReelGrid = new List<int>();
    public Text UserWalletText;
    public Button BetButton;

    private Animator BetAnimator;

    public Toggle soundToggle;
    public Toggle musicToggle;
    public List<List<int>> paylines = new List<List<int>>();
    public List<GameObject> Lines = new List<GameObject>();
    private Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f); // The target scale
    private float duration = 0.5f; // Duration of one bounce
    private int loops = -1; // Number of loops (-1 for infinite)
    public LoopType loopType = LoopType.Yoyo;

    public Text WinWalletText;
    public Text stoptext;
    public GameObject showstop;
    Tween TimerTween;

    public GameObject WinPopup;
    public GameObject LosePopup;
    public Animator WinAnimation;
    public Text BetAmount;

    private bool IsBetPlaced = false;

    private void OnEnable()
    {
        var url = Configuration.BaseSocketUrl;
        Debug.Log("URL+ " + url);

        //UserWalletText.text = PlayerPrefs.GetString("wallet");
        Manager = new SocketManager(new Uri(url));
        var customNamespace = Manager.GetSocket(CustomNamespace);
        customNamespace.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        customNamespace.On(SocketIOEventTypes.Disconnect, OnDisconnected);
        customNamespace.On<string>("slot_game_timer", OnSlot_TimerResponse);
        customNamespace.On<string>("slot_game_status", OnSlot_StatusResponse);
        Manager.Open();

        BetButton.onClick.AddListener(PutBet);
        updatedata();

        musicToggle.isOn = Configuration.GetMusic() == "on";
        soundToggle.isOn = Configuration.GetSound() == "on";

        musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);

        TimerTween = timertext
            .transform.DOScale(targetScale, duration)
            .SetLoops(loops, loopType)
            .SetEase(Ease.InOutQuad);

        BetAnimator = BetButton.GetComponent<Animator>();
        WinAnimation.enabled = false;

        BetAmount.text = "";

        IsBetPlaced = true;
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

    bool online = false;

    private void OnSlot_StatusResponse(string args)
    {
        Debug.Log("JSON:" + args);
        slotData = JsonUtility.FromJson<Slot_RootObject>(args);
        //var slotData = JsonConvert.DeserializeObject<Slot_RootObject_ForPayline>(args);
        try
        {
            if (slotData.game_data[0].status == 0)
            {
                if (!online)
                {
                    //IsStopTimer = true;
                    WinPopup.SetActive(false);
                    LosePopup.SetActive(false);
                    // WinWalletText.text = "0";
                    TimerTween.Play();
                    online = true;
                    BetButton.interactable = true;
                    BetAmount.text = "";
                    BetButton.GetComponentInChildren<Text>().text = "BET";
                    BetAnimator.enabled = true;
                    WinAnimation.enabled = false;
                    Debug.Log($" Status::0{slotData.game_data[0].status}");
                    StartCoroutine(PlaceBetPopup());
                }
            }
            else //if status is 1
            {
                if (online)
                {
                    //StartCoroutine(StopBetPopup());
                    timertext.text = "0";
                    TimerTween.Pause();
                    AudioManager._instance?.PlaySlotReelSpin();
                    online = false;
                    IntReelGrid.Clear();
                    IntReelGrid.AddRange(
                        ConvertToIntegerList(slotData.game_data[0].reel_grid.ToArray())
                    );

                    //PopulatePaylines(5);
                    /*    if (IntReelGrid.Count != 15)
                       {
                           Debug.LogError("ReelGrid must have exactly 15 elements.");
                           return;
                       }
                       for (int i = 0; i < slotController.slotGroupsBeh.Length; i++)
                       {
                           int startIndex = i * 3;
                           if (slotController.slotGroupsBeh[i].symbOrder.Count < 3)
                           {
                               Debug.LogError($"SlotGroup {i} does not have enough space in SymbOrder.");
                               continue;
                           }
                           for (int j = 0; j < 3; j++)
                           {
                               slotController.slotGroupsBeh[i].symbOrder[j] = IntReelGrid[startIndex + j];
                           }
                           var temp = slotController.slotGroupsBeh[i].symbOrder[0];
                           slotController.slotGroupsBeh[i].symbOrder[0] = slotController.slotGroupsBeh[i].symbOrder[2]; // 3 -> 1
                           slotController.slotGroupsBeh[i].symbOrder[2] = temp;
                       } */
                    if (IntReelGrid.Count != 15)
                    {
                        Debug.LogError("ReelGrid must have exactly 15 elements.");
                        return;
                    }

                    int totalGroups = slotController.slotGroupsBeh.Length;
                    int symbolsPerGroup = 3;

                    // Ensure each group can hold the required number of symbols
                    for (int i = 0; i < totalGroups; i++)
                    {
                        if (slotController.slotGroupsBeh[i].symbOrder.Count < symbolsPerGroup)
                        {
                            Debug.LogError(
                                $"SlotGroup {i} does not have enough space in SymbOrder."
                            );
                            return;
                        }
                    }

                    // Populate data horizontally
                    for (int row = 0; row < symbolsPerGroup; row++)
                    {
                        for (int col = 0; col < totalGroups; col++)
                        {
                            int index = row * totalGroups + col;
                            if (index < IntReelGrid.Count)
                            {
                                slotController.slotGroupsBeh[col].symbOrder[row] = IntReelGrid[
                                    index
                                ];
                            }
                            var temp = slotController.slotGroupsBeh[col].symbOrder[0];
                            slotController.slotGroupsBeh[col].symbOrder[0] = slotController
                                .slotGroupsBeh[col]
                                .symbOrder[2]; // 3 -> 1
                            slotController.slotGroupsBeh[col].symbOrder[2] = temp;
                        }
                    }
                    if (IsBetPlaced)
                    {
                        WinWalletText.text = "0";
                        slotController.mainRotateTime = 5f;
                        BetAnimator.enabled = false;
                        slotControls.Spin_Click();
                    }
                    // var slotData_payline = JsonConvert.DeserializeObject<Slot_RootObject_ForPayline>(args);
                    /*  Debug.Log("slotData.paylines" + slotData_payline.paylines.Count);
                     Debug.Log($"Number of Paylines: {slotData_payline.paytable.Count}"); */
                    // Debug.Log("First Payline0: " + string.Join(",", slotData_payline.paylines[0]));
                    // Debug.Log("First Payline1: " + string.Join(",", slotData_payline.paylines[1]));
                    // Debug.Log("First Payline2: " + string.Join(",", slotData_payline.paylines[2]));
                    // Debug.Log("First Payline3: " + string.Join(",", slotData_payline.paylines[3]));
                    // Debug.Log("First Payline4: " + string.Join(",", slotData_payline.paylines[4]));

                    /*   for (int i = 0; i < paylines.Count; i++)
                      {
                          Debug.Log($"Payline {i}: {string.Join(",", paylines[i])}");
                      }
                      paylines.RemoveAt(0);
                      paylines.RemoveAt(1);

                      AddPaylinesToSlotController(paylines); */
                    //  AddPaylinesToSlotController(slotData_payline.paylines);
                    Lines.ForEach(line => line.SetActive(false));
                    DOVirtual.DelayedCall(
                        5f,
                        () =>
                        {
                            if (slotData.game_data[0].winnings.Count > 0)
                            {
                                AudioManager._instance?.PlayHighlightWinSound();
                            }
                            Debug.Log("Winning Counts::" + slotData.game_data[0].winnings.Count);
                            for (int i = 0; i < slotData.game_data[0].winnings.Count; i++)
                            {
                                var obj = Lines.Find(line =>
                                    line.name
                                    == slotData.game_data[0].winnings[i].payline.ToString()
                                );
                                StartCoroutine(HighlightWinner(obj));
                            }
                        }
                    );

                    DOVirtual.DelayedCall(5f, () => StartCoroutine(GetResult()));
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    IEnumerator HighlightWinner(GameObject obj)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(5f);
        obj.SetActive(false);
        /*  yield return new WaitForSeconds(0.5f);
         obj.SetActive(false);
         yield return new WaitForSeconds(0.5f);
         obj.SetActive(true);
         yield return new WaitForSeconds(0.5f);
         obj.SetActive(false);
         yield return new WaitForSeconds(0.5f);
         obj.SetActive(true);
         yield return new WaitForSeconds(0.5f);
         obj.SetActive(false);
         yield return new WaitForSeconds(0.5f);
         obj.SetActive(true);
         yield return new WaitForSeconds(2f); */
    }

    private int timecount;

    private void OnSlot_TimerResponse(string args)
    {
        /*      Debug.Log("TIMER::" + args);
             if (!IsStopTimer)
             {
                 timertext.text = "0";
                 return;
             } */
        //        Debug.Log("Timer Game Json :" + args);
        try
        {
            timertext.text = args;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void OnDisconnected()
    {
        Debug.Log($"Disconnected!!");
        Manager.Close();
    }

    public void OnDisconnect()
    {
        Debug.Log($"Disconnected!!");
        Manager.Close();
    }

    private void OnConnected(ConnectResponse response)
    {
        //        Debug.Log($"OnConnected!!{response.sid}");
    }

    public void PutBet()
    {
        timecount = int.Parse(timertext.text);

        Debug.Log("TIMER COUNT:" + timecount);
        slotController.mainRotateTime = (5 + timecount);
        StartCoroutine(PlaceBet());
    }

    public IEnumerator GetResult()
    {
        string url = SlotConfig.SlotGetResult;
        WWWForm form = new WWWForm();
        form.AddField("user_id", Configuration.GetId());
        form.AddField("token", Configuration.GetToken());
        form.AddField("game_id", slotData.game_data[0].id);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Token", Configuration.TokenLoginHeader);

        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            var responseText = www.downloadHandler.text;
            CommonUtil.CheckLog("+ SlotGetResult" + responseText);

            var jsonResponse = JsonUtility.FromJson<SlotGameResult>(responseText);
            Debug.Log("Wallet" + jsonResponse.wallet);

            if (jsonResponse.code == 102)
            {
                AudioManager._instance?.PlayWinSound();
                WinWalletText.text = jsonResponse.win_amount.ToString();
                WinAnimation.enabled = true;
                // CommonUtil.ShowToast("congratulation!! You Won " + jsonResponse.win_amount);
                showWinPopup(jsonResponse.win_amount.ToString());
            }
            else if (jsonResponse.code == 103)
            {
                if (jsonResponse.win_amount > 0)
                {
                    WinWalletText.text = jsonResponse.win_amount.ToString();
                    AudioManager._instance?.PlayWinSound();
                    WinAnimation.enabled = true;
                    // CommonUtil.ShowToast("congratulation!! You Won " + jsonResponse.win_amount);
                    showWinPopup(jsonResponse.win_amount.ToString());
                }
                else
                {
                    AudioManager._instance?.PlayLoseSound();
                    WinAnimation.enabled = false;
                    CommonUtil.ShowToast(
                        "Better Luck Next Time, You Lost " + jsonResponse.diff_amount
                    );
                    ShowLostPopup();
                }
            }
            if (float.TryParse(jsonResponse.wallet, out float value))
            {
                // Format the float to one decimal place
                string formattedValue = value.ToString("F1");
                PlayerPrefs.SetString("wallet", formattedValue);
                // Output to the Unity console
                // Debug.Log("Formatted String: " + formattedValue); // Output: Formatted String: 56769.4
            }
            IsBetPlaced = true;
            UserWalletText.text = PlayerPrefs.GetString("wallet");
        }
        /*  string Url = SlotConfig.SlotGetResult;
         CommonUtil.CheckLog("RES_Check + API-Call + Result");

         var formData = new Dictionary<string, string>
         {
             { "user_id", Configuration.GetId() },
             { "token", Configuration.GetToken() },
             { "game_id", slotData.game_data[0].id.ToString() },
         };
         CommonUtil.CheckLog(
             "RES_Check + userid + "
                 + Configuration.GetId()
                 + " token "
                 + Configuration.GetToken()
                 + " "
                 + " gameid "
                 + slotData.game_data[0].id
         );
         //AndarBaharBetResult andarbaharresult = new AndarBaharBetResult();
         SlotGameResult SlotGameResult = await APIManager.Instance.Post<SlotGameResult>(Url, formData);

         CommonUtil.CheckLog("Result Message" + SlotGameResult.message);

         if (SlotGameResult.code == 102)
         {
             AudioManager._instance.PlayWinSound();
             CommonUtil.ShowToast("congratulation!! You Won " + SlotGameResult.win_amount);
         }
         else if (SlotGameResult.code == 103)
         {
             if (SlotGameResult.win_amount > 0)
             {
                 AudioManager._instance.PlayWinSound();
                 CommonUtil.ShowToast("congratulation!! You Won " + SlotGameResult.win_amount);
             }
             else
             {
                 AudioManager._instance.PlayLoseSound();
                 CommonUtil.ShowToast("Better Luck Next Time, You Lost " + SlotGameResult.diff_amount);
             }
         } */
    }

    public void showWinPopup(string amount)
    {
        WinPopup.transform.localScale = Vector3.zero;
        WinPopup.gameObject.SetActive(true);
        WinPopup.GetComponentInChildren<Text>().text = amount;
        WinPopup.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack);
    }

    public void ShowLostPopup()
    {
        LosePopup.transform.localScale = Vector3.zero;
        LosePopup.gameObject.SetActive(true);
        LosePopup.transform.DOScale(Vector3.one, 0.8f);
    }

    public IEnumerator PlaceBet()
    {
        IsBetPlaced = false;
        string url = SlotConfig.SlotPlacebet;

        WWWForm form = new WWWForm();
        form.AddField("user_id", Configuration.GetId());
        form.AddField("token", Configuration.GetToken());
        form.AddField("game_id", slotData.game_data[0].id);
        form.AddField("amount", slotControls.LineBet);

        BetAmount.text = slotControls.LineBet.ToString();
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Token", Configuration.TokenLoginHeader);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            var responseText = www.downloadHandler.text;
            CommonUtil.CheckLog($"+ PlaceBetResponse Amount:{slotControls.LineBet}" + responseText);

            var jsonResponse = JsonUtility.FromJson<SlotPlaceBetResponse>(responseText);

            if (jsonResponse.code == 402)
            {
                CommonUtil.ShowToast(jsonResponse.message);
            }
            if (jsonResponse.code == 406)
            {
                CommonUtil.ShowToast(jsonResponse.message);
            }
            else
            {
                if (jsonResponse.code == 200)
                {
                    BetButton.interactable = false;
                    BetButton.GetComponentInChildren<Text>().text = "BET PLACED";
                    BetAnimator.enabled = false;
                    slotControls.Spin_Click();
                    //slotController.mainRotateTimeRandomize = ;
                    //slotController.mainRotateTime = 5 + timecount;
                    PlayerPrefs.SetString("wallet", jsonResponse.wallet.ToString());
                    UserWalletText.text = jsonResponse.wallet.ToString();
                    Debug.Log($"{jsonResponse.message}");
                }
            }
        }
        else
        {
            Debug.Log(www.error);
        }
        CommonUtil.CheckLog("RES_Check + API-Call + PlaceBet");
        /*
                Debug.Log("GETID" + Configuration.GetId() + "TOEKN" + Configuration.GetToken() + "id" + slotData.game_data[0].id.ToString() + "LINEBET::" + slotControls.LineBet.ToString());
                var formData = new Dictionary<string, string>
                   {
                       { "user_id", Configuration.GetId() },
                       { "token", Configuration.GetToken() },
                       { "game_id", slotData.game_data[0].id.ToString()},
                       { "amount", slotControls.LineBet.ToString() },
                   };
                Debug.Log("SlotControls.LineBet" + slotControls.LineBet);

                var jsonResponse = await APIManager.Instance.Post<SlotPlaceBetResponse>(url, formData);

                if (jsonResponse.code == 406)
                {
                    CommonUtil.ShowToast(jsonResponse.message);
                }
                else
                {
                    PlayerPrefs.SetString("wallet", jsonResponse.wallet.ToString());
                    UserWalletText.text = CommonUtil.GetFormattedWallet();
                    if (jsonResponse.code == 200)
                    {
                        Debug.Log($"Successfully Bet{jsonResponse.message}");
                    }
                } */
    }

    public async void updatedata() //string Token)
    {
        string url = Configuration.Url + Configuration.wallet;
        var formData = new Dictionary<string, string>
        {
            { "user_id", Configuration.GetId() },
            { "token", Configuration.GetToken() },
        };
        UserWalletText.text = PlayerPrefs.GetString("wallet");
        try
        {
            Wallet myResponse = await APIManager.Instance.Post<Wallet>(url, formData);
            if (myResponse.code == 200)
            {
                PlayerPrefs.SetString("wallet", myResponse.wallet);
                PlayerPrefs.Save();
                UserWalletText.text = myResponse.wallet;
            }
            else
            {
                UserWalletText.text = PlayerPrefs.GetString("wallet");
                CommonUtil.CheckLog("Error_new:" + myResponse.message);
            }
            CommonUtil.CheckLog("RES+Message" + myResponse.message);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            /*  CommonUtil.CheckLog("RES+Message" + myResponse.message);
             CommonUtil.CheckLog("RES+Code" + myResponse.code); */
        }
    }

    private void AddPaylinesToSlotController(List<List<int>> paylines)
    {
        slotController.payTable.Clear();
        Debug.Log("Paylines Count" + paylines.Count);
        if (slotController == null)
        {
            Debug.LogError("SlotController reference is not assigned.");
            return;
        }
        foreach (var line in paylines)
        {
            // Ensure the line is valid
            if (line == null || line.Count != slotController.slotGroupsBeh.Length)
            {
                Debug.LogWarning("Invalid payline format or length. Skipping.");
                continue;
            }

            // Create a new PayLine object for each line
            PayLine newPayLine = new PayLine
            {
                line = line.ToArray(), // Convert List<int> to int[]///// line = line.ToArray(),
                pay = 0, // Set a default pay value or calculate dynamically
                freeSpins = 0, // Set default free spins or calculate dynamically
                payMult = 1, // Default payout multiplier
                freeSpinsMult =
                    1 // Default free spins multiplier
                ,
            };

            // Add the new PayLine to the SlotController's payTable
            slotController.payTable.Add(newPayLine);
        }

        Debug.Log("Paylines added to SlotController.");
    }

    /* void PopulatePaylines(int paylineLength)
    {
        if (paylineLength <= 0)
        {
            Debug.LogError("Payline length must be greater than 0.");
            return;
        }
        paylines.Clear();

        // Split IntReelGrid into groups of paylineLength
        for (int i = 0; i < IntReelGrid.Count; i += paylineLength)
        {
            List<int> payline = new List<int>();
            // Add elements to the current payline
            for (int j = i; j < i + paylineLength && j < IntReelGrid.Count; j++)
            {
                payline.Add(IntReelGrid[j]);
            }

            paylines.Add(payline); // Add the payline to the list of paylines
        }
    } */
    public IEnumerator StopBetPopup()
    {
        stoptext.text = "STOP BET";
        AudioManager._instance?.PlayStopBetSound();
        showstop.SetActive(true);
        yield return new WaitForSeconds(2);
        showstop.SetActive(false);
    }

    public IEnumerator PlaceBetPopup()
    {
        AudioManager._instance?.PlayPlaceBetSound();
        stoptext.text = "Place BET";
        showstop.SetActive(true);
        showstop.transform.DOLocalMove(Vector3.zero, 0.8f).SetEase(Ease.OutBounce); //OutBounce
        yield return new WaitForSeconds(1.3f);
        //showstop.transform.DOLocalMove(Vector3.up * -126f, 0.66f).SetEase(Ease.OutBounce);
        showstop
            .transform.DOLocalMove(Vector3.up * 1380f, 0.8f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                showstop.SetActive(false);
            });
        // yield return new WaitForSeconds(3);
    }

    void PopulatePaylines(int paylineLength)
    {
        if (paylineLength <= 0)
        {
            Debug.LogError("Payline length must be greater than 0.");
            return;
        }
        paylines.Clear();

        // Populate paylines using every 3rd element from IntReelGrid
        for (int startIndex = 0; startIndex < 3; startIndex++) // Start at 0, 1, 2 (first three columns)
        {
            List<int> payline = new List<int>();

            for (int i = startIndex; i < IntReelGrid.Count; i += 3) // Jump every 3rd element
            {
                payline.Add(IntReelGrid[i]);

                if (payline.Count == paylineLength) // Stop when payline reaches the desired length
                    break;
            }

            if (payline.Count > 0)
            {
                paylines.Add(payline);
            }
        }
    }

    List<int> ConvertToIntegerList(string[] reelGrid)
    {
        // Create a dictionary for mapping
        Dictionary<string, int> mapping = new Dictionary<string, int>
        {
            { "A", 1 },
            { "B", 2 },
            { "C", 3 },
            { "W", 4 },
            { "*", 0 },
        };

        // Create an integer list
        List<int> intList = new List<int>();

        // Map each element in reelGrid to its corresponding integer value
        foreach (var element in reelGrid)
        {
            if (mapping.ContainsKey(element))
            {
                intList.Add(mapping[element]);
            }
            else
            {
                Debug.LogError($"Unmapped value: {element}");
            }
        }

        return intList;
    }
}
