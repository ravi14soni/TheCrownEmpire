using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mkey;
using TMPro;
//using Profile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions.Must;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine;
using IngameDebugConsole;


public class LoadingManager : MonoBehaviour
{
    public Slider slider;
    public float duration = 2f;

    private float startTime;
    private float startValue;
    private float targetValue;
    public Slider progressBar;
    public GameObject panel;
    public TextMeshProUGUI progressText,
        download_text;
    public GameObject playbutton;
    public GameSelection select;
    public string packedTogetherGroupLabel = "Login-Home";

    void Awake()
    {
#if UNITY_ANDROID
        Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
        //Caching.ClearCache();
    }
    void Start()
    {
        CheckAndDownload();
    }


    IEnumerator TransitionAfterDelay()
    {
        while (Time.time - startTime < duration)
        {
            // Calculate the current progress of the movement
            float progress = (Time.time - startTime) / duration;

            // Interpolate between start and target values
            float currentValue = Mathf.Lerp(startValue, targetValue, progress);

            // Set the slider value
            slider.value = currentValue;

            // Wait for the next frame
            yield return null;
        }

        slider.value = targetValue;
        string id = Configuration.GetId();
        CommonUtil.CheckLog("RES_Check + name " + Configuration.GetName());

        if (id != string.Empty)
        {
            Debug.Log("HomePage.unity LogOut Autometic!!!!!!");
            Addressables.LoadSceneAsync("HomePage.unity", LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("LoginRegister.unity LogOut Autometic!!!!!!");
            Addressables.LoadSceneAsync("LoginRegister.unity", LoadSceneMode.Single);
        }
    }

    public void CheckAndDownload()
    {
        StartCoroutine(ManageSceneLoading());
    }

    IEnumerator ManageSceneLoading()
    {
        bool isPackedTogetherDownloaded = false;
        yield return StartCoroutine(CheckAndDownloadGroup(packedTogetherGroupLabel, result => isPackedTogetherDownloaded = result));
        // if (isPackedTogetherDownloaded)
        // {
        //     Debug.Log("Loading SceneA...");
        //     StartCoroutine(TransitionAfterDelay());
        // }
        // else
        // {
        //     Debug.Log("Need to Download Not in Cache..." + isPackedTogetherDownloaded);
        // }
    }

    // IEnumerator CheckAndDownloadGroup(string groupLabel, System.Action<bool> callback)
    // {
    //     var checkSizeHandle = Addressables.GetDownloadSizeAsync(groupLabel);
    //     yield return checkSizeHandle;

    //     if (checkSizeHandle.Result > 0)
    //     {
    //         Debug.Log(
    //             $"Group {groupLabel} needs to be downloaded. Size: {checkSizeHandle.Result} bytes."
    //         );
    //         yield return StartCoroutine(DownloadGroup(groupLabel));
    //         callback(true);
    //     }
    //     else
    //     {
    //         Debug.Log($"Group {groupLabel} is already downloaded.");
    //         callback(true);
    //     }
    // }
    IEnumerator CheckAndDownloadGroup(string groupLabel, System.Action<bool> callback)
    {

        // var checkLocationsHandle = Addressables.LoadResourceLocationsAsync(groupLabel);
        // yield return checkLocationsHandle;
        // string cachePath = Caching.defaultCache.path;
        // Debug.Log(" checkLocationsHandle Cache Path: " + cachePath);

        // Debug.Log("checkLocationsHandle::" + checkLocationsHandle);

        // if (checkLocationsHandle.Status != AsyncOperationStatus.Succeeded || checkLocationsHandle.Result.Count == 0)
        // {
        //     Debug.LogError($" checkLocationsHandle No assets found for label {groupLabel}.");
        //     yield return StartCoroutine(DownloadGroup(groupLabel));
        //     yield break;
        //     //callback(false);
        // }
        // else
        // {
        //     Debug.Log($"checkLocationsHandle Group {groupLabel} is already cached, loading directly.");
        //     Debug.Log("checkLocationsHandle Asset is cached. Cache Path: " + Caching.defaultCache.path);

        //     StartCoroutine(TransitionAfterDelay());
        //     //callback(true);
        // }

        var checkSizeHandle = Addressables.GetDownloadSizeAsync(groupLabel);
        yield return checkSizeHandle;
        bool IsInCache = checkSizeHandle.Result == 0;
        if (IsInCache)
        {
            Debug.Log("Asset is in Cache !");
            Debug.Log($"Group {groupLabel} is already cached, loading directly.");
            callback(true);
            StartCoroutine(TransitionAfterDelay());
        }
        else
        {
            Debug.Log($"Group {groupLabel} needs to be downloaded. Size: {checkSizeHandle.Result} bytes.");
            yield return StartCoroutine(DownloadGroup(groupLabel));
            Debug.Log("Asset is NOT in Cache !");
            callback(true);
        }

        // if (checkSizeHandle.Status == AsyncOperationStatus.Succeeded)
        // {

        //     if (checkSizeHandle.Result > 0) // Addressables think it needs to download
        //     {
        //         Debug.Log($"Group {groupLabel} needs to be downloaded. Size: {checkSizeHandle.Result} bytes.");
        //         yield return StartCoroutine(DownloadGroup(groupLabel));
        //     }
        //     else
        //     {
        //         Debug.Log($"Group {groupLabel} is already cached, loading directly.");
        //     }
        //     callback(true);
        // }
        // else
        // {
        //     Debug.LogError("Failed to get download size.");
        //     callback(false);
        // }
    }

    IEnumerator DownloadGroup(string groupLabel)
    {
        // Step 1: Get Download Size
        AsyncOperationHandle<long> sizeCheck = Addressables.GetDownloadSizeAsync(groupLabel);
        yield return sizeCheck;

        if (sizeCheck.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("Failed to get download size.");
            yield break;
        }

        if (sizeCheck.Result > 0) // Only download if necessary
        {
            Debug.Log($"Downloading {groupLabel}...");
            var downloadHandle = Addressables.DownloadDependenciesAsync(groupLabel);

            while (!downloadHandle.IsDone)
            {
                var status = downloadHandle.GetDownloadStatus();
                float progress = status.Percent;
                UpdateProgress(progress);
                yield return null;
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"Successfully downloaded group: {groupLabel}");
                Addressables.Release(downloadHandle);

                // Ensure UI reaches 100% before loading the scene
                string id = Configuration.GetId();
                if (id != string.Empty)
                {
                    StartCoroutine(SmoothProgressTo100AndLoadScene("HomePage.unity"));
                }
                else
                {
                    StartCoroutine(SmoothProgressTo100AndLoadScene("LoginRegister.unity"));
                }
            }
            else
            {
                Debug.LogError($"Failed to download group: {groupLabel}");
                yield break;
            }
        }
        else
        {
            Debug.Log($"{groupLabel} is already downloaded.");
            StartCoroutine(LoadSceneSafely("LoginRegister.unity"));
        }
    }

    void UpdateProgress(float progress)
    {
        panel.SetActive(true);
        int percent = Mathf.RoundToInt(progress * 100);
        Debug.Log("Percent %:" + percent);
        progressBar.value = progress;
        progressText.text = "Downloading.. " + percent + "%";
        //progressText.text = "Downloading.. " + progress + "%";
    }

    // Smooth transition from current progress to 100% then load scene
    IEnumerator SmoothProgressTo100AndLoadScene(string sceneName)
    {
        float currentProgress = progressBar.value;
        float duration = 0.2f; // Faster transition to 100%
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newProgress = Mathf.Lerp(currentProgress, 1f, elapsedTime / duration);
            UpdateProgress(newProgress);
            yield return null;
        }

        // UpdateProgress(1f); // Ensure it's exactly 100% at the end

        // Load the scene immediately after progress reaches 100%
        StartCoroutine(LoadSceneSafely(sceneName));
    }

    IEnumerator LoadSceneSafely(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");

        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Single
        );
        yield return handle;


        // var status = handle.GetDownloadStatus();
        // float progress = status.Percent;

        // progressText.text = "Downloading.. " + progress + "%";
        // yield return null;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            //Debug.Log($"Scene {sceneName} loaded successfully!");
            CommonUtil.ShowToastDebug($"{sceneName} loaded successfully!");
        }
        else
        {
            Debug.LogError($"Failed to load scene: {sceneName}");
        }
    }

    // private IEnumerator CheckAndLoadScene(string sceneKey)
    // {
    //     // Check the download size of the specific scene
    //     AsyncOperationHandle<long> sizeCheckHandle = Addressables.GetDownloadSizeAsync(sceneKey);
    //     yield return sizeCheckHandle;

    //     if (sizeCheckHandle.Status == AsyncOperationStatus.Succeeded)
    //     {
    //         if (sizeCheckHandle.Result == 0)
    //         {
    //             // Scene is already downloaded, load it instantly
    //             Debug.Log($"Scene {sceneKey} is already downloaded. Loading...");
    //             startTime = Time.time;
    //             startValue = slider.minValue;
    //             targetValue = slider.maxValue;
    //             StartCoroutine(TransitionAfterDelay());
    //         }
    //         else
    //         {
    //             // Scene needs to be downloaded, download only this scene
    //             Debug.Log(
    //                 $"Downloading scene {sceneKey}. Size: {sizeCheckHandle.Result / 1024f / 1024f:F2} MB"
    //             );
    //             StartCoroutine(DownloadAndLoadScenes("LoginRegister.unity", "HomePage.unity"));
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError($"Failed to check scene {sceneKey} download size.");
    //     }

    //     Addressables.Release(sizeCheckHandle);
    // }

    // private IEnumerator DownloadAndLoadScenes(string sceneKey1, string sceneKey2)
    // {
    //     // 1️⃣ Get total download size for both scenes
    //     AsyncOperationHandle<long> getSizeHandle1 = Addressables.GetDownloadSizeAsync(sceneKey1);
    //     AsyncOperationHandle<long> getSizeHandle2 = Addressables.GetDownloadSizeAsync(sceneKey2);

    //     yield return getSizeHandle1;
    //     yield return getSizeHandle2;

    //     long totalSize1 = getSizeHandle1.Result;
    //     long totalSize2 = getSizeHandle2.Result;
    //     long totalSize = totalSize1 + totalSize2;

    //     Debug.Log($"Total Download Size: {totalSize / (1024f * 1024f):F2} MB");

    //     // If both are already cached, skip download
    //     if (totalSize == 0)
    //     {
    //         Debug.Log("Both scenes are already cached. Skipping download.");
    //         progressBar.value = 1f;
    //         progress_text.text = "Download Complete!";
    //         yield return new WaitForSeconds(0.5f);
    //         select.loaddynamicscenebyname("LoginRegister.unity");
    //         yield break;
    //     }

    //     // 2️⃣ Start downloading both scenes
    //     AsyncOperationHandle handle1 = Addressables.DownloadDependenciesAsync(sceneKey1);
    //     AsyncOperationHandle handle2 = Addressables.DownloadDependenciesAsync(sceneKey2);

    //     if (progressBar != null)
    //         panel.gameObject.SetActive(true);

    //     float progress = 0f;
    //     float elapsedTime = 0f;
    //     float estimatedTime = 10f; // ⏳ Adjust this to control how long the progress should take

    //     // 3️⃣ Smooth Progress Update with Downloaded MB
    //     while (!handle1.IsDone || !handle2.IsDone)
    //     {
    //         elapsedTime += Time.deltaTime;
    //         float timeBasedProgress = Mathf.Clamp01(elapsedTime / estimatedTime); // ⏳ Ensures equal distribution over time

    //         float sizeProgress1 =
    //             totalSize1 > 0 ? handle1.PercentComplete * (totalSize1 / (float)totalSize) : 0f;
    //         float sizeProgress2 =
    //             totalSize2 > 0 ? handle2.PercentComplete * (totalSize2 / (float)totalSize) : 0f;
    //         float realProgress = sizeProgress1 + sizeProgress2;

    //         progress = Mathf.Lerp(
    //             progress,
    //             Mathf.Max(realProgress, timeBasedProgress),
    //             Time.deltaTime * 3f
    //         );
    //         progress = Mathf.Clamp(progress, 0f, 1f);

    //         // Calculate downloaded size in MB
    //         float downloadedSize = progress * (totalSize / (1024f * 1024f));
    //         int intProgress = Mathf.RoundToInt(progress * 100);

    //         progressBar.value = progress;

    //         // Separate the percentage progress and downloaded size in different text components
    //         progress_text.text = $"{intProgress}%"; // Show only percentage
    //         download_text.text = $"{downloadedSize:F0} MB / {totalSize / (1024f * 1024f):F0} MB"; // Show downloaded size

    //         yield return null;
    //     }

    //     // 4️⃣ Ensure progress reaches 100% smoothly
    //     while (progress < 1f)
    //     {
    //         progress += Time.deltaTime / 1.5f;
    //         progress = Mathf.Clamp(progress, 0f, 1f);

    //         float downloadedSize = progress * (totalSize / (1024f * 1024f));
    //         int intProgress = Mathf.RoundToInt(progress * 100);

    //         progressBar.value = progress;
    //         progress_text.text =
    //             $"Downloading: {intProgress}% ({downloadedSize:F0} MB / {totalSize / (1024f * 1024f):F0} MB)";
    //         yield return null;
    //     }

    //     // 5️⃣ Handle success/failure
    //     if (
    //         handle1.Status == AsyncOperationStatus.Succeeded
    //         && handle2.Status == AsyncOperationStatus.Succeeded
    //     )
    //     {
    //         Debug.Log("Both scenes downloaded successfully. Proceeding to load...");
    //         panel.gameObject.SetActive(false);
    //         progress_text.gameObject.SetActive(false);
    //         select.loaddynamicscenebyname("LoginRegister.unity");
    //     }
    //     else
    //     {
    //         Debug.LogError("Failed to download one or both scenes.");
    //     }

    //     Addressables.Release(handle1);
    //     Addressables.Release(handle2);
    // }

    private void LoadScene(string sceneKey)
    {
        select.loaddynamicscenebyname("LoginRegister.unity");
        // Load the scene after ensuring it's downloaded
        Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Single);

        if (progressBar != null)
        {
            panel.gameObject.SetActive(false); // Hide progress UI
        }
    }
}
