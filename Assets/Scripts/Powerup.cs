using Photon.Pun;
using UnityEngine;

namespace COMP476A3
{
    [DisallowMultipleComponent]
    public class Powerup : MonoBehaviourPun
    {
        #region Methods
        /// <summary>
        /// Destroys this powerup from the owner
        /// </summary>
        [PunRPC]
        public void Remove()
        {
            if (this.photonView.IsMine)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
            else
            {
                this.photonView.RPC(nameof(Remove), RpcTarget.MasterClient);
            }

        }
        #endregion
    }
}
