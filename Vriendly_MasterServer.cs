using MasterServerToolkit.Networking;
using MasterServerToolkit.MasterServer;

public class Vriendly_MasterServer : MasterServerBehaviour
{

    protected override void Start()
    {
        MstTimer.WaitForEndOfFrame(() =>
        {
            StartServer();
        });
    }

}
