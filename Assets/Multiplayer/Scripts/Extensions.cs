using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer
{
    public static class Debugger
    {
        private static Text _console;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnStart()
        {
            _console = GameObject.FindObjectsOfType<Text>().FirstOrDefault(t => t.name == "console");
        }
        public static void Log(object message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
        _console.text += message;
#endif
        }
        public static byte[] SerializePlayerData(object data)
        {
            var player = (PlayerData)data;
            var array = new List<byte>(16);
            array.AddRange(BitConverter.GetBytes(player.posX));
            array.AddRange(BitConverter.GetBytes(player.posZ));
            array.AddRange(BitConverter.GetBytes(player.rotY));
            array.AddRange(BitConverter.GetBytes(player.helth));

            return array.ToArray();
        }
        public static object DeserializePlayerData(byte[] data)
        {
            return new PlayerData
            {
                posX = BitConverter.ToSingle(data, 0),
                posZ = BitConverter.ToSingle(data, 4),
                rotY = BitConverter.ToSingle(data, 8),
                helth = BitConverter.ToSingle(data, 12)

            };
        }

        public static byte[] SerializeProjectileData(object data)
        {
            var bullet = (ProjectileData)data;
            var array = new List<byte>(12);
            array.AddRange(BitConverter.GetBytes(bullet.posX));
            array.AddRange(BitConverter.GetBytes(bullet.posZ));
            array.AddRange(BitConverter.GetBytes(bullet.rotY));

            return array.ToArray();
        }
        public static object DeserializeProjectileData(byte[] data)
        {
            return new ProjectileData
            {
                posX = BitConverter.ToSingle(data, 0),
                posZ = BitConverter.ToSingle(data, 4),
                rotY = BitConverter.ToSingle(data, 8)
            };
        }
    }
    public struct PlayerData
    {
        public float posX;
        public float posZ;
        public float rotY;
        public float helth;

        public static PlayerData Create(PlayerController player)
        {
            return new PlayerData
            {
                posX = player.transform.position.x,
                posZ = player.transform.position.z,
                rotY = player.transform.eulerAngles.y,
                helth = player.Health
            };
        }

        public void Set(PlayerController player)
        {
            var vector = player.transform.position;
            vector.x = posX;
            vector.z = posZ;
            player.transform.position = vector;

            vector = player.transform.eulerAngles;
            vector.y = rotY;
            player.transform.eulerAngles = vector;

            player.Health = helth;
        }
    }
    public struct ProjectileData
    {
        public float posX;
        public float posZ;
        public float rotY;

        public static ProjectileData Create(ProjectileController bullet)
        {
            return new ProjectileData
            {
                posX = bullet.transform.position.x,
                posZ = bullet.transform.position.z,
                rotY = bullet.transform.eulerAngles.y,
            };
        }

        public void Set(ProjectileController bullet)
        {
            var vector = bullet.transform.position;
            vector.x = posX;
            vector.z = posZ;
            bullet.transform.position = vector;

            vector = bullet.transform.eulerAngles;
            vector.y = rotY;
            bullet.transform.eulerAngles = vector;
        }
    }
}

