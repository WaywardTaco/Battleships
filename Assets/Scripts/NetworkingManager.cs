using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkingManager : MonoBehaviour
{
    public void StartServer(int portNumber){

    }

    public void ClientConnectToServer(string hostName, int portNumber){
        
    }


    public static NetworkingManager Instance { get; private set;} = null;
    void Start()
    {
        if(Instance == null) Instance = this;
        else Destroy(this);
    }
    void OnDestroy()
    {
        if(Instance == this)
            Instance = null;
    }
}
