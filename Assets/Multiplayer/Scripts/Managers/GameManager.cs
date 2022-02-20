using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Multiplayer.Managers
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        private PlayerController _player1, _player2;
        private Camera _cam1, _cam2;
        [SerializeField]
        private string _playerprefabname;
        [SerializeField]
        private InputAction _quit;
        [SerializeField, Range(1f, 15f)]
        private float _positionRange = 7f;
        public Canvas GameOverScreen;
        public Text GameOverText;
        public bool IsPlaying = false;

        public static GameManager GM;
        void Start()
        {
            GM = this;
            GameOverScreen.enabled = false;
            _quit.Enable();
            _quit.performed += OnQuit;
            var pos = new Vector3(Random.Range(-_positionRange, _positionRange), 0f, Random.Range(-_positionRange, _positionRange));
            var GO = PhotonNetwork.Instantiate(_playerprefabname + PhotonNetwork.NickName, pos, new Quaternion());

            PhotonPeer.RegisterType(typeof(PlayerData), 0, Debugger.SerializePlayerData, Debugger.DeserializePlayerData);
            PhotonPeer.RegisterType(typeof(ProjectileData), 0, Debugger.SerializeProjectileData, Debugger.DeserializeProjectileData);

            IsPlaying = true;
        }

        public void AddPlayer(PlayerController player)
        {
            if (player.name.Contains("1"))
            {
                _player1 = player;
                _cam1 = player.gameObject.GetComponentInChildren<Camera>();
            }
            else
            {
                _player2 = player;
                _cam2 = player.gameObject.GetComponentInChildren<Camera>();
            }
            if(_player1 != null && _player2 != null)
            {
                if(player == _player1)
                {
                   _cam1.enabled = false;
                    _cam2.enabled = true;
                }
                else
                { 
                    _cam1.enabled = true;
                    _cam2.enabled = false;
                }
                _player1.SetTarget(_player2.transform);
                _player2.SetTarget(_player1.transform);
            }
        }
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("NetMenuScene");
        }
        public static IEnumerator GameOver(string loser_name)
        {
            GM.IsPlaying = false;
            GM._player1.DisableControls();
            GM._player2.DisableControls();
            GM.GameOverText.text = (loser_name == PhotonNetwork.NickName)
                ? "Commiserations, player" + PhotonNetwork.NickName
                : "Congratulations, player" + PhotonNetwork.NickName;
            GM.GameOverScreen.enabled = true;
            yield return new WaitForSeconds(20);
            PhotonNetwork.LeaveRoom();
        }
       void OnQuit(InputAction.CallbackContext obj)
        {
            PhotonNetwork.LeaveRoom();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
            Application.Quit();
#endif
        }
    }
}