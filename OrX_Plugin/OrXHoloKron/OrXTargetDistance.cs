﻿using System;
using UnityEngine;
using System.Collections;
using FinePrint;
using System.Collections.Generic;
using System.Linq;
using OrX.spawn;

namespace OrX
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class OrXTargetDistance : MonoBehaviour
    {
        public static OrXTargetDistance instance;
        public double mPerDegree = 0;
        public double degPerMeter = 0;
        public float scanDelay = 0;

        Vector3 UpVect;
        Vector3 EastVect;
        Vector3 NorthVect;
        Vector3 targetVect;

        double targetDistance = 250000;
        double _latDiff = 0;
        double _lonDiff = 0;
        double _altDiff = 0;
        double _latMission = 0;
        double _lonMission = 0;
        double _altMission = 0;

        private void Awake()
        {
            if (instance) Destroy(instance);
            instance = this;
        }

        public void TargetDistance(bool primary, bool b, bool Goal, bool checking, Vector3d missionCoords)
        {
            if (!OrXHoloKron.instance.buildingMission)
            {
                StartCoroutine(CheckTargetDistance(primary, b, Goal, checking, "", missionCoords));
            }
        }
        IEnumerator CheckTargetDistance(bool primary, bool b, bool Goal, bool checking, string HoloKronName, Vector3d missionCoords)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                yield return new WaitForFixedUpdate();
                mPerDegree = (((2 * (FlightGlobals.ActiveVessel.mainBody.Radius + FlightGlobals.ActiveVessel.altitude)) * Math.PI) / 360);
                degPerMeter = 1 / mPerDegree;
                scanDelay = 5;

                string hcn = "";
                int coordCount = 0;

                if (FlightGlobals.ActiveVessel.srfSpeed >= OrXHoloKron.instance.topSurfaceSpeed)
                {
                    OrXHoloKron.instance.topSurfaceSpeed = FlightGlobals.ActiveVessel.srfSpeed;
                }

                if (!b)
                {
                    Debug.Log("[OrX Holo Distance Check] Loading HoloKron Targets ..........");

                    if (OrXHoloKron.instance.OrXCoordsList.Count >= 0)
                    {
                        Debug.Log("[OrX Holo Distance Check] OrX Coords List Count = " + OrXHoloKron.instance.OrXCoordsList.Count + " ..........");

                        List<string>.Enumerator coordinate = OrXHoloKron.instance.OrXCoordsList.GetEnumerator();
                        while (coordinate.MoveNext())
                        {
                            try
                            {
                                Debug.Log("[OrX Holo Distance Check] Checking: " + coordinate.Current + " ..........");

                                string[] targetHoloKrons = coordinate.Current.Split(new char[] { ':' });

                                if (targetHoloKrons[0] != null && targetHoloKrons[0].Length > 0 && targetHoloKrons[0] != "null")
                                {
                                    coordCount += 1;

                                    string[] TargetCoords = targetHoloKrons[0].Split(new char[] { ';' });
                                    for (int i = 0; i < TargetCoords.Length; i++)
                                    {
                                        if (TargetCoords[i] != null && TargetCoords[i].Length > 0)
                                        {
                                            string[] data = TargetCoords[i].Split(new char[] { ',' });
                                            HoloKronName = data[1];
                                            _latMission = double.Parse(data[3]);
                                            _lonMission = double.Parse(data[4]);
                                            _altMission = double.Parse(data[5]);
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.Log("[OrX Holo Distance Check] " + coordinate.Current + " was empty ..........");

                                }
                            }
                            catch
                            {
                                Debug.Log("[OrX Load HoloKron Targets] HoloKron data processed ...... ");
                            }

                            yield return new WaitForFixedUpdate();

                            if (FlightGlobals.ActiveVessel.altitude <= _altMission)
                            {
                                _altDiff = _altMission - FlightGlobals.ActiveVessel.altitude;
                            }
                            else
                            {
                                _altDiff = FlightGlobals.ActiveVessel.altitude - _altMission;
                            }

                            if (_latMission >= 0)
                            {
                                if (FlightGlobals.ActiveVessel.latitude >= _latMission)
                                {
                                    _latDiff = FlightGlobals.ActiveVessel.latitude - _latMission;
                                }
                                else
                                {
                                    _latDiff = _latMission - FlightGlobals.ActiveVessel.latitude;
                                }
                            }
                            else
                            {
                                if (FlightGlobals.ActiveVessel.latitude >= 0)
                                {
                                    _latDiff = FlightGlobals.ActiveVessel.latitude - _latMission;
                                }
                                else
                                {
                                    if (FlightGlobals.ActiveVessel.latitude <= _latMission)
                                    {
                                        _latDiff = FlightGlobals.ActiveVessel.latitude - _latMission;
                                    }
                                    else
                                    {

                                        _latDiff = _latMission - FlightGlobals.ActiveVessel.latitude;
                                    }
                                }
                            }

                            if (_lonMission >= 0)
                            {
                                if (FlightGlobals.ActiveVessel.longitude >= _lonMission)
                                {
                                    _lonDiff = FlightGlobals.ActiveVessel.longitude - _lonMission;
                                }
                                else
                                {
                                    _lonDiff = _lonMission - FlightGlobals.ActiveVessel.latitude;
                                }
                            }
                            else
                            {
                                if (FlightGlobals.ActiveVessel.longitude >= 0)
                                {
                                    _lonDiff = FlightGlobals.ActiveVessel.longitude - _lonMission;
                                }
                                else
                                {
                                    if (FlightGlobals.ActiveVessel.longitude <= _lonMission)
                                    {
                                        _lonDiff = FlightGlobals.ActiveVessel.longitude - _lonMission;
                                    }
                                    else
                                    {

                                        _lonDiff = _lonMission - FlightGlobals.ActiveVessel.longitude;
                                    }
                                }
                            }

                            double diffSqr = (_latDiff * _latDiff) + (_lonDiff * _lonDiff);
                            double _altDiffDeg = _altDiff * degPerMeter;
                            double altAdded = (_altDiffDeg * _altDiffDeg) + diffSqr;
                            double _targetDistance = Math.Sqrt(altAdded) * mPerDegree;

                            if (targetDistance >= _targetDistance)
                            {
                                targetDistance = _targetDistance;
                                hcn = HoloKronName;
                            }
                        }
                        coordinate.Dispose();

                        Debug.Log("[OrX Target Distance] === HOLOKRONS FOUND: " + coordCount);


                        List<string>.Enumerator getClosestCoord = OrXHoloKron.instance.OrXCoordsList.GetEnumerator();
                        while (getClosestCoord.MoveNext())
                        {
                            string[] targetHoloKrons = getClosestCoord.Current.Split(new char[] { ':' });
                            try
                            {
                                if (targetHoloKrons[0] != null && targetHoloKrons[0].Length > 0 && targetHoloKrons[0] != "null")
                                {
                                    string[] TargetCoords = targetHoloKrons[0].Split(new char[] { ';' });
                                    for (int i = 0; i < TargetCoords.Length; i++)
                                    {
                                        if (TargetCoords[i] != null && TargetCoords[i].Length > 0)
                                        {
                                            string[] data = TargetCoords[i].Split(new char[] { ',' });
                                            if (data[1] == hcn)
                                            {
                                                HoloKronName = data[1];
                                                _latMission = double.Parse(data[3]);
                                                _lonMission = double.Parse(data[4]);
                                                _altMission = double.Parse(data[5]);

                                                if (targetDistance <= 100000)
                                                {
                                                    Debug.Log("[OrX Target Distance] === TARGET Name: " + HoloKronName);
                                                    Debug.Log("[OrX Target Distance] === _latMission: " + _latMission);
                                                    Debug.Log("[OrX Target Distance] === _lonMission: " + _lonMission);
                                                    Debug.Log("[OrX Target Distance] === _altMission: " + _altMission);
                                                    Debug.Log("[OrX Target Distance] === TARGET Distance in Meters: " + targetDistance);
                                                    b = true;
                                                }
                                                else
                                                {
                                                    Debug.Log("[OrX Holo Distance Check] === NO HOLOKRONS IN RANGE ===");
                                                    OrXHoloKron.instance.targetDistance = 113800;
                                                    scanDelay = 5;
                                                    b = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Debug.Log("[OrX Load HoloKron Targets] HoloKron data processed ...... ");
                            }

                            yield return new WaitForFixedUpdate();
                        }
                        getClosestCoord.Dispose();
                    }
                    else
                    {
                        Debug.Log("[OrX Holo Distance Check] === NO HOLOKRON FOUND ===");
                        OrXHoloKron.instance.targetDistance = 11381138;
                        checking = false;
                    }
                }
                else
                {
                    coordCount += 1;

                    if (FlightGlobals.ActiveVessel.altitude <= _altMission)
                    {
                        _altDiff = _altMission - FlightGlobals.ActiveVessel.altitude;
                    }
                    else
                    {
                        _altDiff = FlightGlobals.ActiveVessel.altitude - _altMission;
                    }

                    if (_latMission >= 0)
                    {
                        if (FlightGlobals.ActiveVessel.latitude >= _latMission)
                        {
                            _latDiff = FlightGlobals.ActiveVessel.latitude - _latMission;
                        }
                        else
                        {
                            _latDiff = _latMission - FlightGlobals.ActiveVessel.latitude;
                        }
                    }
                    else
                    {
                        if (FlightGlobals.ActiveVessel.latitude >= 0)
                        {
                            _latDiff = FlightGlobals.ActiveVessel.latitude - _latMission;
                        }
                        else
                        {
                            if (FlightGlobals.ActiveVessel.latitude <= _latMission)
                            {
                                _latDiff = FlightGlobals.ActiveVessel.latitude - _latMission;
                            }
                            else
                            {

                                _latDiff = _latMission - FlightGlobals.ActiveVessel.latitude;
                            }
                        }
                    }

                    if (_lonMission >= 0)
                    {
                        if (FlightGlobals.ActiveVessel.longitude >= _lonMission)
                        {
                            _lonDiff = FlightGlobals.ActiveVessel.longitude - _lonMission;
                        }
                        else
                        {
                            _lonDiff = _lonMission - FlightGlobals.ActiveVessel.latitude;
                        }
                    }
                    else
                    {
                        if (FlightGlobals.ActiveVessel.longitude >= 0)
                        {
                            _lonDiff = FlightGlobals.ActiveVessel.longitude - _lonMission;
                        }
                        else
                        {
                            if (FlightGlobals.ActiveVessel.longitude <= _lonMission)
                            {
                                _lonDiff = FlightGlobals.ActiveVessel.longitude - _lonMission;
                            }
                            else
                            {

                                _lonDiff = _lonMission - FlightGlobals.ActiveVessel.longitude;
                            }
                        }
                    }

                    double diffSqr = (_latDiff * _latDiff) + (_lonDiff * _lonDiff);
                    double _altDiffDeg = _altDiff * degPerMeter;
                    double altAdded = (_altDiffDeg * _altDiffDeg) + diffSqr;
                    double _targetDistance = Math.Sqrt(altAdded) * mPerDegree;

                    targetDistance = _targetDistance;

                    if (_targetDistance <= FlightGlobals.ActiveVessel.vesselRanges.landed.load * 0.9)
                    {
                        Vector3d stageStartCoords = OrXSpawnHoloKron.instance.WorldPositionToGeoCoords(new Vector3d(_latMission, _lonMission, _altMission), FlightGlobals.currentMainBody);

                        Debug.Log("[OrX Target Distance - Goal] === TARGET Name: " + HoloKronName);
                        Debug.Log("[OrX Target Distance - Goal] === TARGET Distance in Meters: " + _targetDistance);
                        OrXHoloKron.instance.OrXHCGUIEnabled = false;
                        OrXHoloKron.instance.checking = false;
                        CheckIfHoloSpawned(HoloKronName, stageStartCoords, missionCoords, primary, Goal);
                    }
                    else
                    {
                        OrXHoloKron.instance.OrXHCGUIEnabled = true;
                        //OrXHoloKron.instance.challengeRunning = Goal;
                    }
                }

                if (coordCount == 0)
                {
                    Debug.Log("[OrX Holo Distance Check] === NO HOLOKRON IN RANGE ===");
                    OrXHoloKron.instance.targetDistance = 113800;
                    //checking = false;
                    OrXHoloKron.instance.ScreenMsg("No HoloKrons in range ......");
                    scanDelay = 5;
                }

                if (OrXHoloKron.instance.checking)
                {
                    OrXHoloKron.instance.movingCraft = false;
                    OrXHoloKron.instance.targetDistance = targetDistance;
                    OrXHoloKron.instance._altitude = _altMission;

                    if (!b)
                    {
                        scanDelay = Convert.ToSingle(targetDistance / FlightGlobals.ActiveVessel.srfSpeed) / 10;
                        if (scanDelay >= 5)
                        {
                            scanDelay = 5;
                        }

                        while (scanDelay >= 0)
                        {
                            yield return new WaitForSeconds(1);
                            scanDelay -= 1;
                            OrXHoloKron.instance.scanDelay = scanDelay;
                        }
                        StartCoroutine(CheckTargetDistance(primary, b, Goal, checking, HoloKronName, missionCoords));

                    }
                    else
                    {
                        missionCoords = new Vector3d(_latMission, _lonMission, _altMission);
                        yield return new WaitForFixedUpdate();
                        StartCoroutine(CheckTargetDistance(primary, b, Goal, checking, HoloKronName, missionCoords));
                    }
                }
                else
                {

                }
            }
        }
        public void CheckIfHoloSpawned(string HoloKronName, Vector3d stageStartCoords, Vector3d vect, bool primary, bool Goal)
        {
            bool s = false;
            bool rescan = false;
            double _latDiff = 0;
            double _lonDiff = 0;
            double _altDiff = 0;
            Debug.Log("[OrX Target Distance - Spawn Check] === Checking if spawned ===");

            List<Vessel>.Enumerator v = FlightGlobals.Vessels.GetEnumerator();
            while (v.MoveNext())
            {
                try
                {
                    if (v.Current != null && v.Current.loaded && !v.Current.packed)
                    {
                        if (v.Current.name == name)
                        {
                            if (vect.z <= v.Current.altitude)
                            {
                                _altDiff = v.Current.altitude - vect.z;
                            }
                            else
                            {
                                _altDiff = vect.z - v.Current.altitude;
                            }

                            if (v.Current.altitude >= 0)
                            {
                                if (vect.x >= v.Current.latitude)
                                {
                                    _latDiff = vect.x - v.Current.latitude;
                                }
                                else
                                {
                                    _latDiff = v.Current.latitude - vect.x;
                                }
                            }
                            else
                            {
                                if (vect.x >= 0)
                                {
                                    _latDiff = vect.x - v.Current.latitude;
                                }
                                else
                                {
                                    if (vect.x <= v.Current.latitude)
                                    {
                                        _latDiff = vect.x - v.Current.latitude;
                                    }
                                    else
                                    {

                                        _latDiff = v.Current.latitude - vect.x;
                                    }
                                }
                            }

                            if (v.Current.longitude >= 0)
                            {
                                if (vect.y >= v.Current.longitude)
                                {
                                    _lonDiff = vect.y - v.Current.longitude;
                                }
                                else
                                {
                                    _lonDiff = v.Current.longitude - vect.y;
                                }
                            }
                            else
                            {
                                if (vect.y >= 0)
                                {
                                    _lonDiff = vect.y - v.Current.longitude;
                                }
                                else
                                {
                                    if (vect.y <= v.Current.longitude)
                                    {
                                        _lonDiff = vect.y - v.Current.longitude;
                                    }
                                    else
                                    {

                                        _lonDiff = v.Current.latitude - vect.y;
                                    }
                                }
                            }

                            double diffSqr = (_latDiff * _latDiff) + (_lonDiff * _lonDiff);
                            double _altDiffDeg = _altDiff * degPerMeter;
                            double altAdded = (_altDiffDeg * _altDiffDeg) + diffSqr;
                            double _targetDistance = Math.Sqrt(altAdded) * mPerDegree;

                            if (_targetDistance <= 5)
                            {
                                s = true;
                                break;
                            }
                            else
                            {

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    rescan = true;
                    Debug.Log("[OrX Holo Spawn Check] ERROR -" + e + " ...... ");
                }
            }
            v.Dispose();

            if (!s)
            {
                Debug.Log("[OrX Holo Spawn Check] " + HoloKronName + " " + OrXHoloKron.instance.hkCount + " has not been spawned ...... SPAWNING");
                if (primary)
                {
                    OrXSpawnHoloKron.instance.StartSpawn(stageStartCoords, vect, Goal, false, primary, HoloKronName, OrXHoloKron.instance.missionType);
                }
                else
                {
                    //OrXHoloKron.instance.OrXHCGUIEnabled = false;
                    OrXSpawnHoloKron.instance.StartSpawn(stageStartCoords, vect, true, false, false, HoloKronName, OrXHoloKron.instance.missionType);
                }
            }
            else
            {
                if (rescan)
                {
                    Debug.Log("[OrX Holo Spawn Check] ERROR - RETRYING SPAWN CHECK ...... ");
                    CheckIfHoloSpawned(HoloKronName, stageStartCoords, vect, primary, Goal);
                }
                else
                {
                    Debug.Log("[OrX Holo Spawn Check] " + HoloKronName + " " + OrXHoloKron.instance.hkCount + " has already been spawned ...... ");
                }
            }
        }
    }
}