using Photon.Pun;
using UnityEngine;

namespace COMP476A3
{
    [DisallowMultipleComponent]
    public class Destructible : MonoBehaviourPun
    {
        #region Methods
        /// <summary>
        /// Destroys the GameObject this script is attached to
        /// </summary>
        [PunRPC]
        public void DestroyThis() => Destroy(this.gameObject);
        #endregion
    }
}
