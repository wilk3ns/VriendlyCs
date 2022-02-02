using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine.Networking;
using UnityEngine;
using System.Text;
using System;
using Vriendly.Backend.Repository;

namespace Vriendly.Backend
{

    public class Hash
    {
        private static UnicodeEncoding _encoder = new UnicodeEncoding();
        public static string getHashSha256(string text)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(text));
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string Encrypt(string data)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString("vriendlyPublic");
            var dataToEncrypt = _encoder.GetBytes(data);
            var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false);
            var length = encryptedByteArray.Length;
            var item = 0;
            var sb = new StringBuilder();
            foreach (var x in encryptedByteArray)
            {
                item++;
                sb.Append(x);

                if (item < length)
                    sb.Append(",");
            }

            return sb.ToString();
        }

        public static string Decrypt(string data)
        {
            var rsa = new RSACryptoServiceProvider();
            var dataArray = data.Split(new char[] { ',' });
            byte[] dataByte = new byte[dataArray.Length];
            for (int i = 0; i < dataArray.Length; i++)
            {
                dataByte[i] = Convert.ToByte(dataArray[i]);
            }

            rsa.FromXmlString("vriendlyPublic");
            var decryptedByte = rsa.Decrypt(dataByte, false);
            return _encoder.GetString(decryptedByte);
        }
    }
    public class BackendAPI : MonoBehaviour
    {

        protected const string APIHost = "https://api.vriendly.co/";
        protected const string AnalyticsAPIHost = "https://analytics.vriendly.co/";
        protected const int NO_INTERNET_CONNECTION = 2;
        protected const int SERVER_ERROR = 1;
        protected const int DONE = 0;

        private static string _apiToken = "";
        public IEnumerator getVersions()
        {
            yield return null;
        }




        public static IEnumerator startSession(int attempts, Action<Dictionary<ProcessState, string>> callback)
        {

            while (attempts > 0)
            {
                attempts--;
                string url = AnalyticsAPIHost + "startSession";
                Debug.Log("url = " + url);

                Dictionary<ProcessState, string> result = new Dictionary<ProcessState, string>();

                WWWForm form = new WWWForm();

                form.AddField("deviceModel", SystemInfo.deviceModel);

                form.AddField("os", SystemInfo.operatingSystem);

                Debug.Log($"deviceModel: {SystemInfo.deviceModel}, OS: {SystemInfo.operatingSystem}");
                UnityWebRequest uwr = UnityWebRequest.Post(url, form);

                uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                uwr.SetRequestHeader("Authorization", Repo.loginData.user.token);

                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError)
                {
                    if (attempts == 0)
                    {
                        result.Add(ProcessState.NetworkError, "Network Error");
                        Debug.Log("Network Error");
                        callback(result);
                        continue;
                    }
                }
                else if (uwr.responseCode == 201)
                {
                    string jsonString = uwr.downloadHandler.text;
                    result.Add(ProcessState.Successful, jsonString);
                    Debug.Log($"Successfull received:{jsonString}");
                    callback(result);
                    break;
                }
                else
                {
                    if (attempts == 0)
                    {
                        result.Add(ProcessState.NetworkError, "Maybe unauthorized");
                        Debug.Log($"Other, responseCode: {uwr.downloadHandler.text}");
                        callback(result);
                        continue;
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        public static IEnumerator endSession(int attempts, string sessionId, Action<ProcessState> callback)
        {
            while (attempts > 0)
            {
                attempts--;
                string url = AnalyticsAPIHost + $"endSession/{sessionId}";
                Debug.Log("url = " + url);

                var result = ProcessState.None;

                UnityWebRequest uwr = UnityWebRequest.Get(url);

                uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                uwr.SetRequestHeader("Authorization", Repo.loginData.user.token);

                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError)
                {
                    if (attempts == 0)
                    {
                        result = ProcessState.NetworkError;
                        Debug.Log("Network Error");
                        callback(result);
                        continue;
                    }
                }
                else if (uwr.responseCode == 200)
                {
                    result = ProcessState.Successful;
                    Debug.Log("Successfull");
                    callback(result);
                    break;
                }
                else
                {
                    if (attempts == 0)
                    {
                        result = ProcessState.NetworkError;
                        Debug.Log("Maybe unauthorized");
                        callback(result);
                        continue;
                    }
                }
                yield return new WaitForSeconds(1f);
            }

        }


        public static IEnumerator enteredRoom(int attempts, string roomId, Action<Dictionary<ProcessState, string>> callback)
        {
            while (attempts > 0)
            {
                attempts--;

                string url = AnalyticsAPIHost + $"enteredRoom/{roomId}";

                Debug.Log("url = " + url);

                Dictionary<ProcessState, string> result = new Dictionary<ProcessState, string>();

                UnityWebRequest uwr = UnityWebRequest.Get(url);

                uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                uwr.SetRequestHeader("Authorization", Repo.loginData.user.token);

                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError)
                {
                    if (attempts == 0)
                    {
                        result.Add(ProcessState.NetworkError, "Network Error");
                        Debug.Log("Network Error");
                        callback(result);
                        continue;
                    }
                }
                else if (uwr.responseCode == 200)
                {
                    string jsonString = uwr.downloadHandler.text;
                    result.Add(ProcessState.Successful, jsonString);
                    Debug.Log("Successfull");
                    callback(result);
                    break;
                }
                else
                {
                    if (attempts == 0)
                    {
                        result.Add(ProcessState.NetworkError, "Network Error");
                        Debug.Log($"Other responce: {uwr.downloadHandler.text}");
                        callback(result);
                        continue;
                    }
                }

                yield return new WaitForSeconds(1f);
            }

        }


        public static IEnumerator leftRoom(int attempts, string roomSessionId, Action<ProcessState> callback)
        {

            while (attempts > 0)
            {
                attempts--;

                string url = AnalyticsAPIHost + $"leftRoom/{roomSessionId}";

                Debug.Log("url = " + url);

                var result = ProcessState.None;

                UnityWebRequest uwr = UnityWebRequest.Get(url);

                uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                uwr.SetRequestHeader("Authorization", Repo.loginData.user.token);

                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError)
                {
                    if (attempts == 0)
                    {
                        result = ProcessState.NetworkError;
                        Debug.Log("Network Error");
                        callback(result);
                        continue;
                    }
                }
                else if (uwr.responseCode == 200)
                {
                    result = ProcessState.Successful;
                    Debug.Log("Successfull");
                    callback(result);
                    break;
                }
                else
                {
                    if (attempts == 0)
                    {
                        result = ProcessState.NetworkError;
                        Debug.Log($"Other, response : {uwr.downloadHandler.text}");
                        callback(result);
                        continue;
                    }
                }

                yield return new WaitForSeconds(1f);
            }

        }


        public static IEnumerator CheckConnectivity(int attempts, Action<ProcessState, string> response)
        {
            while (attempts > 0)
            {
                attempts--;
                print("Checking Connectivity");

                UnityWebRequest www = UnityWebRequest.Get("https://www.google.com");
                yield return www.SendWebRequest();

                if (www.isNetworkError)
                {
                    if (attempts == 0)
                    {
                        response(ProcessState.Failed, "No Internet");
                        continue;
                    }
                }
                else if (www.responseCode == 200)
                {
                    response(ProcessState.Successful, "Internet Available");
                    break;
                }
                www.Dispose();
                yield return new WaitForSeconds(1f);
            }
        }

        public static IEnumerator GetTexture(string link, Action<Texture2D> texture)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(link);

            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                texture?.Invoke(((DownloadHandlerTexture)www.downloadHandler).texture);
            }
        }

        public static IEnumerator signIn(string email, string password, Action<Dictionary<ProcessState, string>> callback)
        {

            Dictionary<ProcessState, string> result = new Dictionary<ProcessState, string>();

            string url = APIHost + "api/login";
            Debug.Log("url = " + url);


            //Debug.Log(www.text);

            WWWForm form = new WWWForm();

            form.AddField("email", email);
            form.AddField("password", Hash.getHashSha256(password));
            Debug.Log(Hash.getHashSha256(password));
            //form.headers.Add("Authorization", "Token 93745b9719cf490b1c16fbb0be37151e00d926f7d4a8ddde69829e2deb88fb43");

            UnityWebRequest uwr = UnityWebRequest.Post(url, form);

            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                result.Clear();
                result.Add(ProcessState.NetworkError, "Network Error");

            }
            else if (uwr.responseCode == 200)
            {
                string jsonString = uwr.downloadHandler.text;
                result.Add(ProcessState.Successful, jsonString);
            }
            else if (uwr.responseCode == 417)
            {
                result.Clear();
                result.Add(ProcessState.PassFailed, "Password is incorrect");

            }
            else if (uwr.responseCode == 404)
            {
                result.Clear();
                result.Add(ProcessState.UnameFailed, "User not found");
            }
            else
            {
                result.Clear();
                result.Add(ProcessState.Failed, "Unknown Error");
            }

            callback(result);
        }

        public static IEnumerator updateAvatar(string avatarData, Action<ProcessState> callback)
        {
            string url = APIHost + "api/updateProfile/" + Repo.loginData.user.email;
            Debug.Log("url = " + url);

            var result = ProcessState.None;

            WWWForm form = new WWWForm();

            Debug.Log("avatar data: " + avatarData);

            //form.headers.Add("Authorization", Repo.loginData.user.token);

            form.AddField("avatar", avatarData);


            UnityWebRequest uwr = UnityWebRequest.Post(url, form);

            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            uwr.SetRequestHeader("Authorization", Repo.loginData.user.token);

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                result = ProcessState.NetworkError;
                Debug.Log("Network Error");
            }
            else if (uwr.responseCode == 200)
            {
                result = ProcessState.Successful;
                Debug.Log("Successfull");
            }
            else
            {
                result = ProcessState.ServerError;
                Debug.Log("Maybe unauthorized");
            }

            callback(result);
        }

        public static IEnumerator createEvent(string eventName, string placeTitle, Action<Dictionary<ProcessState, string>> callback)
        {
            Dictionary<ProcessState, string> result = new Dictionary<ProcessState, string>();

            string url = APIHost + "api/createEvent";
            Debug.Log("url = " + url);
            //yield return null;

            WWWForm form = new WWWForm();

            form.AddField("eventName", eventName);
            form.AddField("placeTitle", placeTitle);

            UnityWebRequest uwr = UnityWebRequest.Post(url, form);

            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            uwr.SetRequestHeader("Authorization", Repo.loginData.user.token);

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                result.Add(ProcessState.NetworkError, "Network Error");
            }
            if (uwr.responseCode == 200)
            {
                result.Add(ProcessState.Successful, uwr.downloadHandler.text);
            }
            else
            {
                result.Add(ProcessState.ServerError, "Server Error");
            }

            callback(result);

        }

        public static IEnumerator getEvents(Action<Dictionary<ProcessState, string>> callback)
        {
            Dictionary<ProcessState, string> result = new Dictionary<ProcessState, string>();

            string url = APIHost + "api/getEventList";
            Debug.Log("url = " + url);

            WWWForm form = new WWWForm();

            UnityWebRequest uwr = UnityWebRequest.Post(url, form);

            uwr.SetRequestHeader("Authorization", Repo.loginData.user.token);
            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                result.Add(ProcessState.NetworkError, "Network Error");
            }
            if (uwr.responseCode == 200)
            {
                result.Add(ProcessState.Successful, uwr.downloadHandler.text);
            }
            else
            {
                result.Add(ProcessState.ServerError, "Server Error");
            }

            callback(result);

        }

        public static IEnumerator deleteEvent(string eventCode, Action<Dictionary<ProcessState, string>> callback)
        {
            Dictionary<ProcessState, string> result = new Dictionary<ProcessState, string>();

            string url = APIHost + "api/deleteEvent/" + eventCode;
            Debug.Log("url = " + url);

            WWWForm form = new WWWForm();

            form.AddField("eventCode", eventCode);

            UnityWebRequest uwr = UnityWebRequest.Delete(url);

            uwr.SetRequestHeader("Authorization", Repo.loginData.user.token);
            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                result.Add(ProcessState.NetworkError, "Network Error");
            }
            if (uwr.responseCode == 200)
            {
                result.Add(ProcessState.Successful, "Deleted");
            }
            else
            {
                result.Add(ProcessState.ServerError, "Server Error");
            }

            callback(result);

        }

        public static IEnumerator versionControl(Action<Dictionary<ProcessState, string>> callback)
        {
            Dictionary<ProcessState, string> result = new Dictionary<ProcessState, string>();

            string url = APIHost + "api/getVersions/" + Application.companyName;
            Debug.Log(url);

            UnityWebRequest uwr = UnityWebRequest.Get(url);

            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            uwr.SetRequestHeader("Authorization", Repo.loginData.user.token);

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError)
            {
                result.Add(ProcessState.NetworkError, "Network Error");
                Debug.Log("Network Error");
            }
            else if (uwr.responseCode == 200)
            {
                result.Add(ProcessState.Successful, uwr.downloadHandler.text);
                Debug.Log("Successfull");
            }
            else if (uwr.responseCode == 401)
            {
                result.Add(ProcessState.Failed, "Unauthorized");
                Debug.Log("Unauthorized");

            }
            else
            {
                result.Add(ProcessState.ServerError, "Server Error");
                Debug.Log("Server error " + uwr.responseCode + uwr.downloadHandler.text);
            }

            callback(result);
        }


    }
}