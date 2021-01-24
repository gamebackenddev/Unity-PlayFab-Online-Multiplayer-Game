using System.Collections.Generic;
using UnityEngine;
using Mirror;
using PlayFab.MultiplayerModels;
using PlayFab;
using PlayFab.ClientModels;

/*
 *  Simple client code for PlayFab hosting - (c) gamebackend.dev
 */

public class PlayFabClient : NetworkManager
{
    private string PlayFabId;
    GameObject settings;

    void Start()
    {
        settings = GameObject.Find("SETTINGS");
        Authenticate();
    }

    void Authenticate()
    {
        var request = new LoginWithCustomIDRequest { CustomId = settings.GetComponent<Settings>().playerName, CreateAccount = true };
        request.TitleId = settings.GetComponent<Settings>().titleId;
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult obj)
    {
        Debug.Log("Login with CustomID succeeded. Your PlayFabID is " + obj.PlayFabId);

        this.PlayFabId = obj.PlayFabId;

        if (!settings.GetComponent<Settings>().isLocalServer)
        {
            RequestMultiplayerServer();
        }
        else
        {
            this.StartClient();
            NetworkClient.RegisterHandler<PlayerInfo>(OnReceivePlayerInfo);
        }
    }


    public void OnReceivePlayerInfo(PlayerInfo netMsg)
    {
        NetworkClient.connection.Send<PlayerInfo>(new PlayerInfo
        {
            PlayFabId = this.PlayFabId,
            ConnectionId = netMsg.ConnectionId

        });
    }

    private void OnLoginFailure(PlayFabError obj)
    {
        Debug.Log("Login with CustomID failed.");
    }


    private void RequestMultiplayerServer()
    {
        RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();

        requestData.BuildId = settings.GetComponent<Settings>().buildId;
        requestData.PreferredRegions = new List<string>() { "NorthEurope" };
        requestData.SessionId = settings.GetComponent<Settings>().sessionId;
        if (requestData.SessionId.Equals(""))
        {
            requestData.SessionId = System.Guid.NewGuid().ToString();
        }

        PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
    }


    private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
    {
        this.networkAddress = response.IPV4Address;
        this.GetComponent<TelepathyTransport>().port = (ushort)response.Ports[0].Num;
        this.StartClient();
    }

    private void OnRequestMultiplayerServerError(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
    }


}
public struct PlayerInfo : NetworkMessage
{
    public string PlayFabId;
    public int ConnectionId;

}