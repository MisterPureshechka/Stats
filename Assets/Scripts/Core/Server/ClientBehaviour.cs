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

        private const ushort _port = 9000;
        private NetworkDriver _driver;
        private NetworkConnection _connection;
        private bool _done;

        private void Start() 
        {
            _driver = NetworkDriver.Create(new WebSocketNetworkInterface());
            _connection = default;

            var endpoint = NetworkEndpoint.Parse(_ip, _port);
            _connection = _driver.Connect(endpoint);
            _logTextArea.text += "\nCLIENT: " + endpoint.Address;
        }

        private void OnDestroy() 
        {
            _driver.Dispose();
        }

        private void Update() 
        {
            _driver.ScheduleUpdate().Complete();

            if (!_connection.IsCreated)
            {
                if (!_done)
                    _logTextArea.text += "\nCLIENT: " + "Something went wrong during connect";

                return;
            }

            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = _connection.PopEvent(_driver, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    var value = Converter.ToByteArray(-1).Concat(Converter.ToByteArray(-1)).Concat(Converter.ToByteArray(Random.Range(5,15))).ToArray();
                    _driver.BeginSend(_connection, out var writer);
                    writer.WriteBytes(new NativeArray<byte>(value, Allocator.Persistent));
                    _driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    var rawValue = new byte[stream.Length];
                    var value = new NativeArray<byte>(rawValue, Allocator.Persistent);
                    stream.ReadBytes(value);

                    _logTextArea.text += "\nCLIENT: " + "Got the room index " + Converter.FromByteArray<int>(value.Take(4).ToArray()) + " back from the server";
                    _logTextArea.text += "\nCLIENT: " + "Got the player index " + Converter.FromByteArray<int>(value.Skip(4).Take(4).ToArray()) + " back from the server";

                    _done = true;
                    _connection.Disconnect(_driver);
                    _connection = default;
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    _logTextArea.text += "\nCLIENT: " + "Client got disconnected from server";
                    _connection = default;
                }
            }
        }
    }
}

