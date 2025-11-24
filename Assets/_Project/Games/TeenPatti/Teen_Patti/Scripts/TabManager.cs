using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro ke liye

public class TabManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button publicTab;
    public Button privateTab;
    public Button joinTableTab;   // ✅ New Button
    public PrivateTablePopup popup;

    [Header("Tab Texts")]
    public TMP_Text publicTabText;
    public TMP_Text privateTabText;
    public TMP_Text joinTableTabText;   // ✅ New Text

    [Header("Panels / ScrollViews")]
    public GameObject publicScrollView;
    public GameObject privateScrollView;   // ✅ New panel
    public GameObject joinTableScrollView; // ✅ New panel

    [Header("Tab Sprites")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;

    public enum TabType { Public, Private, JoinTable }
    public TabType currentTab { get; private set; } = TabType.Public;

    void Start()
    {
        popup = FindObjectOfType<PrivateTablePopup>();
        publicTab.onClick.AddListener(ShowPublic);
        privateTab.onClick.AddListener(ShowPrivate);
        joinTableTab.onClick.AddListener(ShowJoinTable); // ✅ Listener add kiya

        // Load saved state (0 = Public, 1 = Private, 2 = JoinTable)
        int savedValue = PlayerPrefs.GetInt("SelectedTab", 0);

        if (savedValue == 1)
            ShowPrivate();
        else if (savedValue == 2)
            ShowJoinTable();
        else
            ShowPublic();
    }

    void ShowPublic()
    {
        currentTab = TabType.Public;
        Debug.Log("PLAY NOW CLICKED! Tab = Public");

        // Panels toggle
        publicScrollView.SetActive(true);

        // Buttons UI update
        publicTab.GetComponent<Image>().sprite = activeSprite;
        privateTab.GetComponent<Image>().sprite = inactiveSprite;
        joinTableTab.GetComponent<Image>().sprite = inactiveSprite;

        // Text colors
        publicTabText.color = Color.white;
        privateTabText.color = Color.black;
        joinTableTabText.color = Color.black;

        PlayerPrefs.SetInt("SelectedTab", 0);
    }

    void ShowPrivate()
    {
        currentTab = TabType.Private;
        Debug.Log("PLAY NOW CLICKED! Tab = Private");

        // Panels toggle
        publicScrollView.SetActive(true);

        // Buttons UI update
        publicTab.GetComponent<Image>().sprite = inactiveSprite;
        privateTab.GetComponent<Image>().sprite = activeSprite;
        joinTableTab.GetComponent<Image>().sprite = inactiveSprite;

        // Text colors
        publicTabText.color = Color.black;
        privateTabText.color = Color.white;
        joinTableTabText.color = Color.black;

        PlayerPrefs.SetInt("SelectedTab", 1);
    }

    void ShowJoinTable()
    {
        currentTab = TabType.JoinTable;
        Debug.Log("PLAY NOW CLICKED! Tab = Join Table");

        // Panels toggle
        publicScrollView.SetActive(false);


        // Buttons UI update
        publicTab.GetComponent<Image>().sprite = inactiveSprite;
        privateTab.GetComponent<Image>().sprite = inactiveSprite;
        joinTableTab.GetComponent<Image>().sprite = activeSprite;

        // Text colors
        publicTabText.color = Color.black;
        privateTabText.color = Color.black;
        joinTableTabText.color = Color.white;

        PlayerPrefs.SetInt("SelectedTab", 1);
        popup.OpenPopup();
    }
    public void SelectTab(int tabIndex)
    {
        if (tabIndex == 0) ShowPublic();
        else if (tabIndex == 1) ShowPrivate();
        else if (tabIndex == 2) ShowJoinTable();
    }
}
