using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class ReferImageLoad : MonoBehaviour
{
    [Header("Image ka direct URL do")]
    public string imageUrl = "https://www.shutterstock.com/image-vector/vector-illustration-urban-architecture-cityscape-260nw-1621311928.jpg";

    [Header("UI Image ka reference do")]
    public Image targetImage;

    void OnEnable()
    {
        StartCoroutine(LoadImage(imageUrl));
    }

    IEnumerator LoadImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Image load failed: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                          new Vector2(0.5f, 0.5f));
            targetImage.sprite = sprite;
        }
    }
}
