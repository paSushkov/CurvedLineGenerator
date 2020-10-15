using UnityEngine;


namespace Sushkov.LinesUtility
{
    public sealed class Rotator : MonoBehaviour
    {
        #region PrivateData

        private Transform _transform;

        #endregion


        #region Fields

        public float xSpeed = 1f;
        public float ySpeed = 1f;
        public float zSpeed = 1f;
        public bool isUsingGlobal;

        #endregion


        #region UnityMethods

        private void Awake()
        {
            _transform = transform;
        }

        private void FixedUpdate()
        {
            if (isUsingGlobal)
            {
                transform.Rotate(Vector3.right, xSpeed * Time.fixedDeltaTime, Space.World);
                transform.Rotate(Vector3.up, ySpeed * Time.fixedDeltaTime, Space.World);
                transform.Rotate(Vector3.forward, zSpeed * Time.fixedDeltaTime, Space.World);
            }
            else
            {
                transform.Rotate(_transform.right, xSpeed * Time.fixedDeltaTime, Space.World);
                transform.Rotate(_transform.up, ySpeed * Time.fixedDeltaTime, Space.World);
                transform.Rotate(_transform.forward, zSpeed * Time.fixedDeltaTime, Space.World);
            }
            
        }

        #endregion
    }
}