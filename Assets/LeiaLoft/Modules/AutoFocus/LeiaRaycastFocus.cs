using UnityEngine;

namespace LeiaLoft
{
    [RequireComponent(typeof(LeiaCamera))]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.leialoft.com/developer/unity-sdk/modules/auto-focus#leiaraycastfocus")]
    public class LeiaRaycastFocus : LeiaFocus
    {
        [Tooltip("Layers the camera should focus on"), SerializeField] private LayerMask _layers = ~0;
        public LayerMask layers
        {
            get
            {
                return _layers;
            }
            set
            {
                _layers = value;
            }
        }

        [SerializeField, Tooltip("The maximum distance the auto focus algorithm is allowed to raycast to collect sample points. Note that if this is smaller than the convergence range max value, then it will take precedence over the convergence range max value when determining the max convergence distance.")]
        private float _maxRaycastDistance = 1000f;
        [Range(1, 1000), Tooltip("Raycast samples to take")]
        [SerializeField] private int samples = 500;
        private int previous_samples = -1;
        private Vector2[] cameraNearPlaneRaysOrigins = new Vector2[500];

        [Tooltip("Show debug rays in the scene editor")]
        [SerializeField] private bool showDebugRaycasts = false;

        private float furthestDistance;
        private float closestDistance;
        private float avgDistance;
        private float hits = 0;

        /// <summary>
        /// A higher than 1 distanceWeightPower value will result in centermost raycasts being more important.
        /// A closer to 1 distanceWeightPower will result in more uniform significance of raycasts across the view.
        /// </summary>
        const float distanceWeightPower = 1.1f;
        /// <summary>
        /// The minimum distancePowered a sample can have (prevents division by zero, sets cap on how significant 
        /// the centermost point is, lower values mean the centermost point is more significant, do not set to 0)
        /// </summary>
        const float minDistancePowered = .1f;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void LateUpdate()
        {
            CalculateTargetConvergenceAndBaseline();
            base.LateUpdate();
        }

        public float MaxRaycastDistance
        {
            get
            {
                return _maxRaycastDistance;
            }
            set
            {
                _maxRaycastDistance = Mathf.Max(value, 0f);
            }
        }

        readonly Vector3[] frustrumCorners = new Vector3[4];
        private Matrix4x4 nearPlaneTM;
        private Matrix4x4 offsettedNearPlaneTM;
        readonly float crossScale = 0.01f;

        private void CalculateTargetConvergenceAndBaseline()
        {
            samples = Mathf.Max(1, samples);
            if (samples != previous_samples) {
                System.Array.Resize(ref cameraNearPlaneRaysOrigins, samples);
                int _sqrtSamples = Mathf.CeilToInt(Mathf.Sqrt(samples));
                int spaceSegments = _sqrtSamples + 1;
                float percentagePerSegment = 1f / spaceSegments;
                int samplesCounter = 0;
                //Always perform a sample in the direct center of the view
                cameraNearPlaneRaysOrigins[samplesCounter++] = new Vector2(0, 0);
                float offset = -0.5f + percentagePerSegment;
                for (int x = 0; x < _sqrtSamples; x++)
                {
                    for (int y = 0; y < _sqrtSamples; y++)
                    {
                        if (samplesCounter < samples)
                        {
                            cameraNearPlaneRaysOrigins[samplesCounter] = new Vector2(offset + x * percentagePerSegment, offset + y * percentagePerSegment);
                            samplesCounter++;
                        }
                    }
                }

                previous_samples = samples;
            }

            Vector3 wfc0;
            Vector3 wfc1;
            Vector3 wfc2;
            Vector3 wfc3;
            Matrix4x4 ltwm = transform.localToWorldMatrix;
            Vector3 pos = transform.position;
            Vector3 right = transform.right;
            Vector3 fwd = transform.forward;
            Vector3 up = transform.up;

            float forwardOffset = leiaCamera.Camera.nearClipPlane;
            if (leiaCamera.Camera.orthographic)
            {
                float ysize = leiaCamera.Camera.orthographicSize;
                float xsize = ysize * leiaCamera.Camera.aspect;
                wfc0 = pos + fwd * forwardOffset - up * ysize - right * xsize;
                wfc1 = pos + fwd * forwardOffset + up * ysize - right * xsize;
                wfc2 = pos + fwd * forwardOffset + up * ysize + right * xsize;
                wfc3 = pos + fwd * forwardOffset - up * ysize + right * xsize;
            }
            else
            {  
                leiaCamera.Camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), forwardOffset, Camera.MonoOrStereoscopicEye.Mono, frustrumCorners);
                wfc0 = pos + ltwm.MultiplyVector(frustrumCorners[0]);
                wfc1 = pos + ltwm.MultiplyVector(frustrumCorners[1]);
                wfc2 = pos + ltwm.MultiplyVector(frustrumCorners[2]);
                wfc3 = pos + ltwm.MultiplyVector(frustrumCorners[3]);
            }

            nearPlaneTM = Matrix4x4.Translate( Vector3.LerpUnclamped( wfc0, wfc2, 0.5f) );
            nearPlaneTM.SetColumn(0, wfc3 - wfc0 );
            nearPlaneTM.SetColumn(1, wfc1 - wfc0 );
            if (showDebugRaycasts) {
                offsettedNearPlaneTM = nearPlaneTM;
                offsettedNearPlaneTM.SetColumn(3, offsettedNearPlaneTM.GetColumn(3) + (Vector4)(fwd * 0.001f));
            }


            furthestDistance = 0;
            closestDistance = float.MaxValue;
            avgDistance = 0f;
            hits = 0;
            float convergencePlaneHalfWidth;

            //Execute a grid of samples across the entire view
            if (leiaCamera.Camera.orthographic)
            {
                convergencePlaneHalfWidth = leiaCamera.Camera.orthographicSize * 2f;
                for (int i = 0; i < cameraNearPlaneRaysOrigins.Length; i++)
                {              
                    Vector3 startPoint = nearPlaneTM.MultiplyPoint3x4(cameraNearPlaneRaysOrigins[i]);
                    SampleSpherecast(startPoint, fwd, convergencePlaneHalfWidth / (samples * 2f), i);
                }
 

            } else
            {
                convergencePlaneHalfWidth = leiaCamera.ConvergenceDistance * Mathf.Sin(leiaCamera.Camera.fieldOfView * Mathf.Deg2Rad) / 2f;
                for (int i = 0; i < cameraNearPlaneRaysOrigins.Length; i++)
                {
                    Vector3 startPoint = nearPlaneTM.MultiplyPoint3x4(cameraNearPlaneRaysOrigins[i]);
                    Vector3 direction = (startPoint - transform.position).normalized;
                    SampleSpherecast(startPoint, direction, convergencePlaneHalfWidth / (samples * 2f), i);
                }
            }
 

            if (hits > 0)
            {
                avgDistance /= hits;

                float newTargetConvergenceDistance = avgDistance + leiaCamera.Camera.nearClipPlane;

                this.SetTargetConvergence(newTargetConvergenceDistance);

                float nearPlaneBestBaseline = GetRecommendedBaselineBasedOnNearPlane(
                    closestDistance, 
                    targetConvergence
                );

                float farPlaneBestBaseline = GetRecommendedBaselineBasedOnFarPlane(
                    furthestDistance, 
                    targetConvergence
                );

                float targetBaseline = Mathf.Min(nearPlaneBestBaseline, farPlaneBestBaseline);
                this.SetTargetBaseline(targetBaseline);
            }
        }

        float GetRecommendedBaselineBasedOnNearPlane(float nearPlaneDistance, float convergenceDistance)
        {
            float recommendedBaseline;

            if (leiaCamera.Camera.orthographic)
            {
                recommendedBaseline = -1f / (nearPlaneDistance - convergenceDistance);
            }
            else //if its a perspective camera
            {
                recommendedBaseline = nearPlaneDistance / Mathf.Max(convergenceDistance - nearPlaneDistance, .01f);
            }

            return recommendedBaseline;
        }

        float GetRecommendedBaselineBasedOnFarPlane(float farPlaneDistance, float convergenceDistance)
        {
            float recommendedBaseline;

            if (leiaCamera.Camera.orthographic)
            {
                recommendedBaseline = 1f / (farPlaneDistance - convergenceDistance);
            }
            else //if its a perspective camera
            {
                recommendedBaseline = farPlaneDistance / Mathf.Max(convergenceDistance - farPlaneDistance, .01f);
            }

            return recommendedBaseline;
        }

        /// <summary>
        /// Performs a spherecast sample, sets the closest and furthest distance found so far, and accumulates
        /// the sum of hit distances so that the average can be later calculated and used to set convergence distance
        /// </summary>
        /// <param name="startPoint"></param> The point to start the spherecast from
        /// <param name="direction"></param> The direction for the spherecast to travel in
        /// <param name="radius"></param> The radius of the spherecast
        void SampleSpherecast(Vector3 startPoint, Vector3 direction, float radius, int sampleIndex)
        {
 
            RaycastHit hit;
            Physics.SphereCast(
                startPoint,
                radius,
                direction,
                out hit,
                MaxRaycastDistance,
                layers
                );

            if (hit.collider != null)
            {
                if (hit.distance > furthestDistance)
                {
                    furthestDistance = hit.distance;
                }
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                }

                //Accumulate weighted average of hit distances where weight is inversely proportional to distance from center of view
               
                float distancePowered = Mathf.Pow(hit.distance, distanceWeightPower);
                float weight = (1f / Mathf.Max(minDistancePowered, distancePowered));

                avgDistance += hit.distance * weight;
                hits += 1f * weight;

                if (showDebugRaycasts)
                {
                    Debug.DrawLine(startPoint, startPoint + direction * hit.distance, Color.green);
                    Vector2 c0 = cameraNearPlaneRaysOrigins[sampleIndex];
                    Vector2 cbottom = c0 + new Vector2(0, -crossScale);
                    Vector2 ctop = c0 + new Vector2(0, crossScale);
                    Vector2 cleft = c0 + new Vector2(-crossScale, 0);
                    Vector2 cright = c0 + new Vector2(crossScale, 0);
                    Debug.DrawLine(offsettedNearPlaneTM.MultiplyPoint3x4(ctop), nearPlaneTM.MultiplyPoint3x4(cbottom), Color.green);
                    Debug.DrawLine(offsettedNearPlaneTM.MultiplyPoint3x4(cleft), nearPlaneTM.MultiplyPoint3x4(cright), Color.green);
                }
 
            }
 
            else
            {
                if (showDebugRaycasts)
                {
                    Debug.DrawRay(startPoint, direction * targetConvergence * 2f, Color.red);
                    Vector2 c0 = cameraNearPlaneRaysOrigins[sampleIndex];
                    Vector2 cbottom = c0 + new Vector2(0, -crossScale);
                    Vector2 ctop = c0 + new Vector2(0, crossScale);
                    Vector2 cleft = c0 + new Vector2(-crossScale, 0);
                    Vector2 cright = c0 + new Vector2(crossScale, 0);
                    Debug.DrawLine(offsettedNearPlaneTM.MultiplyPoint3x4(ctop), nearPlaneTM.MultiplyPoint3x4(cbottom), Color.red);
                    Debug.DrawLine(offsettedNearPlaneTM.MultiplyPoint3x4(cleft), nearPlaneTM.MultiplyPoint3x4(cright), Color.red);
                }
                
            }
 
        }

    }
}