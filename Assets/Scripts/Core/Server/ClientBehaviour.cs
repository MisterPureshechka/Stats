using UnityEngine;

using Unity.Collections;
using Unity.Networking.Transport;

using System.Linq;
using UnityEngine.UI;
using Utils;

namespace Core.Server
{
    public class ClientBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Text _logTextArea;
        [SerializeField]
        private string _ip = "127.0.0.1";
        [SerializeField]
        private ushort _port = 9000;

        private NetworkDriver _driver;
        private NetworkConnection _connection;

        [SerializeField]
        [ShowOnly]
        private int _roomIndex;
        [SerializeField]
        [ShowOnly]
        private int _playerIndex;

        private void OnEnable() 
        {
            _roomIndex = -1;
            _playerIndex = -1;

            _driver = NetworkDriver.Create(new WebSocketNetworkInterface());
            _connection = default;

            var endpoint = NetworkEndpoint.Parse(_ip, _port);
            _connection = _driver.Connect(endpoint);
            _logTextArea.text += "\nCLIENT: " + endpoint.Address;
        }

        private void OnDisable()
        {
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
                    _logTextArea.text += "\nCLIENT: " + "Client got connected to server";
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

                    _logTextArea.text += "\nCLIENT: " + "Got the room index " + _roomIndex;
                    _logTextArea.text += "\nCLIENT: " + "Got the player index " + _playerIndex;
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    _logTextArea.text += "\nCLIENT: " + "Client got disconnected from server";
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
