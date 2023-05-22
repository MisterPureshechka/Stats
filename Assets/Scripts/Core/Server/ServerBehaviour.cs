using UnityEngine;

using Unity.Collections;
using Unity.Networking.Transport;

using System.Linq;
using Utils;

namespace Core.Server
{
    public class ServerBehaviour : MonoBehaviour
    {
        [SerializeField]
        private string _ip = "127.0.0.1";
        [SerializeField]
        private ushort _port = 9000;
        [SerializeField]
        private int _maxRooms = 250;
        [SerializeField]
        private int _maxPlayersInRoom = 15;

        private NetworkDriver _driver;
        private NativeList<NetworkConnection> _connections;

        private byte[][][] _rooms;

        private void Awake()
        {
            _rooms = new byte[_maxRooms][][];
            for (var i = 0; i < _rooms.Length; i++)
                _rooms[i] = new byte[_maxPlayersInRoom][];

            _driver = NetworkDriver.Create(new WebSocketNetworkInterface());

            var endpoint = NetworkEndpoint.Parse(_ip, _port);
            if (_driver.Bind(endpoint) != 0)
                Debug.Log("SERVER: " + "Failed to bind to port " + _port);
            else
                _driver.Listen();

            Debug.Log("SERVER: " + endpoint.Address);
            _connections = new NativeList<NetworkConnection>(Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (!_driver.IsCreated)
                return;

            _driver.Dispose();
            _connections.Dispose();
        }

        private void Update()
        {
            _driver.ScheduleUpdate().Complete();

            for (int i = 0; i < _connections.Length; i++)
            {
                if (_connections[i].IsCreated)
                    continue;

                _connections.RemoveAtSwapBack(i);
                --i;
            }

            NetworkConnection c;
            while ((c = _driver.Accept()) != default)
            {
                _connections.Add(c);
                Debug.Log("SERVER: " + "Accepted a connection");
            }

            for (int i = 0; i < _connections.Length; i++)
            {
                if (!_connections[i].IsCreated)
                    continue;

                NetworkEvent.Type cmd;
                while ((cmd = _driver.PopEventForConnection(_connections[i], out var stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        Debug.Log("SERVER: " + "Received some data from client");
                        var rawGetData = new byte[stream.Length];
                        var getData = new NativeArray<byte>(rawGetData, Allocator.Persistent);
                        stream.ReadBytes(getData);

                        var rawRoomIndex = getData.Take(4).ToArray();
                        var roomIndex = Converter.FromByteArray<int>(rawRoomIndex);

                        var rawPlayerIndex = getData.Skip(4).Take(4).ToArray();
                        var playerIndex = Converter.FromByteArray<int>(rawPlayerIndex);

                        if (roomIndex < 0)
                        {
                            for (var roomI = 0; roomI < _rooms.Length; roomI++)
                            {
                                for (var playerI = 0; playerI < _rooms[roomI].Length; playerI++)
                                {
                                    if (_rooms[roomI][playerI] != null)
                                        continue;

                                    roomIndex = roomI;
                                    playerIndex = playerI;
                                    break;
                                }

                                if (roomIndex >= 0)
                                    break;
                            }

                            if (roomIndex < 0)
                                return;
                        }

                        _rooms[roomIndex][playerIndex] = getData.Skip(4).Skip(4).ToArray();

                        var rawSendData = Converter.ToByteArray(roomIndex)
                            .Concat(Converter.ToByteArray(playerIndex))
                            .Concat(_rooms[roomIndex][playerIndex]).ToArray();
                        for (var playerI = 0; playerI < _rooms[roomIndex].Length; playerI++)
                        {
                            if (playerI == playerIndex)
                                continue;

                            if (_rooms[roomIndex][playerI] == null)
                                continue;

                            rawSendData = rawSendData
                                .Concat(Converter.ToByteArray(playerI))
                                .Concat(_rooms[roomIndex][playerI]).ToArray();
                        }

                        var sendData = new NativeArray<byte>(rawSendData, Allocator.Persistent);
                        _driver.BeginSend(NetworkPipeline.Null, _connections[i], out var writer);
                        writer.WriteBytes(sendData);
                        _driver.EndSend(writer);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("SERVER: " + "Client disconnected from server");
                        _connections[i] = default;
                    }
                }
            }
        }
    }
}