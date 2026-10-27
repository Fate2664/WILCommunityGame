using System.Collections.Generic;
using UnityEngine;

namespace WILCommunityGame
{
    public class Sprinkler : MonoBehaviour, IBuildPreview
    {
        [SerializeField] private Transform placementPoint;
        [SerializeField, Min(0.1f)] private float plotSpacing = 2.7f;
        [SerializeField, Min(0f)] private float heightTolerance = 0.5f;

        private static readonly List<Sprinkler> activeSprinklers = new();
        private PlotSocket plotSocket;
        private bool isPreview;

        private void Start()
        {
            if (!isPreview && plotSocket == null)
                AttachToPlot(GetComponentInParent<PlotSocket>());
        }

        private void OnEnable()
        {
            if (!isPreview && plotSocket != null && !activeSprinklers.Contains(this))
                activeSprinklers.Add(this);
        }

        private void OnDisable() => activeSprinklers.Remove(this);

        public Pose GetPlacementPose(Pose socketPose)
        {
            if (placementPoint == null) return socketPose;

            Vector3 offset = Vector3.Scale(transform.InverseTransformPoint(placementPoint.position), transform.localScale);
            return new Pose(socketPose.position - socketPose.rotation * offset, socketPose.rotation);
        }

        public void AttachToPlot(PlotSocket socket)
        {
            if (isPreview || socket == null || socket.Plot == null) return;

            BuildPart part = GetComponent<BuildPart>();
            if (part == null || (socket.IsOccupied && socket.Occupant != part)) return;

            if (plotSocket != null && plotSocket.Occupant == part)
                plotSocket.SetOccupant(null);

            plotSocket = socket;
            plotSocket.SetOccupant(part);
            Pose pose = GetPlacementPose(socket.GetSnapPose());
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            transform.SetParent(socket.transform, true);

            if (isActiveAndEnabled && !activeSprinklers.Contains(this))
                activeSprinklers.Add(this);

            foreach (LandManager plot in FindObjectsByType<LandManager>(FindObjectsSortMode.None))
            {
                if (CoversPlot(plot))
                    plot.WaterPlot();
            }
        }

        private bool CoversPlot(LandManager plot)
        {
            if (isPreview || !isActiveAndEnabled || plotSocket == null || plotSocket.Plot == null || plot == null)
                return false;

            Transform center = plotSocket.Plot.transform;
            Vector3 offset = Quaternion.Inverse(center.rotation) * (plot.transform.position - center.position);
            float halfSize = plotSpacing * 1.5f;

            return Mathf.Abs(offset.x) < halfSize && Mathf.Abs(offset.z) < halfSize &&
                   Mathf.Abs(offset.y) <= heightTolerance;
        }

        public static bool IsPlotCovered(LandManager plot)
        {
            foreach (Sprinkler sprinkler in activeSprinklers)
            {
                if (sprinkler != null && sprinkler.CoversPlot(plot))
                    return true;
            }

            return false;
        }

        public void PrepareAsPreview()
        {
            isPreview = true;
            activeSprinklers.Remove(this);

            foreach (ParticleSystem particles in GetComponentsInChildren<ParticleSystem>(true))
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
