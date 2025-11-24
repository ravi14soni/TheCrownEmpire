using AndroApps;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ChaalSlider : MonoBehaviour
{
    public Slider slider;
    public float totalTime = 30f;
    public int seatNumber;   // seat position from server (1,2,3,4)
    public float timer;
    public bool ischaal;
    public Image sliderimage;
    public string id;
    public GameObject obj, obj2;
    public List<GameObject> prefab;
    public GameObject startcoinprefab;
    public Transform parent;
    public Transform parent2;
    public bool animated, begin, animated2;
    public string scenename;
    public string game;
    public string amount;
    public bool anim, start;
    public int autochaalcount;
    public bool animatedchaalovercard;
    public GameObject backcard;

    public bool showhelp;
    public GameObject placeholderglow, discardglow, finishdeskglow;
    public AudioSource coinaudio, timeraudio, chaalaudio;
    public bool callaudio, stopupdategameamount, check;
    public TextMeshProUGUI timertext;
    private bool starttimeraudio, startchaalaudio;
    public int seat;
    public float lerpSpeed = 3f;
    private float targetValue;
    public bool mychaal;
    public Transform targetPosition;
    public float duration = 1f;
    public GameObject coinpanel;
    public bool isRechargingUser;

    void OnEnable()
    {
        timer = 0;
        check = true;
    }

    private void Update()
    {
        if (scenename == "teenpatti")
        {
            obj = GameObject.Find("Manager (1)").transform.GetChild(0).gameObject;
            TeenPattiSocketManager tpm = obj.GetComponent<TeenPattiSocketManager>();

            if (tpm == null) return;

            if (tpm.gamestart && tpm.firstbetcoin && check)
            {
                check = false;
                callaudio = true;
                StartCoroutine(startcoinanim());
            }

            if (ischaal)
            {
                HandleChaalState(tpm);
            }
            else
            {
                HandleNoChaalState(tpm);
            }
        }
    }

    private void HandleChaalState(TeenPattiSocketManager tpm)
    {
        callaudio = true;
        stopupdategameamount = true;
        animated = false;
        coinaudio.gameObject.SetActive(false);

        if (tpm.chaaltimer == 0)
        {
            slider.gameObject.SetActive(false);
            timertext.gameObject.SetActive(false);
        }
        else
        {
            if (!isRechargingUser)
            {
                slider.gameObject.SetActive(true);
                timertext.text = tpm.chaaltimer.ToString();
                targetValue = Mathf.Clamp01((totalTime - tpm.chaaltimer) / totalTime);
                slider.value = Mathf.Lerp(slider.value, targetValue, lerpSpeed * Time.deltaTime);

                if (tpm.chaaltimer > 15)
                    SetSliderColor(Color.green);
                else if (tpm.chaaltimer > 5)
                    SetSliderColor(Color.yellow);
                else
                    SetSliderColor(Color.red);
            }
            else
            {
                totalTime = 300;
                slider.gameObject.SetActive(true);
                timertext.text = tpm.chaaltimer.ToString();
                targetValue = Mathf.Clamp01((totalTime - tpm.chaaltimer) / totalTime);
                slider.value = Mathf.Lerp(slider.value, targetValue, lerpSpeed * Time.deltaTime);

                if (tpm.chaaltimer > 150)
                    SetSliderColor(Color.green);
                else if (tpm.chaaltimer > 50)
                    SetSliderColor(Color.yellow);
                else
                    SetSliderColor(Color.red);
            }
        }

        if (id == Configuration.GetId())
        {
            if (!startchaalaudio)
            {
                if (Configuration.GetSound() == "on")
                {
                    chaalaudio.volume = 1f;
                    chaalaudio.Play();
                }
                startchaalaudio = true;
            }
            tpm.canvasstart();
            anim = true;
        }

        // Seat based turn order logic
        if (!begin)
        {
            // IMPORTANT: Use this instance's seatNumber to build the clockwise list
            AddSeatBasedTurnOrder(tpm);
            Debug.Log("=== Seat-based Turn Order ===");
            foreach (string pid in tpm.IDtoplay)
                Debug.Log("Seat Order -> " + pid);
            begin = true;
        }

        if (timer >= 15f && !starttimeraudio)
        {
            if (Configuration.GetSound() == "on")
            {
                timeraudio.volume = 1f;
                timeraudio.Play();
            }
            starttimeraudio = true;
        }

        if (timer >= 29f && Configuration.GetId() == id)
        {
            tpm.leave();
            timer = 0f;
        }
    }

    private void HandleNoChaalState(TeenPattiSocketManager tpm)
    {
        timeraudio.Stop();
        starttimeraudio = false;
        startchaalaudio = false;

        if (id == Configuration.GetId())
            tpm.stopcanvas();

        totalTime = 30f;
        timer = 0f;
        slider.value = 0;
        slider.gameObject.SetActive(false);
        timertext.gameObject.SetActive(false);
        begin = false;
    }

    private void SetSliderColor(Color color)
    {
        var img = slider.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>();
        color.a = 0.5f;
        img.color = color;
    }

    IEnumerator startcoinanim()
    {
        Debug.Log("RES_Check + Start anim " + this.gameObject.transform.parent.name);
        yield return new WaitForSeconds(0.2f);
        obj.GetComponent<TeenPattiSocketManager>().firstbetcoin = false;
        Firstchaalanim();
    }

    // ** IMPORTANT: This uses seatNumber from each ChaalSlider instance **
    private void AddSeatBasedTurnOrder(TeenPattiSocketManager tpm)
    {
        if (tpm == null)
        {
            Debug.LogError("tpm is null in AddSeatBasedTurnOrder");
            return;
        }

        tpm.IDtoplay.Clear();

        // Build map seat -> user_id using the actual ChaalSlider seatNumber values
        var seatMap = new Dictionary<int, string>();

        // Ensure manager exposes a 'profiles' list of GameObjects (same as earlier)
        foreach (var profile in tpm.profiles)
        {
            if (profile == null) continue;
            var cs = profile.GetComponent<ChaalSlider>();
            if (cs == null) continue;
            if (cs.seatNumber <= 0) continue; // skip unassigned
            if (!seatMap.ContainsKey(cs.seatNumber))
                seatMap[cs.seatNumber] = cs.id;
        }

        // Standard clockwise ordering (visual): 1 -> 2 -> 3 -> 4
        List<int> fullClockwise = new List<int> { 1, 2, 3, 4 };
        var activeSeats = fullClockwise.Where(s => seatMap.ContainsKey(s)).ToList();

        if (activeSeats.Count == 0)
        {
            Debug.LogError("No active seats found in AddSeatBasedTurnOrder");
            return;
        }

        // Use this player's seatNumber as starting index (so clockwise is relative to you)
        int mySeat = this.seatNumber;
        int startIndex = activeSeats.IndexOf(mySeat);
        if (startIndex == -1) startIndex = 0;

        for (int i = 0; i < activeSeats.Count; i++)
        {
            int seat = activeSeats[(startIndex + i) % activeSeats.Count];
            tpm.IDtoplay.Add(seatMap[seat]);
            Debug.Log($"🔁 Clockwise -> Seat {seat} -> {seatMap[seat]}");
        }

        // If visuals appear reversed (anticlockwise), uncomment the next line to reverse:
        // tpm.IDtoplay.Reverse();

        Debug.Log("Final IDtoplay:");
        foreach (var id in tpm.IDtoplay) Debug.Log(id);
    }

    public void Firstchaalanim()
    {
        Debug.Log("RES_Check + Start Chaal anim " + this.gameObject.transform.name);
        obj = GameObject.Find("Manager (1)").transform.GetChild(0).gameObject;

        GameObject coin = Instantiate(startcoinprefab, parent);
        coin.transform.localPosition = Vector3.zero;
        coin.transform.DOMove(targetPosition.position, duration).SetEase(Ease.Linear);
        coin.transform.DOScale(Vector3.zero, duration).SetEase(Ease.Linear);

        StartCoroutine(startcoinpanel());
    }

    IEnumerator startcoinpanel()
    {
        yield return new WaitForSeconds(0.3f);
        if (id == Configuration.GetId())
        {
            coinpanel.transform.DOScale(new Vector3(0.01f, 0.01f, 0.01f), 0.4f).SetEase(Ease.Linear);
        }
    }
}

[System.Serializable]
public class PlayerSeatData
{
    public string playerId;
    public int seatPosition;
}
