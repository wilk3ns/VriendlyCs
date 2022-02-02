using System;
using UnityEngine;
using Vriendly.Backend.Models;

namespace Vriendly.Backend.Repository

{
    public static class Repo
    {

        public static LoginDataRoot loginData;
        public static VersionDataRoot vcData;
        public static AnalyticsResponse sessionResponse,eventResponse;

        public static void fillLoginData(string data)
        {
            //loginData = JsonUtility.FromJson<Root>(data);
            loginData = JsonUtility.FromJson<LoginDataRoot>(data);
        }

        public static void fillVCData(string data)
        {
            vcData = JsonUtility.FromJson<VersionDataRoot>(data);
        }

        public static void filleventData(string data)
        {
            eventResponse = JsonUtility.FromJson<AnalyticsResponse>(data);
        }

        public static void fillsessionData(string data)
        {
            sessionResponse = JsonUtility.FromJson<AnalyticsResponse>(data);
        }
    }
}