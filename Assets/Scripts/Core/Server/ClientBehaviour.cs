using UnityEngine;

using Unity.Collections;
using Unity.Networking.Transport;

using System.Linq;
using Utils;
using System;

namespace Core.Server
{
    public class ClientBehaviour : MonoBehaviour
    {
        public static Action<byte[]> OnDataGetted;

        private static ClientBehaviour _instance;

        [SerializeField]
        private string _ip = "127.0.0.1";
        [SerializeField]
        private ushort _port = 9000;

        private NetworkDriver _driver;
        private NetworkConnection _connection;

        [SerializeField]
        [ShowOnly]
        private int _roomIndex;
        public int RoomIndex => _roomIndex;

        [SerializeField]
        [ShowOnly]
        private int _playerIndex;
        public int PlayerIndex => _playerIndex;

        public static void Send(params byte[] data)
        {
            if (_instance != null)
                _instance.SendData(data);
        }

        public static int GetPlayerIndex()
        {
            if (_instance == null)
                return -1;

            return _instance.PlayerIndex;
        }

        private void OnEnable() 
        {
            _instance = this;

            _roomIndex = -1;
            _playerIndex = -1;

            _driver = NetworkDriver.Create(new WebSocketNetworkInterface());
            _connection = default;

            var endpoint = NetworkEndpoint.Parse(_ip, _port);
            _connection = _driver.Connect(endpoint);
        }

        private void OnDisable()
        {
            _instance = null;

            if (_connection.IsCreated) 
            {
                _connection.Disconnect(_driver);
                _connection = default;
            }
            _driver.Dispose();
        }

        private void Update() 
        {
            _driver.ScheduleUpdate().Complete();
            if (!_connection.IsCreated)
                return;

            NetworkEvent.Type cmd;
            while ((cmd = _connection.PopEvent(_driver, out var stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    var rawValue = new byte[stream.Length];
                    var value = new NativeArray<byte>(rawValue, Allocator.Persistent);
                    stream.ReadBytes(value);

                    if (_roomIndex < 0)
                        _roomIndex = Converter.FromByteArray<int>(value.Take(4).ToArray());
                    if (_playerIndex < 0)
                        _playerIndex = Converter.FromByteArray<int>(value.Skip(4).Take(4).ToArray());

                    OnDataGetted.Invoke(value.ToArray());
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    _connection = default;
                }
            }
        }

        public void SendData(params byte[] data)
        {
            var roomIndex = Converter.ToByteArray(_roomIndex);
            var playerIndex = Converter.ToByteArray(_playerIndex);

            var value = roomIndex.Concat(playerIndex).Concat(data).ToArray();
            _driver.BeginSend(_connection, out var writer);
            writer.WriteBytes(new NativeArray<byte>(value, Allocator.Persistent));
            _driver.EndSend(writer);
        }
    }
}
