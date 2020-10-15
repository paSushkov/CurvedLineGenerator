using System;
using UnityEngine;

namespace Sushkov.LinesUtility
{
    public class UpDowner : MonoBehaviour
    {
        #region Fields

        public AnimationCurve curve;
        [Range(1, 10f)]
        public float power;
        private Transform _transform;
        private Vector3 _localPosition;

        #endregion

        
        #region UnityEvents

        private void Awake()
        {
            _transform = transform;
            _localPosition = _transform.localPosition;
        }

        private void FixedUpdate()
        {
            _localPosition.y = power*curve.Evaluate(Time.time);
            _transform.localPosition = _localPosition;
        }

        #endregion
        
        
    }
}