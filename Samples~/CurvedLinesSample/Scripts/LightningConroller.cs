using UnityEngine;

namespace Sushkov.LinesUtility
{
    public class LightningConroller : MonoBehaviour
    {
        public ParticleSystem sparks;
        public ParticleSystem flash;
        public LineRenderer lineRenderer;
        public CurvesUtility lineControl;
        public Light flashLight;

        // Update is called once per frame
        void Update()
        {
            if (lineControl.IsCutted)
            {
                var hitPosition = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
                sparks.transform.position = hitPosition;
                flash.transform.position = hitPosition;
                sparks.transform.LookAt(lineRenderer.GetPosition(lineRenderer.positionCount - 2), Vector3.up);
                if (!sparks.isStopped) return;
                sparks.Play();
                flash.Play();
                flashLight.enabled = true;
            }
            else
            {
                if (!sparks.isPlaying) return;
                sparks.Stop();
                flash.Stop();
                flashLight.enabled = false;
            }
        }
    }
}