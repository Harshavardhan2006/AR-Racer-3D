using UnityEngine;

public class TrackResetButton : MonoBehaviour
{
    [SerializeField] private ARTrackPlacement placement;
    [SerializeField] private RaceManager raceManager;

    public void ResetTrack()
    {
        if (raceManager != null)
            raceManager.ResetRace();

        if (placement != null)
            placement.ResetPlacement();
    }
}
