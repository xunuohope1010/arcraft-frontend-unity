// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class AzureSpatialAnchorsNearbyDemoScript : DemoScriptBase
    {
        internal enum AppState
        {
            Placing = 0,
            Saving,
            ReadyToGraph,
            Graphing,
            ReadyToSearch,
            Searching,
            ReadyToNeighborQuery,
            Neighboring,
            Done,
            ModeCount
        }

        private readonly Color[] colors =
        {
            Color.white,
            Color.magenta,
            Color.magenta,
            Color.yellow,
            Color.magenta,
            Color.cyan,
            Color.magenta,
            Color.green,
            Color.grey
        };

        private readonly Vector3[] scaleMods =
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(.1f, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, .1f),
            new Vector3(0, 0, 0),
            new Vector3(0, .1f, 0),
            new Vector3(0, 0, 0)
        };

        private readonly int numToMake = Points.target.Count;

        private AppState _currentAppState = AppState.Placing;

        AppState currentAppState
        {
            get { return _currentAppState; }
            set
            {
                if (_currentAppState != value)
                {
                    Debug.LogFormat("State from {0} to {1}", _currentAppState, value);
                    _currentAppState = value;
                }
            }
        }

        readonly List<string> anchorIds = new List<string>();

        readonly Dictionary<AppState, Dictionary<string, GameObject>> spawnedObjectsPerAppState =
            new Dictionary<AppState, Dictionary<string, GameObject>>();

        Dictionary<string, GameObject> spawnedObjectsInCurrentAppState
        {
            get
            {
                if (spawnedObjectsPerAppState.ContainsKey(_currentAppState) == false)
                {
                    spawnedObjectsPerAppState.Add(_currentAppState, new Dictionary<string, GameObject>());
                }

                return spawnedObjectsPerAppState[_currentAppState];
            }
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public override void Start()
        {
            Debug.Log(">>Azure Spatial Anchors Demo Script Start");

            base.Start();

            if (!SanityCheckAccessConfiguration())
            {
                return;
            }

            feedbackBox.text =
                "Find nearby demo.  First, we need to place a few anchors. Tap somewhere to place the first one";

            InputSwitch.ScreenEnable = false;
            BlockLeft.text = "" + numToMake;
            TapHere.SetActive(false);
            NextBtn.SetActive(false);
            HotBar.SetActive(true);
            Score.text = 0.ToString();

            Debug.Log("Azure Spatial Anchors Demo script started");
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public override void Update()
        {
            base.Update();
            int timeLeft = (int) (dueDate2 - DateTime.Now).TotalSeconds;
            if (timeLeft<0)
            {
                InputSwitch.laserEnable = false;
                CleanupObjectsBetweenPasses2();
                dueDate2 = DateTime.MaxValue;
                TapHere.SetActive(false);
                HotBar.SetActive(true);
                NextBtn.SetActive(false);
            }
            // HandleCurrentAppState();
        }

        private void HandleCurrentAppState()
        {
            int timeLeft = (int) (dueDate - DateTime.Now).TotalSeconds;
            switch (currentAppState)
            {
                case AppState.ReadyToGraph:
                    feedbackBox.text = "Next: Tap to start a query for all anchors we just made.";
                    break;
                case AppState.Graphing:
                    feedbackBox.text =
                        $"Making sure we can find the anchors we just made. ({locatedCount}/{numToMake})";
                    break;
                case AppState.ReadyToSearch:
                    feedbackBox.text = "Next: Tap to start looking for just the first anchor we placed.";
                    break;
                case AppState.Searching:
                    feedbackBox.text = $"Looking for the first anchor you made. Give up in {timeLeft}";
                    if (timeLeft < 0)
                    {
                        Debug.Log("Out of time");
                        // Restart the demo..
                        feedbackBox.text = "Failed to find the first anchor.  Try again.";
                        currentAppState = AppState.Done;
                    }

                    break;
                case AppState.ReadyToNeighborQuery:
                    feedbackBox.text = "Next: Tap to start looking for anchors nearby the first anchor we placed.";
                    break;
                case AppState.Neighboring:
                    // We should find all anchors except for the anchor we are using as the source anchor.
                    feedbackBox.text =
                        $"Looking for anchors nearby the first anchor. {locatedCount}/{numToMake - 1} {timeLeft}";
                    if (timeLeft < 0)
                    {
                        feedbackBox.text = "Failed to find all the neighbors.  Try again.";
                        currentAppState = AppState.Done;
                    }

                    if (locatedCount == numToMake - 1)
                    {
                        feedbackBox.text = "Found them all!";
                        currentAppState = AppState.Done;
                    }

                    break;
            }
        }

        protected override bool IsPlacingObject()
        {
            return currentAppState == AppState.Placing;
        }

        protected override Color GetStepColor()
        {
            return colors[(int) currentAppState];
        }

        private int locatedCount = 0;

        protected override void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {
            base.OnCloudAnchorLocated(args);

            if (args.Status == LocateAnchorStatus.Located)
            {
                UnityDispatcher.InvokeOnAppThread(() =>
                {
                    locatedCount++;
                    currentCloudAnchor = args.Anchor;
                    Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
                    anchorPose = currentCloudAnchor.GetPose();
#endif
                    // HoloLens: The position will be set based on the unityARUserAnchor that was located.

                    SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);

                    spawnedObject.transform.localScale += scaleMods[(int) currentAppState];
                    spawnedObject = null;

                    if (currentAppState == AppState.Graphing)
                    {
                        if (spawnedObjectsInCurrentAppState.Count == anchorIds.Count)
                        {
                            currentAppState = AppState.ReadyToSearch;
                        }
                    }
                    else if (currentAppState == AppState.Searching)
                    {
                        currentAppState = AppState.ReadyToNeighborQuery;
                    }
                });
            }
        }

        private DateTime dueDate = DateTime.Now;
        
        
        private readonly List<Material> allSpawnedMaterials = new List<Material>();

        protected override void SpawnOrMoveCurrentAnchoredObject(Vector3 worldPos, Quaternion worldRot)
        {
            if (currentCloudAnchor != null &&
                spawnedObjectsInCurrentAppState.ContainsKey(currentCloudAnchor.Identifier))
            {
                spawnedObject = spawnedObjectsInCurrentAppState[currentCloudAnchor.Identifier];
            }

            bool spawnedNewObject = spawnedObject == null;

            base.SpawnOrMoveCurrentAnchoredObject(worldPos, worldRot);

            if (spawnedNewObject)
            {
                allSpawnedObjects.Add(spawnedObject);
                allSpawnedMaterials.Add(spawnedObjectMat);

                if (currentCloudAnchor != null &&
                    spawnedObjectsInCurrentAppState.ContainsKey(currentCloudAnchor.Identifier) == false)
                {
                    spawnedObjectsInCurrentAppState.Add(currentCloudAnchor.Identifier, spawnedObject);
                }
            }

#if WINDOWS_UWP || UNITY_WSA
            if (currentCloudAnchor != null
                    && spawnedObjectsInCurrentAppState.ContainsKey(currentCloudAnchor.Identifier) == false)
            {
                spawnedObjectsInCurrentAppState.Add(currentCloudAnchor.Identifier, spawnedObject);
            }
#endif
        }

        public async override Task AdvanceDemoAsync()
        {
            switch (currentAppState)
            {
                case AppState.Placing:
                    if (spawnedObject != null)
                    {
                        InputSwitch.ScreenEnable = false;
                        NextBtn.SetActive(false);
                        currentAppState = AppState.Saving;
                        if (!CloudManager.IsSessionStarted)
                        {
                            await CloudManager.StartSessionAsync();
                        }

                        await SaveCurrentObjectAnchorToCloudAsync();
                        
                        HotBar.SetActive(true);
                        BlockType.text = "None";
                        dueDate3 = DateTime.MaxValue;
                    }

                    break;
                case AppState.ReadyToGraph:
                    await DoGraphingPassAsync();
                    break;
                case AppState.ReadyToSearch:
                    await DoSearchingPassAsync();
                    break;
                case AppState.ReadyToNeighborQuery:
                    await DoNeighboringPassAsync();
                    break;
                case AppState.Done:
                    await CloudManager.ResetSessionAsync();
                    CleanupObjectsBetweenPasses();
                    currentAppState = AppState.Placing;
                    feedbackBox.text = $"Place an object. {allSpawnedObjects.Count}/{numToMake} ";
                    break;
            }
        }

        protected override async Task OnSaveCloudAnchorSuccessfulAsync()
        {
            await base.OnSaveCloudAnchorSuccessfulAsync();

            Debug.Log("Anchor created, yay!");

            anchorIds.Add(currentCloudAnchor.Identifier);

            // Sanity check that the object is still where we expect
            Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
            anchorPose = currentCloudAnchor.GetPose();
#endif
            // HoloLens: The position will be set based on the unityARUserAnchor that was located.

            Debug.Log("first we need to find position " + anchorPose.position.ToString("F8"));
            Debug.Log("second, we need to find rotation " + anchorPose.rotation.ToString("F8"));
            // List<Vector3> lists = new List<Vector3>();
            // Vector3 rightPoint = new Vector3();
            // if (allSpawnedObjects.Count==1)
            // {
            //     //create list
            //     // float centerPointX = anchorPose.position.x;
            //     // float centerPointY = anchorPose.position.y;
            //     // float centerPointZ = anchorPose.position.z;
            //     Vector3 cornerPoint = new Vector3(anchorPose.position.x-0.1f*2, anchorPose.position.y-0.1f*2, anchorPose.position.z-0.1f*2);
            //     
            //     
            //     for (int i = 0; i < 5; i++)
            //     {
            //         for (int j = 0; j < 5; j++)
            //         {
            //             for (int k = 0; k < 5; k++)
            //             {
            //                 Points.lists.Add(new Vector3(cornerPoint.x+i*0.1f, cornerPoint.y+j*0.1f, cornerPoint.z+k*0.1f));
            //             }
            //         }
            //     }
            //
            //     Points.removeVector(Points.lists,anchorPose.position);
            //     // Debug.Log("full list: "+Points.lists.ToString());
            //     string strBuild = "full list ";
            //     for (int i = 0; i < Points.lists.Count; i++)
            //     {
            //         strBuild += Points.lists[i].ToString("F8") + " ";
            //     }
            //     Debug.Log(strBuild);
            //     Debug.Log("anchor.position: "+anchorPose.position);
            //     Debug.Log("length of list: "+Points.lists.Count);
            // }
            // else
            // {
            //     Points.removeVector(Points.lists,anchorPose.position);
            //     // Debug.Log("full list: "+Points.lists.ToString());
            //     string strBuild = "full list ";
            //     for (int i = 0; i < Points.lists.Count; i++)
            //     {
            //         strBuild += Points.lists[i].ToString("F8") + " ";
            //     }
            //     Debug.Log(strBuild);
            //     Debug.Log("anchor.position: "+anchorPose.position);
            //     Debug.Log("length of list else: "+Points.lists.Count);
            // }
            // SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);
            Debug.Log("add to list: " + anchorPose.position.ToString("F8"));
            Vector3 correctPoint = new Vector3((float) Math.Round(anchorPose.position.x, 1),
                (float) Math.Round(anchorPose.position.y, 1), (float) Math.Round(anchorPose.position.z, 1));

            Points.lists.Add(correctPoint);
            Points.candidate.Remove(correctPoint);

            for (float i = 0.1f; i <= 0.1f; i += 0.1f)
            {
                Vector3 candidatePoint =
                    new Vector3((float)Math.Round(correctPoint.x + i,1), correctPoint.y, correctPoint.z);
                if (!Points.candidate.Contains(candidatePoint)&&!Points.lists.Contains(candidatePoint))
                {
                    Points.candidate.Add(candidatePoint);
                }

                candidatePoint = new Vector3((float)Math.Round(correctPoint.x - i,1), correctPoint.y, correctPoint.z);
                if (!Points.candidate.Contains(candidatePoint)&&!Points.lists.Contains(candidatePoint))
                {
                    Points.candidate.Add(candidatePoint);
                }

                candidatePoint = new Vector3(correctPoint.x, correctPoint.y, (float)Math.Round(correctPoint.z + i,1));
                if (!Points.candidate.Contains(candidatePoint)&&!Points.lists.Contains(candidatePoint))
                {
                    Points.candidate.Add(candidatePoint);
                }

                candidatePoint = new Vector3(correctPoint.x, correctPoint.y, (float)Math.Round(correctPoint.z - i,1));
                if (!Points.candidate.Contains(candidatePoint)&&!Points.lists.Contains(candidatePoint))
                {
                    Points.candidate.Add(candidatePoint);
                }
                
                candidatePoint = new Vector3(correctPoint.x, (float)Math.Round(correctPoint.y+i,1), correctPoint.z);
                if (!Points.candidate.Contains(candidatePoint)&&!Points.lists.Contains(candidatePoint))
                {
                    Points.candidate.Add(candidatePoint);
                }
                
                candidatePoint = new Vector3(correctPoint.x, (float)Math.Round(correctPoint.y-i,1), correctPoint.z);
                if (!Points.candidate.Contains(candidatePoint)&&!Points.lists.Contains(candidatePoint)&&(float)Math.Round(correctPoint.y-i-Points.centerPointY,1)>=0.0f)
                {
                    Points.candidate.Add(candidatePoint);
                }
            }
            
            SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);
            
            SpawnNewAnchoredObject3(Points.candidate);


            int sum = 0;
            foreach (Vector3 point in Points.offset)
            {
                if (Points.lists.Contains(point))
                {
                    sum++;
                }
            }
            
            Debug.Log("sum: "+sum);
            
            int score = (int)(sum / (double)Points.target.Count*100);
            Debug.Log("score: "+score);
            Score.text = score.ToString();

            spawnedObject = null;
            currentCloudAnchor = null;
            if (allSpawnedObjects.Count < numToMake)
            {
                BlockLeft.text = "" + (numToMake - allSpawnedObjects.Count);
                feedbackBox.text = $"Saved...Make another {allSpawnedObjects.Count}/{numToMake} ";
                currentAppState = AppState.Placing;
                CloudManager.StopSession();
            }
            else
            {
                BlockLeft.text = "" + (numToMake - allSpawnedObjects.Count);
                feedbackBox.text = "Saved... ready to start finding them.";
                CloudManager.StopSession();
                // currentAppState = AppState.ReadyToGraph;
            }
        }

        protected override void OnSaveCloudAnchorFailed(Exception exception)
        {
            base.OnSaveCloudAnchorFailed(exception);
        }

        private async Task DoGraphingPassAsync()
        {
            SetGraphEnabled(false);
            await CloudManager.ResetSessionAsync();
            locatedCount = 0;
            SetAnchorIdsToLocate(anchorIds);
            SetNearbyAnchor(null, 10, numToMake);
            await CloudManager.StartSessionAsync();
            currentWatcher = CreateWatcher();
            currentAppState = AppState.Graphing; //do the recall..
        }

        private async Task DoSearchingPassAsync()
        {
            await CloudManager.ResetSessionAsync();
            await CloudManager.StartSessionAsync();
            SetGraphEnabled(false);
            IEnumerable<string> anchorsToFind = new[] {anchorIds[0]};
            SetAnchorIdsToLocate(anchorsToFind);
            locatedCount = 0;
            dueDate = DateTime.Now.AddSeconds(30);
            currentWatcher = CreateWatcher();
            currentAppState = AppState.Searching;
        }

        private async Task DoNeighboringPassAsync()
        {
            await CloudManager.StartSessionAsync();
            SetGraphEnabled(true);
            ResetAnchorIdsToLocate();
            SetNearbyAnchor(currentCloudAnchor, 10, numToMake);
            locatedCount = 0;
            dueDate = DateTime.Now.AddSeconds(30);
            currentWatcher = CreateWatcher();
            currentAppState = AppState.Neighboring;
        }

        private void CleanupObjectsBetweenPasses2()
        {
            foreach (GameObject go in allSpawnedObjects2)
            {
                Destroy(go);
            }
            allSpawnedObjects2.Clear();
            foreach (GameObject go in allSpawnedObjects)
            {
                go.SetActive(true);
            }

            foreach (GameObject go in allSpawnedObjects3)
            {
                go.SetActive(true);
            }
        }
        
        private void CleanupObjectsBetweenPasses()
        {
            foreach (GameObject go in allSpawnedObjects)
            {
                Destroy(go);
            }

            allSpawnedObjects.Clear();

            foreach (Material m in allSpawnedMaterials)
            {
                Destroy(m);
            }

            allSpawnedMaterials.Clear();

            currentCloudAnchor = null;
            spawnedObject = null;
            spawnedObjectMat = null;
            spawnedObjectsPerAppState.Clear();
            anchorIds.Clear();
        }
        
    }
}