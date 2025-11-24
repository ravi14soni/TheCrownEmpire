using System;
using System.Collections;
using System.Collections.Generic; // Requires Newtonsoft.Json package
using System.Threading.Tasks;
using Best.HTTP;
using Best.HTTP.Request.Upload.Forms;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    private static APIManager _instance;
    public static APIManager Instance => _instance;

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

    public async Task<T> Post<T>(string url, Dictionary<string, string> formData)
    {
        CommonUtil.CheckLog("url " + url);
        MultipartFormDataStream form = new MultipartFormDataStream();
        foreach (var field in formData)
        {
            form.AddField(field.Key, field.Value);
        }
        var request = HTTPRequest.CreatePost(url);
        string json = "API PROBLEM CONTECT WITH BACKEND";
        request.SetHeader("Token", Configuration.TokenLoginHeader);
        request.UploadSettings.UploadStream = form;
        try
        {
            var response = await request.GetHTTPResponseAsync();
            if (response.IsSuccess)
            {
                CommonUtil.CheckLog(
                    $"Res_CheckResponse: {typeof(T).FullName}" + response.DataAsText
                );
                //CommonUtil.CheckLog($"Expected return type: {typeof(T).FullName}");
                json = response.DataAsText;
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                CommonUtil.CheckLog(
                    $"Server sent an error: {response.StatusCode}-{response.Message}"
                );
                CommonUtil.CheckLog($"Server sent an error: {response.DataAsText}");
            }
        }
        catch (AsyncHTTPException e)
        {
            // 6. Error handling
            CommonUtil.CheckLog($"Request finished with error! Error: {e.Message}");
            return JsonConvert.DeserializeObject<T>(e.Message);
        }
        return JsonConvert.DeserializeObject<T>(json);
    }

    public async Task<T> PostWithCustomToken<T>(
        string url,
        Dictionary<string, string> formData,
        string token
    )
    {
        CommonUtil.CheckLog("url " + url);
        MultipartFormDataStream form = new MultipartFormDataStream();
        foreach (var field in formData)
        {
            form.AddField(field.Key, field.Value);
        }
        var request = HTTPRequest.CreatePost(url);
        string json = "API PROBLEM CONTECT WITH BACKEND";
        request.SetHeader("Token", token);
        request.UploadSettings.UploadStream = form;
        try
        {
            var response = await request.GetHTTPResponseAsync();
            if (response.IsSuccess)
            {
                CommonUtil.CheckLog(
                    $"Res_CheckResponse: {typeof(T).FullName}" + response.DataAsText
                );
                //CommonUtil.CheckLog($"Expected return type: {typeof(T).FullName}");
                json = response.DataAsText;
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                CommonUtil.CheckLog(
                    $"Server sent an error: {response.StatusCode}-{response.Message}"
                );
                CommonUtil.CheckLog($"Server sent an error: {response.DataAsText}");
            }
        }
        catch (AsyncHTTPException e)
        {
            // 6. Error handling
            CommonUtil.CheckLog($"Request finished with error! Error: {e.Message}");
            return JsonConvert.DeserializeObject<T>(e.Message);
        }
        return JsonConvert.DeserializeObject<T>(json);
    }

    public async Task<T> PostRaw<T>(string url, Dictionary<string, string> formData)
    {
        CommonUtil.CheckLog("url " + url);

        // Serialize the formData dictionary into a JSON string
        string jsonData = JsonConvert.SerializeObject(formData);

        var request = HTTPRequest.CreatePost(url);
        string json = "API PROBLEM CONNECT WITH BACKEND";

        // Set the headers to indicate JSON content
        request.SetHeader("Token", Configuration.TokenLoginHeader);
        request.SetHeader("Content-Type", "application/json");

        // Upload raw JSON data as the request body
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(jsonData)
        );

        try
        {
            var response = await request.GetHTTPResponseAsync();
            if (response.IsSuccess)
            {
                CommonUtil.CheckLog(
                    $"Res_CheckResponse: {typeof(T).FullName}" + response.DataAsText
                );
                json = response.DataAsText;
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                CommonUtil.CheckLog(
                    $"Server sent an error: {response.StatusCode}-{response.Message}"
                );
                CommonUtil.CheckLog($"Server sent an error: {response.DataAsText}");
            }
        }
        catch (AsyncHTTPException e)
        {
            // 6. Error handling
            CommonUtil.CheckLog($"Request finished with error! Error: {e.Message}");
            return JsonConvert.DeserializeObject<T>(e.Message);
        }

        return JsonConvert.DeserializeObject<T>(json);
    }

    public async Task GetWallet()
    {
        string Url = Configuration.Url + Configuration.wallet;
        CommonUtil.CheckLog("RES_Check + API-Call + profile");

        var formData = new Dictionary<string, string>
        {
            { "id", Configuration.GetId() },
            { "token", Configuration.GetToken() },
        };
        Wallet wallet = new Wallet();
        wallet = await Post<Wallet>(Url, formData);
        if (wallet.code == 200)
        {
            PlayerPrefs.SetString("wallet", wallet.wallet);
            PlayerPrefs.SetString("winning_wallet", wallet.winning_wallet);
            PlayerPrefs.Save();
        }
    }
}
