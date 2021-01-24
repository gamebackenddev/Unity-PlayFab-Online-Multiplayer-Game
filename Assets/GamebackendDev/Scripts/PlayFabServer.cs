using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using PlayFab;
using PlayFab.MultiplayerAgent.Model;

/*
 *  Simple server code for PlayFab hosting - (c) gamebackend.dev
 */
public class PlayFabServer : NetworkManager
{
    private List<PlayerConnection> playerConnections = new List<PlayerConnection>();
    private List<ConnectedPlayer> connectedPlayers = new List<ConnectedPlayer>();

    string PlayFabId;

    void Start()
    {
        StartPlayFabAPI();
        this.StartServer();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerInfo>(OnReceivePlayerInfo);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        Debug.Log("Connected client to server, ConnectionId: " + conn.connectionId);

        playerConnections.Add(new PlayerConnection
        {
            ConnectionId = conn.connectionId
        });

        conn.Send<PlayerInfo>(new PlayerInfo
        {
            ConnectionId = conn.connectionId
        });
    }


    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("Client disconnected from server, ConnectionId: " + conn.connectionId);

        var playerConnection = playerConnections.Find(c => c.ConnectionId == conn.connectionId);

        connectedPlayers.Remove(playerConnection.ConnectedPlayer);
        playerConnections.Remove(playerConnection);

        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(connectedPlayers);

        if (playerConnections.Count == 0)
        {
            StartCoroutine(Shutdown());
        }
    }

    private void OnReceivePlayerInfo(PlayerInfo netMsg)
    {
        var playerConnection = playerConnections.Find(c => c.ConnectionId == netMsg.ConnectionId);
        playerConnection.ConnectedPlayer = new ConnectedPlayer(netMsg.PlayFabId);
        connectedPlayers.Add(playerConnection.ConnectedPlayer);
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(connectedPlayers);
    }


    void StartPlayFabAPI()
    {
        PlayFabMultiplayerAgentAPI.Start();
        StartCoroutine(ReadyForPlayers());
    }


    IEnumerator ReadyForPlayers()
    {
        yield return new WaitForSeconds(.5f);
        PlayFabMultiplayerAgentAPI.ReadyForPlayers();
    }

    private IEnumerator Shutdown()
    {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

}
public class PlayerConnection
{
    public ConnectedPlayer ConnectedPlayer;
    public int ConnectionId;
}