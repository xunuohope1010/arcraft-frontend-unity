using System.Collections;
using Microsoft.Azure.SpatialAnchors.Unity.Examples.Restful;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class Backend : MonoBehaviour
    {
        private string URL = "http://arcraft.eba-pmni9yyz.us-west-1.elasticbeanstalk.com/";
        #region Static Interface

        /// <summary>
        /// The Singleton
        /// </summary>
        private static Backend _instance;

        /// <summary>
        /// Get the singleton
        /// </summary>
        public static Backend Instance
        {
            get
            {
                if (_instance == null)
                    return null;
                else
                    return _instance;
            }
        }

        #endregion

        #region [Backend]

        private void Start()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(this.gameObject); //ensure the instance is the only instance in the game
            }
        }

        public void getPattern(string patternName)
        {
            StartCoroutine(RestfulGetCoroutine(URL+"?name="+patternName, OnString, OnString));
        }

        public void updateScore(ScoreRequest scoreRequest)
        {
            StartCoroutine(RestfulPostCoroutine(URL+"update", JsonUtility.ToJson(scoreRequest), OnString, OnString));
        }
        
        public void login(LoginRequest loginRequest)
        {
            StartCoroutine(RestfulPostCoroutine(URL+"login", JsonUtility.ToJson(loginRequest), OnString, OnString));
        }
        
        public void SignUp(SignUpRequest signUpRequest)
        {
            StartCoroutine(RestfulPostCoroutine(URL+"signup", JsonUtility.ToJson(signUpRequest), OnString, OnString));
        }
        
        private IEnumerator RestfulPostCoroutine(string url, string jsonData, UnityAction<string> callbackOnSuccess,
            UnityAction<string> callbackOnFail)
        {
            Debug.Log("jsonData: " + jsonData);
            using (UnityWebRequest www = UnityWebRequest.Put(url, jsonData))
            {
                www.method = UnityWebRequest.kHttpVerbPOST;
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    callbackOnFail?.Invoke(www.error);
                }
                else
                {
                    Debug.Log(www.downloadHandler.text);
                    callbackOnSuccess?.Invoke(www.downloadHandler.text);
                }
            }
        }


        /// <summary>
        /// Coroutine that handles communication with REST server.
        /// </summary>
        /// <returns>The coroutine.</returns>
        /// <param name="url">API url.</param>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        /// <typeparam name="T">Data Model Type.</typeparam>
        private IEnumerator RestfulGetCoroutine(string url, UnityAction<string> callbackOnSuccess,
            UnityAction<string> callbackOnFail)
        {
            var www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                callbackOnFail?.Invoke(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                ParseResponse(www.downloadHandler.text, callbackOnSuccess, callbackOnFail);
            }
        }

        /// <summary>
        /// This method finishes request process as we have received answer from server.
        /// </summary>
        /// <param name="data">Data received from server in JSON format.</param>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        /// <typeparam name="T">Data Model Type.</typeparam>
        private void ParseResponse<T>(string data, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail)
        {
            if (data == "OK") return;
            var parsedData = JsonUtility.FromJson<T>(data);
            callbackOnSuccess?.Invoke(parsedData);
        }

        #endregion


        /// <summary>
        /// A un-implemented function for call back
        /// </summary>
        private void OnString(string ans)
        {
            return;
        }
    }
}