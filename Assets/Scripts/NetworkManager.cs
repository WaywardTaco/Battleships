using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private Socket _connectedServer;
    private Socket _listener;
    private Socket _connectedClient;
    private const string SYN = "<|SYN|>";
    private const string SYN_ACK = "<|SYN_ACK|>";
    private const string ACK = "<|ACK|>";
    private const string EOM = "<|EOM|>";

    private const int CONNECTION_TIMEOUT_LIMIT = 60000;

    private bool _isServer = false;

    public void CleanupConnections(){
        _isServer = false;
        _connectedServer = null;
        _listener = null;
    }

    public void StartServer(int portNumber){
        // IPEndPoint endpoint = new(, portNumber);

        // _listener = new()

        RunServer();
    }

    private async Task RunServer(){
        _connectedClient = await _listener.AcceptAsync();
        while (true){
            string response = await ReadMessage();
            ProcessServerReceive(response);
        }
    }

    private async Task RunClient(){
        _connectedClient = await _listener.AcceptAsync();
        while (true){
            string response = await ReadMessage();
            ProcessServerReceive(response);
        }
    }

    private void ProcessServerReceive(string message){

    }

    public async Task<bool> ConnectBattle(string hostname, int portNumber){
        bool result = await EstablishConnection(hostname, portNumber, false);

        if(!result){
            Debug.LogWarning($"[SERVER-WARN]: Failed to start battle with {hostname} on Port {portNumber}");
            return result;
        }

        return true;
    }

    public void SendTeamToServer(){

    }

    public void SendMovesToServer(){

    }

    public void SendBattleStatusToClient(){

    }

    private async Task SendData(string data){
        var dataBytes = Encoding.UTF8.GetBytes(data);

        if(_isServer){
            _ = await _listener.SendAsync(dataBytes, SocketFlags.None);
            Debug.Log($"[NETWORK-DEBUG]: Sent message to client \"{data}\"");
        } else {
            _ = await _connectedServer.SendAsync(dataBytes, SocketFlags.None);
            Debug.Log($"[NETWORK-DEBUG]: Sent message to server \"{data}\"");
        }
    }

    private async Task<string> ReadMessage(){
        Socket handler;
        if(_isServer)   handler = _connectedClient;
        else            handler = _connectedServer;

        var buffer = new byte[1_024];
        var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
        var response = Encoding.UTF8.GetString(buffer, 0, received);

        if(response == ACK){
            Debug.Log("[NETWORK-DEBUG]: Received ACK message");
            return "";
        }

        if (response.IndexOf(EOM) <= -1){
            Debug.Log("[NETWORK-DEBUG]: Received invalid response, include <|EOM|> to message");
            return "";
        }

        response.Replace(EOM, "");

        return response;
    }

    private async Task<bool> EstablishConnection(string hostname, int portNumber, bool isServer){
        IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(hostname);
        IPAddress ipAddress = ipHostInfo.AddressList[1];
        IPEndPoint endpoint = new(ipAddress, portNumber);

        if(endpoint == null){
            Debug.LogWarning($"[NETWORK-WARN]: Could not connect to endpoint at {ipAddress} (Port: {portNumber})");
            return false;
        }

        Socket serverSocket = new(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        if(serverSocket == null){
            Debug.LogWarning($"[NETWORK-WARN]: Could not connect to socket at {ipAddress} (Port: {portNumber})");
            return false;
        }

        _isServer = isServer;

        if(!_isServer){
            /* Client specific code */
            await serverSocket.ConnectAsync(endpoint);
            _connectedServer = serverSocket;

            /* Exchange 3-way handshake */
            await SendData(SYN);
            int connectionAttemptStartTime = DateTime.Now.Millisecond;
            while(DateTime.Now.Millisecond - connectionAttemptStartTime < CONNECTION_TIMEOUT_LIMIT){
                if(await ReadMessage() == SYN_ACK){
                    Debug.Log("[NETWORK-DEBUG]: SYN acknowledged by server, ready to connect");
                    await SendData(ACK);
                    return true;
                }
            }

            Debug.LogWarning("[NETWORK-WARN]: Connection timeout!");
            return false;
        } else {
            /* Server specific code */
            serverSocket.Bind(endpoint);
            serverSocket.Listen(1);

            _connectedClient = await serverSocket.AcceptAsync();

            /* Await 3-way handshake */
            int connectionAttemptStartTime = DateTime.Now.Millisecond;
            while(DateTime.Now.Millisecond - connectionAttemptStartTime < CONNECTION_TIMEOUT_LIMIT){
                if(await ReadMessage() == SYN){
                    Debug.Log("[NETWORK-DEBUG]: Received Connection Request, sending SYN ACK");
                    await SendData(SYN_ACK);
                }
            }
        }

        return true;
    }

    public static NetworkManager Instance { get; private set;}
    async Task Start()
    {
        if(Instance == null){
            Instance = this;
        } else {
            Destroy(this);
        }

        var hostname = Dns.GetHostName();
        IPHostEntry localhost = await Dns.GetHostEntryAsync(hostname);
        IPAddress address = localhost.AddressList[0];

        Debug.Log($"[SERVER-DEBUG]: {hostname}, {localhost.HostName}, {address}, {localhost.AddressList[1]}");
    }
    void OnDestroy()
    {
        Instance = this;
    }
}
