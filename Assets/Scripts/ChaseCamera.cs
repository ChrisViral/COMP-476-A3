using System.Linq;
using UnityEngine;

namespace COMP476A3
{
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public class ChaseCamera : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private Vector3 offset = new Vector3(0f, 4f, -7f);
        [SerializeField]
        private float smoothTime = 0.3f;
        private Vector3 followVelocity = Vector3.zero;
        #endregion

        #region Properties
        private Transform target;
        public Transform Target
        {
            get => this.target;
            set
            {
                //Set the target if it is not null
                if (value)
                {
                    this.target = value;
                    this.transform.position = this.FollowTarget;
                    this.transform.LookAt(this.target);
                }
            }
        }

        /// <summary>
        /// Follow if the target is not null
        /// </summary>
        public bool IsFollowing => this.target;

        /// <summary>
        /// The position of the chase cam while following it's target
        /// </summary>
        private Vector3 FollowTarget => this.Target.position + this.Target.TransformVector(this.offset);
        #endregion

        #region Functions
        private void LateUpdate()
        {
            //Only follow if a follow target exists
            if (this.IsFollowing)
            {
                //Smooth movement to the target
                this.transform.position = Vector3.SmoothDamp(this.transform.position, this.FollowTarget, ref this.followVelocity, this.smoothTime);
                this.transform.LookAt(this.target);
            }
        }
        #endregion
    }
}
