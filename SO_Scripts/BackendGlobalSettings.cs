using System;
using System.Collections;
using UnityEngine;
using Vriendly.Utilities;

namespace Vriendly.Global.Settings
{
    /// <summary>
    /// GLobal Backend Settings for the whole project.
    /// All can be accessed only trough its <see cref="ScriptableObject"/> Instance
    /// </summary>
    [CreateAssetMenu(fileName = settingsName, menuName = "Vriendly/Settings/" + settingsName, order = 1)]
    public class BackendGlobalSettings : Settings
    {

        protected new const string settingsName = "Backend Global Settings";

        public int BuildNumber => _buildNumber;

        [SerializeField]
        private string _masterIp = "127.0.0.1";

        [SerializeField]
        private bool _getMasterIpFromBackend = false;


        [SerializeField]
        private int _buildNumber = 0;


        public void GetMasterIP(Action<string> onComplete)
        {
            if (_getMasterIpFromBackend)
            {
                IPUtilities.GetMasterIp(_buildNumber, (response) =>
                {
                    onComplete?.Invoke(response);
                });
            }
            else
            {
                onComplete?.Invoke(_masterIp);
            }
        }
    }

}
