using UnityEngine;

namespace COMP476A3.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(Canvas))]
    public class AlignToCamera : MonoBehaviour
    {
        #region Fields
        private Canvas canvas;
        #endregion

        #region Functions
        private void Awake()
        {
            //Setup the camera
            this.canvas = GetComponent<Canvas>();
            this.canvas.worldCamera = Camera.main;
        }

        private void Update()
        {
            //ReSharper disable once PossibleNullReferenceException
            //Always point away from the camera
            this.canvas.transform.rotation = Quaternion.LookRotation(this.transform.position - Camera.main.transform.position);
        }
        #endregion
    }
}
