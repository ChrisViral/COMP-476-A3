using Photon.Pun;

namespace COMP476A3
{
    public static class Extensions
    {
        /// <summary>
        /// Checks if an object is controllable by this machine or by the network
        /// </summary>
        /// <param name="mb">MonoBehaviour to check</param>
        /// <returns>True if the MonoBehaviour is locally controlled, false if it's networked</returns>
        public static bool IsControllable(this MonoBehaviourPun mb) => mb.photonView.IsMine || !PhotonNetwork.IsConnected;
    }
}
