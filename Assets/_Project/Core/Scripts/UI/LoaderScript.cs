using System;
using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using Mkey;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class LoaderScript : MonoBehaviour
{
    public GameObject Panel;
    public Image slider;
    public TextMeshProUGUI text;

    public List<Image> AllImages = new List<Image>();

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    /// 

    void OnEnable()
    {
        if (transform.childCount > 2)
        {
            Panel = transform.GetChild(2).gameObject;
        }
        else
        {
            Panel = transform.GetChild(0).gameObject;
        }
        slider = Panel.transform.GetChild(1).GetComponent<Image>();
        text = Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        foreach (Transform image in transform.GetChild(0).transform)
        {
            AllImages.Add(image.GetComponent<Image>());
        }
    }


    public void CheckAndDownload(string scenename)
    {
        StartCoroutine(ManageSceneLoading(scenename));
    }
    private int Count = 0;
    private IEnumerator ManageSceneLoading(string scenename)
    {
#if UNITY_WEBGL
        Count++;
        if (Count == 2)
        {
            LoadDynamic(scenename);
        }
#endif

        bool isPackedTogetherDownloaded = false;
        bool isSceneDownloaded = false;

        yield return StartCoroutine(CheckAndDownloadSceneDefalut(scenename, result => isPackedTogetherDownloaded = result));

        // if (isPackedTogetherDownloaded)
        // {
        //     Debug.Log("Loading SceneA...");
        //     CommonUtil.ShowToast($"Download Successfuly...");
        //     Panel.SetActive(false);
        //     //LoadDynamic(scenename);
        // }
    }
    IEnumerator CheckAndDownloadSceneDefalut(string sceneAddress, System.Action<bool> callback)
    {
        yield return StartCoroutine(CheckAndDownloadScene(sceneAddress, callback));
    }
    IEnumerator CheckAndDownloadScene(string sceneadress, System.Action<bool> callback)
    {
        var checkSizeHandle = Addressables.GetDownloadSizeAsync(sceneadress);
        yield return checkSizeHandle;

        if (checkSizeHandle.Result > 0)
        {
            Debug.Log(
                $"Group {sceneadress} needs to be downloaded. Size: {checkSizeHandle.Result} bytes."
            );
            yield return StartCoroutine(DownloadScene(sceneadress));

            CommonUtil.ShowToast($"Download Successfully...");
            Panel.SetActive(false);
            SetTransparency(255);
        }
        else
        {
            Debug.Log($"Group {sceneadress} is already downloaded.");
            LoadDynamic(sceneadress);
        }
    }
    IEnumerator DownloadScene(string sceneadress)
    {
        Panel.SetActive(true);

        SetTransparency(180);


        // Step 1: Get Download Size
        AsyncOperationHandle<long> sizeCheck = Addressables.GetDownloadSizeAsync(sceneadress);
        yield return sizeCheck;

        if (sizeCheck.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("Failed to get download size.");
            yield break;
        }

        if (sizeCheck.Result > 0) // Only download if necessary
        {
            Debug.Log($"Downloading {sceneadress}...");
            var downloadHandle = Addressables.DownloadDependenciesAsync(sceneadress);

            while (!downloadHandle.IsDone)
            {
                var status = downloadHandle.GetDownloadStatus();
                float progress = status.Percent;
                UpdateProgress(progress);
                yield return null;
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"Successfully downloaded group: {sceneadress}");
                Addressables.Release(downloadHandle);
            }
            else
            {
                Debug.LogError($"Failed to download group: {sceneadress}");
                yield break;
            }
        }
        else
        {
            Debug.Log($"{sceneadress} is already downloaded.");
            Panel.SetActive(false);
            SetTransparency(255);
        }
    }
    void UpdateProgress(float progress)
    {
        text.color = Color.white;
        Panel.SetActive(true);
        int percent = Mathf.RoundToInt(progress * 100);
        slider.fillAmount = progress;
        Debug.Log("Percent %:" + percent + "Progress %:" + progress);
        text.text = "Downloading.. " + percent + "%";
    }
    private void LoadDynamic(string scene)
    {
        if (slider != null)
        {
            SetTransparency(255);
            Panel.gameObject.SetActive(false); // Hide progress UI
        }

        if (scene == "point")
        {
            loaddynamicscenebyname("PointTable.unity");
        }
        else if (scene == "pool")
        {
            loaddynamicscenebyname("Join_Table(Pool).unity");
        }
        else if (scene == "deal")
        {
            loaddynamicscenebyname("Join_Table(Deal).unity");
        }
        else if (scene == "teenpatti")
        {
            loaddynamicscenebyname("TeenPatti_GamePlay.unity");
        }
        else
        {
            Debug.Log("Name of Load Scene:" + scene);
            loaddynamicscenebyname(scene);
        }
    }

    public void loaddynamicscenebyname(string scenename)
    {
        SceneLoader.Instance.LoadDynamicScene(scenename);
    }
    [Button]
    public void SetTransparency(float alphaValue)
    {
        Debug.Log("Alpha" + alphaValue + "allimages" + AllImages);
        float alpha = Mathf.Clamp(alphaValue, 0, 255) / 255f; // Convert 0-255 to 0-1
        foreach (var img in AllImages)
        {
            if (img != null)
            {
                Color color = img.color;
                color.a = alpha; // Ensure alpha is between 0 and 1
                img.color = color;
            }
        }
    }
}
