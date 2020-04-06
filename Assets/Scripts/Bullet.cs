using Photon.Pun;
using UnityEngine;

namespace COMP476A3
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Bullet : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private float speed = 5;
        [SerializeField]
        private GameObject explosion;
        #endregion

        #region Functions
        private void Awake() => GetComponent<Rigidbody>().velocity = this.transform.forward * this.speed;

        private void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Instantiate(this.explosion.name, this.transform.position, Quaternion.identity);
                PhotonNetwork.Destroy(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
                Instantiate(this.explosion, this.transform.position, Quaternion.identity);
            }
        }
        #endregion
    }
}
