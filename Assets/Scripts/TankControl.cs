using System;
using Photon.Pun;
using UnityEngine;

namespace COMP476A3
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class TankControl : MonoBehaviourPun
    {
        public enum Rotation
        {
            NONE  =  0,
            LEFT  = -1,
            RIGHT =  1
        }

        public enum Direction
        {
            UP    = 0,
            RIGHT = 90,
            DOWN  = 180,
            LEFT  = 270
        }

        #region Fields
        [SerializeField]
        private Transform[] leftWheels = new Transform[0];
        [SerializeField]
        private Transform[] rightWheels = new Transform[0];
        [SerializeField]
        private Renderer leftTrack;
        [SerializeField]
        private Renderer rightTrack;
        [SerializeField]
        private float rotateSpeed = 45f;
        [SerializeField]
        private float speed = 1f;
        [SerializeField]
        private float wheelSpeed = 110f;
        [SerializeField]
        private float trackSpeed = 0.35f;
        [SerializeField]
        private GameObject bullet;
        [SerializeField]
        private Vector3 bulletSpawn;

        public Rotation rotateDir;
        public Direction targetDirection;
        private new Rigidbody rigidbody;
        #endregion

        #region Static methods
        /// <summary>
        /// Clamps the passed angle between [0, 360[
        /// </summary>
        /// <param name="angle">Angle to clamp</param>
        /// <returns>the clamped value of the angle</returns>
        private static int ClampAngle(int angle) => angle < 0 ? angle + 360 : angle >= 360 ? angle - 360 : angle;
        #endregion

        #region Functions
        private void Awake() => this.rigidbody = GetComponent<Rigidbody>();

        private void FixedUpdate()
        {
            //Only do physics work if on client machine
            if (this.IsControllable() && this.rotateDir != Rotation.NONE)
            {
                //Rotation
                float target = (float)this.targetDirection;
                Quaternion rotation = this.rigidbody.rotation;
                Vector3 euler = rotation.eulerAngles;

                //Stop rotating when you reach the desired direction
                if (Mathf.Abs(Mathf.DeltaAngle(euler.y, target)) < 0.001f)
                {
                    this.rotateDir = Rotation.NONE;
                    this.rigidbody.angularVelocity = Vector3.zero;
                }
                else
                {
                    //Else calculate necessary angular velocity
                    float prevAngle = rotation.eulerAngles.y;
                    float newAngle = Quaternion.RotateTowards(rotation, Quaternion.Euler(0f, target, 0f), this.rotateSpeed * Time.fixedDeltaTime).eulerAngles.y;
                    this.rigidbody.angularVelocity = Vector3.up * ((Mathf.DeltaAngle(prevAngle, newAngle) * Mathf.Deg2Rad) / Time.fixedDeltaTime);
                }
            }
        }

        private void Update()
        {
            //Only catch input for client tanks
            if (this.IsControllable())
            {
                //Turn handling
                if (Input.GetButton("Left"))
                {
                    //Don't register turn inputs if both directions are pressed at once
                    if (Input.GetButton("Right")) { return; }

                    //Left turn
                    if (this.rotateDir != Rotation.LEFT)
                    {
                        this.rotateDir = Rotation.LEFT;
                        this.targetDirection = (Direction)ClampAngle((int)this.targetDirection - 90);
                        this.rigidbody.velocity = Vector3.zero;
                    }
                }
                else if (Input.GetButton("Right") && this.rotateDir != Rotation.RIGHT)
                {
                    //Right turn
                    this.rotateDir = Rotation.RIGHT;
                    this.targetDirection = (Direction)ClampAngle((int)this.targetDirection + 90);
                    this.rigidbody.velocity = Vector3.zero;
                }
                //Only move forwards or backwards if we are not turning
                else if (this.rotateDir == Rotation.NONE)
                {
                    this.rigidbody.velocity = this.transform.forward * (Input.GetAxis("Vertical") * this.speed);
                }

                //Bullet firing
                if (Input.GetButtonDown("Fire"))
                {
                    Vector3 spawnLocation = this.transform.position + this.transform.TransformVector(this.bulletSpawn);
                    if (PhotonNetwork.IsConnected)
                    {
                        PhotonNetwork.Instantiate(this.bullet.name, spawnLocation, this.rigidbody.rotation);
                    }
                    else
                    {
                        Instantiate(this.bullet, spawnLocation, this.rigidbody.rotation);
                    }
                }
            }

            //Get speed
            float currentSpeed;
            float rightWheelMod = 1f, rightTrackMod = 1f;
            float angularSpeed = this.rigidbody.angularVelocity.y * Mathf.Rad2Deg;

            if (Math.Abs(angularSpeed) > 0.01f)
            {
                //Turning speed
                currentSpeed = angularSpeed;
                rightTrackMod = -1f;
            }
            else
            {
                //Moving speed
                Vector3 velocity = this.rigidbody.velocity;
                currentSpeed = velocity.magnitude * (Vector3.Angle(this.transform.forward, velocity) > 90f ? -1f : 1f);
                rightWheelMod = -1f;
            }

            //If speed is significant enough
            if (Math.Abs(currentSpeed) > 0.01f)
            {
                //Set wheel rotation speed
                float rotationSpeed = currentSpeed * this.wheelSpeed * Time.deltaTime;
                foreach (Transform wheel in this.leftWheels)
                {
                    wheel.Rotate(Vector3.right, rotationSpeed);
                }
                rotationSpeed *= rightWheelMod;
                foreach (Transform wheel in this.rightWheels)
                {
                    wheel.Rotate(Vector3.right, rotationSpeed);
                }

                //Set track movement speed
                Vector2 trackTextureSpeed = Vector2.up * (currentSpeed * this.trackSpeed);
                this.leftTrack.material.mainTextureOffset += trackTextureSpeed;
                this.rightTrack.material.mainTextureOffset += trackTextureSpeed * rightTrackMod;
            }
        }
        #endregion
    }
}