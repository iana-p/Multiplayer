using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Multiplayer.Managers;

namespace Multiplayer
{
    public class PlayerController : MonoBehaviour, IPunObservable
    {

        public static GameManager GM;

        private Controls _controls;
        private Transform _bulletPool;
        [SerializeField]
        private Rigidbody _rigidBody;
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private ProjectileController _bulletPrefab;
        [SerializeField]
        private PhotonView _photonView;

        
        [Space, SerializeField, Range(1f, 10f)]
        private float _moveSpeed = 8f;
        [SerializeField, Range(0.5f, 10f)]
        private float _maxSpeed = 2f;

        [Space, Range(1f, 10f)]
        public float Health = 5f;

        [Space, SerializeField, Range(0.1f, 0.5f)]
        private float _attackDelay = 0.4f;
        [Space, SerializeField, Range(0.1f, 0.5f)]
        private float _rotateDelay = 0.2f;

        [Space, SerializeField]
        private Vector3 _firePoint;
        void Start()
        {
            GM = FindObjectOfType<GameManager>();
            _rigidBody = GetComponent<Rigidbody>();
            _bulletPool = FindObjectOfType<UnityEngine.EventSystems.EventSystem>().transform;
            _controls = new Controls();
            FindObjectOfType<Managers.GameManager>().AddPlayer(this);
        }
        private IEnumerator Fire()
        {
            while(true)
            {
                if (!_photonView.IsMine) break;
                var bullet = PhotonNetwork.Instantiate("Bullet", transform.TransformPoint(_firePoint), transform.rotation);
                bullet.GetComponent<ProjectileController>().Parent = name;
             
                bullet.transform.position = transform.TransformPoint(_firePoint);
                bullet.transform.rotation = transform.rotation;
                
                yield return new WaitForSeconds(_attackDelay);
            }
        }
        private IEnumerator Focus()
        {
            while(true)
            {
                transform.LookAt(_target);
                transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
                yield return new WaitForSeconds(_rotateDelay);
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(_firePoint, 0.2f);
        }
        void FixedUpdate()
        {
              if(Health <= 0)
            {
                _photonView.RPC("GameOverRPC", RpcTarget.All, PhotonNetwork.NickName);
            }
            var direction = _controls.Player1.Movement.ReadValue<Vector2>();

            if (direction.x == 0 && direction.y == 0) return;
            var globaldirection = transform.TransformDirection(direction);  
            var velocity = _rigidBody.velocity;
            var globalvelocity = transform.TransformDirection(velocity);
            globalvelocity += new Vector3(globaldirection.x, 0f, globaldirection.y) * _moveSpeed * Time.fixedDeltaTime; 
            globalvelocity.y = 0;
            globalvelocity = Vector3.ClampMagnitude(globalvelocity, _maxSpeed);
            velocity = transform.InverseTransformDirection(globalvelocity);
            _rigidBody.velocity = velocity;

          
        }

       
        public void SetTarget(Transform target)
        {
            _target = target;
            if (!_photonView.IsMine) return;
            
            _controls.Player1.Enable();
           
            StartCoroutine(Fire());
            StartCoroutine(Focus());
        }
        private void OnTriggerEnter(Collider other)
        {
            var bullet = other.GetComponent<ProjectileController>();
            var wall = other.GetComponent<BoundComponent>();
            if (wall == null && bullet != null && bullet.Parent == name) return;
            if (wall != null)
            {
                Health = 0;
                return;
            }
            if (wall == null && bullet != null && bullet.Parent != name)
            {
                Health -= bullet.GetDamage;
                Destroy(other.gameObject);
            }
        }
        private void OnDestroy()
        {
            _controls.Player1.Disable();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.IsWriting)
            {
                stream.SendNext(PlayerData.Create(this));
            }
            else
            {
                ((PlayerData)stream.ReceiveNext()).Set(this);
            }
        }
 
        [PunRPC]
        public void GameOverRPC(string loser_name)
        {
           StartCoroutine(GameManager.GameOver(loser_name));
        }
        public void DisableControls()
        {
            _controls.Disable();
        }

    }
}