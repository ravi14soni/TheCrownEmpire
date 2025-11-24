using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Best.HTTP;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageUtil : MonoBehaviour
{
    private static ImageUtil _instance;
    private string Target;
    public bool bannerloaded;
    public bool isGuest;
    public static ImageUtil Instance => _instance;

    void OnEnable()
    {
        //Application.ExternalEval("UnityIsReady();");
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public async Task<Sprite> GetSpriteFromURLAsync(string url)
    {
        if (Configuration.ProfileImage == url)
        {
            Debug.Log("URL NOT Found:");
            return null;
        }
        try
        {
            // Download the image as a Texture2D
            Sprite texture = await DownloadImageToTextureAsync(url);

            // Convert the Texture2D to a Sprite
            return texture;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error downloading image from {url}: {ex.Message}");
            return null;
        }
    }

    private async Task<Sprite> DownloadImageToTextureAsync(string url)
    {
        var tcs = new TaskCompletionSource<Sprite>();

        // Configure the request
        HTTPRequest request = new HTTPRequest(
            new System.Uri(url),
            HTTPMethods.Get,
            (req, res) =>
            {
                if (res.IsSuccess && res.Data != null)
                {
                    try
                    {
                        Debug.Log("Image data received. Processing...");

                        // Create a texture from the raw byte data
                        Texture2D texture = new Texture2D(2, 2);
                        if (texture.LoadImage(res.Data)) // Load the image into the texture
                        {
                            Debug.Log("Image loaded successfully.");
                            Sprite sprite = Sprite.Create(
                                texture,
                                new Rect(0, 0, texture.width, texture.height),
                                new Vector2(0.5f, 0.5f)
                            );
                            tcs.SetResult(sprite);
                        }
                        else
                        {
                            throw new Exception("Failed to load texture from image data.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing image: {ex.Message}");
                        tcs.SetException(ex);
                    }
                }
                else
                {
                    Debug.LogError($"Download failed: {res?.Message ?? "Unknown error"}");
                    tcs.SetException(new Exception(res?.Message ?? "Unknown error"));
                }
            }
        );

        // Increase the buffer size to accommodate larger images
        request.DownloadSettings.ContentStreamMaxBuffered = 10 * 1024 * 1024; // 10 MB

        // Send the request
        request.Send();

        return await tcs.Task;
    }

    #region base64 from image

    public async Task<string> GetBase64FromTextureAsync(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null.");
            return null;
        }

        try
        {
            // Wait until the texture is encoded (avoids blocking the main thread)
            byte[] textureBytes = await Task.Run(() => texture.EncodeToPNG()); // Use EncodeToJPG for JPG images

            // Convert to Base64
            string base64String = Convert.ToBase64String(textureBytes);

            return base64String;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error encoding texture: {ex.Message}");
            return null;
        }
    }

    //string base64 = await GetBase64FromTextureAsync(texture);

    #endregion

    #region Set Image


    public void OpenGallery(string target, Image img, GameObject logo_img)
    {
#if UNITY_WEBGL
        CommonUtil.CheckLog("Clicked for opengallery");
        WebGLImageUtil.Target = target;
        WebGLImageUtil.ImageComponent = img;
        WebGLImageUtil.LogoImage = logo_img;
        CommonUtil.CheckLog("Clicked for webgl");
        // Trigger the gallery in the browser
        Application.ExternalCall("openGalleryWebGL", target);
#endif
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
        Debug.Log("Opening gallery to pick an image...");

        // Open the gallery using NativeGallery
         NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                // Load image from path into a Texture2D
                Texture2D originalTexture = NativeGallery.LoadImageAtPath(path, 2048);
                if (originalTexture == null)
                {
                    Debug.LogError("Couldn't load texture from " + path);
                    return;
                }
   
                // Open Cropper UI
                ImageCropper.Instance.Show(
                    originalTexture,
                    (bool result, Texture original, Texture2D cropped) =>
                    {
                        if (result) // user pressed OK
                        {
                            // Save cropped image temporarily
                             Texture2D resized = ResizeTexture(cropped, 104, 104);

            // Apply as sprite
             Sprite newSprite = Sprite.Create(resized, new Rect(0, 0, resized.width, resized.height), new Vector2(0.5f, 0.5f));
             img.sprite = newSprite;

            // Save if needed
                string croppedPath = SaveTempFile(resized);
                ProcessSelectedImage(croppedPath, target, img, logo_img);
                        }
                        else
                        {
                            Debug.LogWarning("Image crop cancelled!");
                        }
                    },
                   settings: new ImageCropper.Settings()
{
    ovalSelection = true,   // ✅ circle crop
    autoZoomEnabled = false,
    selectionMinSize = new Vector2(504, 504),
    selectionMaxSize = new Vector2(504, 504) // ✅ lock crop to 104x104
}
                );
            }
        }, "image/*");
        if (permission != NativeGallery.Permission.Granted)
        {
            Debug.LogError("File picker permission denied or not granted.");
        }
#endif
    }
private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
{
    RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
    Graphics.Blit(source, rt);

    RenderTexture previous = RenderTexture.active;
    RenderTexture.active = rt;

    Texture2D newTex = new Texture2D(newWidth, newHeight);
    newTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
    newTex.Apply();

    RenderTexture.active = previous;
    RenderTexture.ReleaseTemporary(rt);

    return newTex;
}

    private Texture2D MakeReadable(Texture2D texture)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(texture, tmp);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        Texture2D readableTex = new Texture2D(texture.width, texture.height);
        readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readableTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        return readableTex;
    }
private string SaveTempFile(Texture2D texture)
{
    Texture2D readableTex = MakeReadable(texture); // ✅ Ensure texture is readable
    string path = Path.Combine(Application.temporaryCachePath, "cropped.png");
    File.WriteAllBytes(path, readableTex.EncodeToPNG());
    return path;
}
    private void ProcessSelectedImage(string path, string target, Image img, GameObject logo_img)
    {
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log($"Image selected from gallery: {path}");

            ImageUtil.Instance.LoadImageFromFileAsync(path, target, img, logo_img);
        }
        else
        {
            CommonUtil.CheckLog("No image selected.");
            //Debug.LogError("No image selected.");
        }
    }

    private async void LoadImageFromFileAsync(
        string path,
        string name,
        Image image,
        GameObject logoimg
    )
    {
        if (File.Exists(path))
        {
            try
            {
                // Read the image file as bytes
                byte[] imageBytes = File.ReadAllBytes(path);

                // Create a Texture2D from the byte array
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes); // This automatically resizes the texture based on the image data

                // Create a sprite from the texture
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    Vector2.one * 0.5f
                );

                // Get base64 string from the image
                string imgUrl = GetBase64(imageBytes);

                // Use UnityMainThreadDispatcher to update the UI components on the main thread
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    HandleImage(name, image, logoimg, sprite, imgUrl);
                });
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Exception during image loading: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("File does not exist at path: " + path);
        }
    }

    public void OnImageSelected(string data)
    {
        CommonUtil.CheckLog("data after select: " + data);
        // Data format: "target|base64String"
        string[] parts = data.Split('|');
        if (parts.Length != 2)
        {
            Debug.LogError("Invalid data format received.");
            return;
        }

        string target = parts[0];
        string base64String = parts[1];
        byte[] imageBytes = System.Convert.FromBase64String(base64String);

        // Create a Texture2D from the byte array
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        // Convert to Sprite
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        // Apply the sprite to the image
        HandleImage(
            target,
            WebGLImageUtil.ImageComponent,
            WebGLImageUtil.LogoImage,
            sprite,
            base64String
        );
    }

    // Handle updating the image sprite and the base64 URL (executed on the main thread)
    private void HandleImage(
        string name,
        Image spriteimg,
        GameObject logoimg,
        Sprite sprite,
        string imgUrl
    )
    {
        // Check the image name and update the UI components accordingly
        if (name == "passbook")
        {
            // Set the sprite of the image UI component
            spriteimg.sprite = sprite;

            // Save the base64 string (if needed elsewhere in your code)
            SpriteManager.Instance.base64forimgpassbook = imgUrl;

            // Hide the loading logo or image
            logoimg.SetActive(false);
        }
        else if (name == "profile")
        {
            // Set the sprite of the image UI component
            spriteimg.sprite = sprite;

            // Save the base64 string (if needed elsewhere in your code)
            SpriteManager.Instance.base64forimgrofile = imgUrl;
            SpriteManager.Instance.profile_image = sprite;
        }
        else if (name == "ticket")
        {
            // Set the sprite of the image UI component
            spriteimg.sprite = sprite;

            // Save the base64 string (if needed elsewhere in your code)
            SpriteManager.Instance.base64forimgeforticket = imgUrl;
            //SpriteManager.Instance.profile_image = sprite;
        }
        else if (name == "adhar")
        {
            // Set the sprite of the image UI component
            spriteimg.sprite = sprite;

            // Save the base64 string (if needed elsewhere in your code)
            SpriteManager.Instance.base64forimgaadhar = imgUrl;

            // Hide the loading logo or image
            logoimg.SetActive(false);
        }
        else if (name == "pan")
        {
            // Set the sprite of the image UI component
            spriteimg.sprite = sprite;

            // Save the base64 string (if needed elsewhere in your code)
            SpriteManager.Instance.base64forimgpan = imgUrl;

            // Hide the loading logo or image
            logoimg.SetActive(false);
        }
        else if (name == "qr")
        {
            // Set the sprite of the image UI component
            spriteimg.sprite = sprite;

            // Save the base64 string (if needed elsewhere in your code)
            SpriteManager.Instance.base64forimgpan = imgUrl;
            SpriteManager.Instance.base64forimgcrypto = imgUrl;

            // Hide the loading logo or image
            logoimg.SetActive(false);
        }
        else if (name == "manual_ss")
        {
            spriteimg.sprite = sprite;

            // Save the base64 string (if needed elsewhere in your code)
            SpriteManager.Instance.base64forimgmanualss = imgUrl;

            // Hide the loading logo or image
            logoimg.SetActive(false);
        }
        if (name == "usdt_screenshort")
        {
            spriteimg.sprite = sprite;
            SpriteManager.Instance.base64forimgeforusdt = imgUrl;
            // Hide the loading logo or image
            logoimg.SetActive(false);
        }
        else
        {
            Debug.LogError("Unknown image name: " + name);
        }
    }

    // Utility method to convert image bytes to Base64 string
    private string GetBase64(byte[] imageBytes)
    {
        return System.Convert.ToBase64String(imageBytes);
    }
    #endregion
}

public static class WebGLImageUtil
{
    public static string Target;
    public static Image ImageComponent;
    public static GameObject LogoImage;
}
