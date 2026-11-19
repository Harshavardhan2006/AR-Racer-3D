using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTrackPlacement : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject racingTrackPrefab;
    [SerializeField] private GameObject placementPanel;
    [SerializeField] private GameObject gameHud;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private RaceHUD hud;

    private GameObject placedTrack;

    private static readonly List<ARRaycastHit> Hits =
        new List<ARRaycastHit>();

    public bool IsTrackPlaced => placedTrack != null;

    private void Awake()
    {
        if (raycastManager == null)
            raycastManager = FindFirstObjectByType<ARRaycastManager>();

        if (planeManager == null)
            planeManager = FindFirstObjectByType<ARPlaneManager>();

        if (hud == null)
            hud = FindFirstObjectByType<RaceHUD>();

        if (gameHud != null)
            gameHud.SetActive(false);
    }

    private void Update()
    {
        if (placedTrack != null || Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        if (raycastManager == null || racingTrackPrefab == null)
            return;

        if (!raycastManager.Raycast(
                touch.position,
                Hits,
                TrackableType.PlaneWithinPolygon))
            return;

        Pose pose = Hits[0].pose;
        PlaceAt(pose.position, pose.rotation);
    }

    public void PlaceAt(Vector3 position, Quaternion rotation)
    {
        if (placedTrack != null || racingTrackPrefab == null)
            return;

        placedTrack = Instantiate(
            racingTrackPrefab,
            position,
            rotation);

        if (placementPanel != null)
            placementPanel.SetActive(false);

        if (gameHud != null)
            gameHud.SetActive(true);

        SetPlanesActive(false);

        if (hud != null)
            hud.ShowGameplay();
    }

    private void SetPlanesActive(bool value)
    {
        if (planeManager == null)
            return;

        planeManager.enabled = value;

        foreach (ARPlane plane in planeManager.trackables)
            plane.gameObject.SetActive(value);
    }

    public void ResetPlacement()
    {
        if (placedTrack != null)
            Destroy(placedTrack);

        placedTrack = null;

        if (placementPanel != null)
            placementPanel.SetActive(true);

        if (gameHud != null)
            gameHud.SetActive(false);

        SetPlanesActive(true);

        if (hud != null)
            hud.ShowPlacement();
    }
}
