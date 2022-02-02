using MasterServerToolkit.MasterServer;
using System.Collections;
using UnityEngine;
using Vriendly.Global.Settings;

public class Vriendly_ConnectionHelper : ConnectionHelper<Vriendly_ConnectionHelper>
{
    private bool GotIP;
    private BackendGlobalSettings _backendGlobaSettings;

    protected override void Start()
    {
        _backendGlobaSettings = SettingsCollection.GetSettings<BackendGlobalSettings>();
        //Initialize();
        // If master IP is provided via cmd arguments or via Backend
    }

    public void Initialize()
    {
        if (Mst.Args.IsProvided(Mst.Args.Names.MasterIp))
            serverIP = Mst.Args.MasterIp;
        else
        {
            _backendGlobaSettings.GetMasterIP((ip) =>
            {
                serverIP = ip;
                GotIP = true;
            });
        }
        // If master port is provided via cmd arguments
        if (Mst.Args.IsProvided(Mst.Args.Names.MasterPort))
            serverPort = Mst.Args.MasterPort;
        StartCoroutine(WaitForIp());
    }

    public void Deinitialize()
    {
        GotIP = false;
        Connection.Disconnect();
    }

    public IEnumerator WaitForIp()
    {
        yield return new WaitUntil(() => GotIP);
        StartConnection();
    }

}
