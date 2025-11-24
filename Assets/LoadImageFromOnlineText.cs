using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class LoadImageFromOnlineText : MonoBehaviour
{
    [Header("Text file ka URL do (jo image ka link return kare)")]
    public string textFileUrl = "http://64.227.52.235/settings_data/refer_image.txt";

    [Header("UI Image ka reference do")]
    public Image targetImage;

    void Start()
    {
        StartCoroutine(GetImageUrlFromTextFile());
    }

    IEnumerator GetImageUrlFromTextFile()
    {
        Debug.Log("Downloading text file from: " + textFileUrl);

        UnityWebRequest request = UnityWebRequest.Get(textFileUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Text file load failed: " + request.error);
        }
        else
        {
            string imageUrl = request.downloadHandler.text.Trim();
            Debug.Log("👉 URL found in text file: " + imageUrl);

            if (string.IsNullOrEmpty(imageUrl))
            {
                Debug.LogError("❌ Text file is empty or invalid!");
                yield break;
            }

            StartCoroutine(LoadImage(imageUrl));
        }
    }

    IEnumerator LoadImage(string url)
    {
        Debug.Log("Downloading image from: " + url);

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Image load failed: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            if (texture == null)
            {
                Debug.LogError("❌ Texture is null, image not valid!");
                yield break;
            }

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                          new Vector2(0.5f, 0.5f));
            targetImage.sprite = sprite;

            Debug.Log("✅ Refer Image loaded successfully!");
        }
    }
}
