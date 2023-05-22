using UnityEngine;

using Unity.Collections;
using Unity.Networking.Transport;

using Utils;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Core.Character;
using Core.Character.InputSpace;

namespace Core.Server
{
    public class ClientBehaviour : MonoBehaviour
    {
        private static ClientBehaviour _instance;

        [SerializeField]
        private GameObject _playerPrefab;
        [SerializeField]
        private int _sendRate = 10;
        [SerializeField]
        private string _ip = "127.0.0.1";
        [SerializeField]
        private ushort _port = 9000;

        private readonly Dictionary<int, CharacterMover> _players = new();

        private NetworkDriver _driver;
        private NetworkConnection _connection;

        [SerializeField]
        [ShowOnly]
        private int _roomIndex;

        [SerializeField]
        [ShowOnly]
        private int _playerIndex;

        private CharacterMover _localPlayer;

        public static void StartSending(CharacterMover player) =>
            _instance.StartCoroutine(_instance.Sending(player));
        private IEnumerator Sending(CharacterMover player)
        {
            while (true)
            {
                yield return new WaitForSeconds(1f / _sendRate);

                var data = Converter.ToByteArray(player.transform.position.x)
                    .Concat(Converter.ToByteArray(player.transform.position.y))
                    .Concat(Converter.ToByteArray(player.transform.position.z))
                    .Concat(Converter.ToByteArray(player.TargetPos.x))
                    .Concat(Converter.ToByteArray(player.TargetPos.y))
                    .Concat(Converter.ToByteArray(player.TargetPos.z));

                if (_instance != null)
                    _instance.SendData(data.ToArray());
            }
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
                    var playerGO = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
                    _localPlayer = playerGO.GetComponent<CharacterMover>();
                    StartSending(_localPlayer);
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

                    GetData(value.ToArray());
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

        private void GetData(byte[] data)
        {
            var kegleIndex = 4;
            while (kegleIndex < data.Length)
            {
                var rawData = data.Skip(kegleIndex).Take(4).ToArray();
                var playerIndex = Converter.FromByteArray<int>(rawData);
                kegleIndex += 4;

                rawData = data.Skip(kegleIndex).Take(4).ToArray();
                var playerPosX = Converter.FromByteArray<float>(rawData);
                kegleIndex += 4;

                rawData = data.Skip(kegleIndex).Take(4).ToArray();
                var playerPosY = Converter.FromByteArray<float>(rawData);
                kegleIndex += 4;

                rawData = data.Skip(kegleIndex).Take(4).ToArray();
                var playerPosZ = Converter.FromByteArray<float>(rawData);
                kegleIndex += 4;

                rawData = data.Skip(kegleIndex).Take(4).ToArray();
                var targetPosX = Converter.FromByteArray<float>(rawData);
                kegleIndex += 4;

                rawData = data.Skip(kegleIndex).Take(4).ToArray();
                var targetPosY = Converter.FromByteArray<float>(rawData);
                kegleIndex += 4;

                rawData = data.Skip(kegleIndex).Take(4).ToArray();
                var targetPosZ = Converter.FromByteArray<float>(rawData);
                kegleIndex += 4;

                var playerPos = new Vector3(playerPosX, playerPosY, playerPosZ);
                var targetPos = new Vector3(targetPosX, targetPosY, targetPosZ);

                if (playerIndex == _playerIndex)
                {
                    if (!_players.ContainsKey(playerIndex))
                        _players[_playerIndex] = _localPlayer;
                }
                else
                {
                    if (!_players.ContainsKey(playerIndex))
                    {
                        var playerGO = Instantiate(_playerPrefab, playerPos, Quaternion.identity);
                        _players[playerIndex] = playerGO.GetComponent<CharacterMover>();

                        _players[playerIndex].transform.position = playerPos;
                        playerGO.GetComponent<PlayerInput>().enabled = false;
                    }

                    _players[playerIndex].SetInputs(playerPos, targetPos, false);
                }
            }
        }
    }
}
