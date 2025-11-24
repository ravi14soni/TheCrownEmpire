using UnityEngine;
using UnityEngine.UI;

public class ShareButtonRef : MonoBehaviour
{
     public GameObject share;
    public Button close;
    bool isPrivateSocket;
    
    public Button sharebtn;
     // Start is called once before the first execution of Update after the MonoBehaviour is created
     void Start()
     {
          isPrivateSocket = PlayerPrefs.GetInt("join") == 1 ? true : false;
          close.onClick.AddListener(closepopup);
          sharebtn.onClick.AddListener(openpopup);
          if (isPrivateSocket)
          {
             share.SetActive(false);
          }
          else
          { 
               share.SetActive(true);
          }
        
    }
     public void closepopup()
    { 
         share.SetActive(false);
    }
     public void openpopup()
    { 
         share.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
