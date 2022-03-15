using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Modules.Starter;
using UnityEngine;
using UnityEngine.XR;

namespace Modules.Scenario
{
    public class ContactPointManager : MonoBehaviour
    {
        [SerializeField] private AreaTarget exitTarget;
        [SerializeField] private List<AreaTarget> areaTargets;
        [SerializeField] private GameObject pointsRoot;
        [SerializeField] private ContactPoint pointsPrefab;
        public AreaTarget menuTarget;
        public Dictionary<string, ContactPoint> contactPoints;
        public Dictionary<string, string> pointBoneMapper;
        public List<GameObject> currentlyActivePoints;
        private bool isShown;
        private Camera cam;
        private List<string> _createdPoints;
        private List<string> _currentPointsList;

        private void Awake()
        {
            contactPoints = new Dictionary<string, ContactPoint>();
            currentlyActivePoints = new List<GameObject>();
            _createdPoints = new List<string>();

            var pointDict = BookDatabase.Instance.MedicalBook.pointById;
            pointBoneMapper = pointDict.ToDictionary(key => key.Key, value => value.Value.humanBone);

            foreach (var point in pointDict)
            {
                var contactPoint = Instantiate(pointsPrefab, pointsRoot.transform);
                contactPoint.pointName = point.Key;
                contactPoint.bone = point.Value.bone;
                contactPoint.humanBone = point.Value.humanBone;
                contactPoint.name = point.Key;
                contactPoint.transform.position = new Vector3(point.Value.posX, point.Value.posY, point.Value.posZ);
                contactPoint.transform.eulerAngles = new Vector3(point.Value.rotX, point.Value.rotY, point.Value.rotZ);
                contactPoints.Add(point.Key, contactPoint);
            }
            exitTarget.SetSize(0.12f);
            menuTarget.SetSize(0.12f);
            GameManager.Instance.checkTableController.onTriggerChange = ReloadPoints;
        }

        public void ActivatePoints(List<string> pointsToActivate)
        {
            _currentPointsList = pointsToActivate;

            cam = GameManager.Instance.starterController.GetCamera();

            var currentTrigger = GameManager.Instance.scenarioController.GetCurrentTrigger();
            var i = 0;

            if (currentTrigger.Item1.requiredAction.Values.FirstOrDefault().checkUpTrigger != null &&
                    currentTrigger.Item2 == null)
            {
                FilterPoints(pointsToActivate, out pointsToActivate);
                CheckAreaTargets(pointsToActivate, areaTargets);
            }
            else
            {
                CreateExtraAreaTargets(pointsToActivate.Count, areaTargets.Count, areaTargets);
            }

            foreach (var pointId in pointsToActivate)
            {
                contactPoints.TryGetValue(pointId, out var contactPoint);
                if (contactPoint == null) continue;
                var areaTarget = exitTarget;
                if (pointId == "EXIT") areaTarget = exitTarget;
                else if (pointId == "MENU") areaTarget = menuTarget;
                else areaTarget = areaTargets[i];
                areaTarget.transform.position = contactPoint.transform.position;
                areaTarget.transform.eulerAngles = contactPoint.transform.eulerAngles;
                areaTarget.transform.Translate(areaTargets[i].transform.up * 0.008f, Space.World);
                areaTarget.TurnOn(cam);
                areaTarget.transform.SetParent(contactPoint.GetParent());
                areaTarget.name = contactPoint.pointName;
                currentlyActivePoints.Add(areaTarget.gameObject);
                if (pointId != "EXIT" && pointId != "MENU") i++;
                _createdPoints.Add(pointId);
            }
        }

        public ContactPoint GetPoint(string pointName)
        {
            return contactPoints[pointName];
        }

        public void ActivateCurrentSet(bool val)
        {
            if (!isShown && val)
                ActivateCurrentSetHelper(true);
            else if (isShown && !val)
                ActivateCurrentSetHelper(false);
        }

        private void ActivateCurrentSetHelper(bool val)
        {
            isShown = val;
            foreach (var currentlyActivePoint in currentlyActivePoints)
            {
                if (currentlyActivePoint != null)
                    currentlyActivePoint.SetActive(val);
            }
        }

        private void CreateExtraAreaTargets(int pointCount, int targetCount, List<AreaTarget> areaTargets)
        {
            if (pointCount > targetCount)
            {
                var parent = areaTargets[0].transform.parent;
                var obj = areaTargets[0].gameObject;

                for (var i = 0; i < pointCount - targetCount; i++)
                {
                    var newAreaTarget = Instantiate(obj, parent).GetComponent<AreaTarget>();
                    areaTargets.Add(newAreaTarget);
                }
            }

            foreach (var areaTarget in areaTargets)
            {
                areaTarget.gameObject.SetActive(false);
            }

            currentlyActivePoints.Clear();
            isShown = false;
        }



        private void CheckAreaTargets(List<string> pointsToActivate, List<AreaTarget> areaTargets)
        {
            currentlyActivePoints.Clear();

            var parent = areaTargets[0].transform.parent;
            var obj = areaTargets[0].gameObject;

            foreach (var point in pointsToActivate)
            {
                if (_createdPoints.Contains(point))
                    continue;

                var newAreaTarget = Instantiate(obj, parent).GetComponent<AreaTarget>();
                areaTargets.Add(newAreaTarget);
            }

            foreach (var areaTarget in areaTargets)
            {
                areaTarget.gameObject.SetActive(false);
            }

            isShown = false;
        }

        public void FilterPoints(List<string> pointsToActivate, out List<string> filteredPoints)
        {
            var currentTrigger = GameManager.Instance.scenarioController.GetCurrentTrigger();
            var triggers = new List<ScenarioController.Trigger>();
            var distractorsAmount = 5;

            filteredPoints = new List<string> { "EXIT", "MENU" };

            if (currentTrigger.Item1 != null)
                triggers.Add(currentTrigger.Item1);
            if (currentTrigger.Item2 != null)
                triggers.Add(currentTrigger.Item2);

            foreach (var trigger in triggers)
            {
                var checkUps = trigger.requiredAction.Values.Select(x => x.checkUpTrigger);

                if (checkUps != null)
                {
                    foreach (var checkUp in checkUps)
                    {
                        var physicalPoints = checkUp.GetPointInfo().values;
                        distractorsAmount -= 1;

                        foreach (var item in pointsToActivate)
                        {
                            if (physicalPoints.Contains(item))
                                filteredPoints.Add(item);
                        }
                    }

                }
            }

            if (GameManager.Instance.scenarioController.GetMode() != ScenarioModel.Mode.Selfcheck)
            {
                if (pointsToActivate.Count < 1)
                    return;

                var distractorsAdded = 0;
                for (int i = 0; i < distractorsAmount; i++)
                {
                    if (distractorsAdded > 2) break;
                    if (pointsToActivate.Count - filteredPoints.Count < 1) break;
                    var points = GetRandomPoints(pointsToActivate, filteredPoints);
                    filteredPoints.AddRange(points);
                    distractorsAdded += points.Count;
                }
            }
        }

        public void ReloadPoints()
        {
            if (_currentPointsList == null)
                return;
            ActivateCurrentSet(false);
            ActivatePoints(_currentPointsList);
            ActivateCurrentSet(true);
        }

        private List<string> GetRandomPoints(List<string> pointsToActivate, List<string> filteredPoints)
        {
            var randomPoint = pointsToActivate[Random.Range(0, pointsToActivate.Count)];

            return filteredPoints.Contains(randomPoint) ?
                GetRandomPoints(pointsToActivate, filteredPoints) :
                GameManager.Instance.physicalExamController.GetCheckUpsByPointId(randomPoint).FirstOrDefault().GetPointInfo().values;
        }
    }
}
