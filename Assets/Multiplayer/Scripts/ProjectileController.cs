using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer
{
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField, Range(1f, 10f)]
        private float _moveSpeed = 3f;
        [SerializeField, Range(1f, 10f)]
        private float _damage = 1f;
        [SerializeField, Range(1f, 15f)]
        private float _lifetime = 7f;

        [SerializeField]
        private PhotonView _photonView;
        public string Parent { get; set; }
        public float GetDamage => _damage;
        void Start()
        {
            StartCoroutine(OnDeath());
        }

        void Update()
        {
           
            transform.position += transform.forward * _moveSpeed * Time.deltaTime;
        }

        private IEnumerator OnDeath()
        {
            yield return new WaitForSeconds(_lifetime);
            Destroy(gameObject);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(ProjectileData.Create(this));
            }
            else
            {
                ((ProjectileData)stream.ReceiveNext()).Set(this);
            }
        }
    }
}