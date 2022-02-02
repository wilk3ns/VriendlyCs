using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Vriendly.Backend.Repository;

namespace Vriendly.Utilities
{
    public class NetworkResponse
    {
        public const string Error = "error";
        public const string Success = "success";
    }


    public static class IPUtilities
    {
        public async static void GetMasterIp(int buildNumber, Action<string> onGotIP)
        {
            await WaitUntil(() => Repo.vcData != null);
            foreach (Backend.Models.Version version in Repo.vcData.versions)
            {
                if (version.buildNumber == buildNumber && version.version == Application.version)
                {
                    onGotIP?.Invoke(version.masterServerIP);
                }
            }
            await Task.Delay(1000);
        }

        private async static Task WaitUntil(Func<bool> done)
        {
            while (!done.Invoke()) 
            {
                await Task.Delay(500);
            }
        }

    }
}
