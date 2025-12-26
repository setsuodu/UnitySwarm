using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

public class SimpleServer : MonoBehaviour, INetEventListener
{
    private NetManager _netManager;
    public NetPacketProcessor Processor { get; private set; } = new NetPacketProcessor();
    public Action<NetPeer> OnPlayerConnected;

    public void StartServer(int port)
    {
        _netManager = new NetManager(this) { AutoRecycle = true };
        _netManager.Start(port);
    }

    void Update() => _netManager?.PollEvents();

    public void SendToAll<T>(T packet, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : class, new()
    {
        NetDataWriter writer = new NetDataWriter();
        Processor.Write(writer, packet);
        _netManager.SendToAll(writer, method);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod delivery) => Processor.ReadAllPackets(reader, peer);
    public void OnConnectionRequest(ConnectionRequest request) => request.AcceptIfKey("ExampleGame");
    public void OnPeerConnected(NetPeer peer)
    {
        //Debug.Log($"Client is connected: Id={peer.Id}, Tag={peer.Tag}, Address={peer.Address}, RemoteId={peer.RemoteId}, ConnectionState={peer.ConnectionState}");
        OnPlayerConnected?.Invoke(peer);
    }

    // 其他接口留空...
    public void OnPeerDisconnected(NetPeer p, DisconnectInfo d) { }
    public void OnNetworkError(System.Net.IPEndPoint e, System.Net.Sockets.SocketError s) { }
    public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint e, NetPacketReader r, UnconnectedMessageType t) { }
    public void OnNetworkLatencyUpdate(NetPeer p, int l) { }
}