using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace COMP476A3.Assets.Scripts
{
    /// <summary>
    /// Particle auto destroyer
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(ParticleSystem))]
    public class AutoDestroyParticle : MonoBehaviour
    {
        #region Functions
        //Set object to be destroyed as soon as the ParticleSystem has played one full cycle
        private void Start() => Destroy(this.gameObject, GetComponent<ParticleSystem>().main.duration);
        #endregion
    }
}
