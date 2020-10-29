// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.XR.ARFoundation;

#endif

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public abstract class InputInteractionBase : MonoBehaviour
    {
#if UNITY_ANDROID || UNITY_IOS
        ARRaycastManager arRaycastManager;
        protected DateTime dueDate3 = DateTime.MaxValue;
#endif
        /// <summary>
        /// Destroying the attached Behaviour will result in the game or Scene
        /// receiving OnDestroy.
        /// </summary>
        /// <remarks>
        /// OnDestroy will only be called on game objects that have previously been active.
        /// </remarks>
        public virtual void OnDestroy()
        {
#if WINDOWS_UWP || UNITY_WSA
            UnityEngine.XR.WSA.Input.InteractionManager.InteractionSourcePressed -=
 InteractionManager_InteractionSourcePressed;
#endif
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public virtual void Start()
        {
#if UNITY_ANDROID || UNITY_IOS
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
            if (arRaycastManager == null)
            {
                Debug.Log("Missing ARRaycastManager in scene");
            }
#endif
#if WINDOWS_UWP || UNITY_WSA
            UnityEngine.XR.WSA.Input.InteractionManager.InteractionSourcePressed +=
 InteractionManager_InteractionSourcePressed;
#endif
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public virtual void Update()
        {
            if (InputSwitch.ScreenEnable)
            {
                TriggerInteractions();
            }

            int timeLeft = (int) (dueDate3 - DateTime.Now).TotalSeconds;
            if (InputSwitch.laserEnable && timeLeft<0)
            {
                
                OnTouchInteractionEnded2();
                dueDate3 = DateTime.Now.AddMilliseconds(500);
            }
            
        }

        private void TriggerInteractions()
        {
            OnGazeInteraction();

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }

                OnTouchInteraction(touch);
            }
        }

        /// <summary>
        /// Called when gaze interaction occurs.
        /// </summary>
        protected virtual void OnGazeInteraction()
        {
            // See if we hit a surface. If not, position the object in front of the user.
            RaycastHit target;
            if (TryGazeHitTest(out target))
            {
                OnGazeObjectInteraction(target.point, target.normal);
            }
            else
            {
                OnGazeObjectInteraction(Camera.main.transform.position + Camera.main.transform.forward * 1.5f,
                    -Camera.main.transform.forward);
            }
        }

        /// <summary>
        /// Called when gaze interaction begins.
        /// </summary>
        /// <param name="hitPoint">The hit point.</param>
        /// <param name="target">The target.</param>
        protected virtual void OnGazeObjectInteraction(Vector3 hitPoint, Vector3 hitNormal)
        {
            // To be overridden.
        }

        /// <summary>
        /// Called when a touch interaction occurs.
        /// </summary>
        /// <param name="touch">The touch.</param>
        protected virtual void OnTouchInteraction(Touch touch)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                OnTouchInteractionEnded(touch);
            }
        }

        /// <summary>
        /// Called when a touch interaction has ended.
        /// </summary>
        /// <param name="touch">The touch.</param>
        protected virtual void OnTouchInteractionEnded(Touch touch)
        {
#if UNITY_ANDROID || UNITY_IOS
            List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
            Debug.Log("find the touch point: " + touch.position);
            if (arRaycastManager.Raycast(touch.position, aRRaycastHits) && aRRaycastHits.Count > 0)
            {
                ARRaycastHit hit = aRRaycastHits[0];

                Debug.Log("find the hit position: " + hit.pose.position.ToString("F8"));
                //compare the distance
                // float minDistance = Single.MaxValue;
                // Vector3 rightPoint = new Vector3();
                // for (int i = 0; i < Points.lists.Count; i++)
                // {
                //     float dis = Vector3.Distance(Points.lists[i], hit.pose.position);
                //     if (dis<minDistance)
                //     {
                //         rightPoint = Points.lists[i];
                //     }
                // }

                // if (Points.lists.Count!=0)
                // {
                //     OnSelectObjectInteraction(rightPoint, hit);
                // }
                // else
                // {
                Vector3 centerPoint = new Vector3((float) Math.Round(hit.pose.position.x, 1),
                    (float) Math.Round(hit.pose.position.y, 1), (float) Math.Round(hit.pose.position.z, 1));;

                if (Points.lists.Count == 0)
                {
                    Points.centerPoint = centerPoint;
                    Points.centerPointY = (float) Math.Round(hit.pose.position.y, 1);
                }


                // OnSelectObjectInteraction(hit.pose.position, hit);
                if (Points.candidate.Count == 0)
                {
                    OnSelectObjectInteraction(centerPoint, hit);
                    Debug.Log("center point: " + centerPoint.ToString("F8"));
                }
                else
                {
                    int indexCandidate = 0;
                    float myDistance = Single.MaxValue;
                    for (int i = 0; i < Points.candidate.Count; i++)
                    {
                        float tempDistance = Vector3.Distance(Points.candidate[i], centerPoint);
                        if (tempDistance<myDistance)
                        {
                            myDistance = tempDistance;
                            indexCandidate = i;
                        }
                    }
                    OnSelectObjectInteraction(Points.candidate[indexCandidate], hit);
                    Debug.Log("center point: " + Points.candidate[indexCandidate].ToString("F8"));
                }
                
                // else if (!Points.lists.Contains(centerPoint))
                // {
                //     foreach (var point in Points.lists)
                //     {
                //         float distance = Vector3.Distance(point, centerPoint);
                //         if (distance >= 0.2f && distance <= 0.5f)
                //         {
                //             OnSelectObjectInteraction(centerPoint, hit);
                //             Debug.Log("center point, count!=0: " + centerPoint.ToString("F8"));
                //             break;
                //         }
                //     }
                // }

                // }
            }
#elif WINDOWS_UWP || UNITY_WSA
            RaycastHit hit;
            if (TryGazeHitTest(out hit))
            {
                OnSelectObjectInteraction(hit.point, hit);
            }
#endif
        }
        
        
        protected virtual void OnTouchInteractionEnded2()
        {
            List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
            Debug.Log("find the touch point: " + new Vector2(Screen.width/2.0f, Screen.height/2.0f));
            if (arRaycastManager.Raycast(new Vector2(Screen.width/2.0f, Screen.height/2.0f), aRRaycastHits) && aRRaycastHits.Count > 0)
            {
                ARRaycastHit hit = aRRaycastHits[0];

                Debug.Log("find the hit position: " + hit.pose.position.ToString("F8"));
                
                Vector3 centerPoint = new Vector3((float) Math.Round(hit.pose.position.x, 1),
                    (float) Math.Round(hit.pose.position.y, 1), (float) Math.Round(hit.pose.position.z, 1));;

                // if (Points.lists.Count == 0)
                // {
                //     Points.centerPoint = centerPoint;
                //     Points.centerPointY = (float) Math.Round(hit.pose.position.y, 1);
                // }


                // OnSelectObjectInteraction(hit.pose.position, hit);
                if (Points.candidate.Count == 0)
                {
                    OnSelectObjectInteraction(centerPoint, hit);
                    Debug.Log("center point: " + centerPoint.ToString("F8"));
                }
                else
                {
                    int indexCandidate = 0;
                    float myDistance = Single.MaxValue;
                    for (int i = 0; i < Points.candidate.Count; i++)
                    {
                        float tempDistance = Vector3.Distance(Points.candidate[i], centerPoint);
                        if (tempDistance < myDistance)
                        {
                            myDistance = tempDistance;
                            indexCandidate = i;
                        }
                    }

                    OnSelectObjectInteraction(Points.candidate[indexCandidate], hit);
                    Debug.Log("center point: " + Points.candidate[indexCandidate].ToString("F8"));
                }
            }
        }
        /// <summary>
        /// Called when a select interaction occurs.
        /// </summary>
        /// <remarks>Currently only called for HoloLens.</remarks>
        protected virtual void OnSelectInteraction()
        {
#if WINDOWS_UWP || UNITY_WSA
            RaycastHit hit;
            if (TryGazeHitTest(out hit))
            {
                OnSelectObjectInteraction(hit.point, hit);
            }
#endif
        }

        /// <summary>
        /// Called when a touch object interaction occurs.
        /// </summary>
        /// <param name="hitPoint">The position.</param>
        /// <param name="target">The target.</param>
        protected virtual void OnSelectObjectInteraction(Vector3 hitPoint, object target)
        {
            // To be overridden.
        }

        private bool TryGazeHitTest(out RaycastHit target)
        {
            Camera mainCamera = Camera.main;

            return Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out target);
        }

#if WINDOWS_UWP || UNITY_WSA
        /// <summary>
        /// Handles the HoloLens interaction event.
        /// </summary>
        /// <param name="obj">The <see cref="UnityEngine.XR.WSA.Input.InteractionSourcePressedEventArgs"/> instance containing the event data.</param>
        private void InteractionManager_InteractionSourcePressed(UnityEngine.XR.WSA.Input.InteractionSourcePressedEventArgs obj)
        {
            if (obj.pressType == UnityEngine.XR.WSA.Input.InteractionSourcePressType.Select)
            {
                OnSelectInteraction();
            }
        }
#endif
    }
}