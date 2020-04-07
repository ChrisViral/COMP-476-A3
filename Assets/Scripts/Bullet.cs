using Photon.Pun;
using UnityEngine;

namespace COMP476A3
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Bullet : MonoBehaviourPun
    {
        #region Constants
        /// <summary>
        /// The layer upon which obstacles lie
        /// </summary>
        private const int OBSTACLE_LAYER = 8;
        #endregion

        #region Fields
        [SerializeField]
        private float speed = 5;
        [SerializeField]
        private int damage = 5;
        [SerializeField]
        private GameObject explosion;
        #endregion

        #region Functions
        private void Awake()
        {
            if (!PhotonNetwork.IsConnected || this.photonView.IsMine)
            {
                //Only set velocity if owning the object
                GetComponent<Rigidbody>().velocity = this.transform.forward * this.speed;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //Check for obstacle and destroy them
            if (other.gameObject.layer == OBSTACLE_LAYER)
            {
                Destructible destructible = other.GetComponent<Destructible>();
                if (destructible)
                {
                    if (PhotonNetwork.IsConnected)
                    {
                        if (this.photonView.IsMine)
                        {
                            //Networked destruction
                            destructible.photonView.RPC(nameof(Destructible.DestroyThis), RpcTarget.All);
                        }
                    }
                    else
                    {
                        destructible.DestroyThis();
                    }
                }
            }
            else
            {
                //Otherwise check for enemy tanks
                TankControl tank = other.GetComponent<TankControl>();
                if (tank)
                {
                    if (PhotonNetwork.IsConnected)
                    {
                        if (this.photonView.IsMine)
                        {
                            tank.photonView.RPC(nameof(TankControl.TakeDamage), RpcTarget.All, this.damage);
                        }
                    }
                    else
                    {
                        tank.TakeDamage(this.damage);
                    }
                }
            }

            //Destroy the bullet
            if (PhotonNetwork.IsConnected)
            {
                if (this.photonView.IsMine)
                {
                    PhotonNetwork.Instantiate(this.explosion.name, this.transform.position, Quaternion.identity);
                    PhotonNetwork.Destroy(this.gameObject);
                }
            }
            else
            {
                Instantiate(this.explosion, this.transform.position, Quaternion.identity);
                Destroy(this.gameObject);
            }
        }
        #endregion
    }
}
