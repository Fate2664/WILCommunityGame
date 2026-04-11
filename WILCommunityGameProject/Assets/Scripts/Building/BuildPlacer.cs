using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WILCommunityGame
{
    public class BuildPlacer : MonoBehaviour
    {
        #region Class Variables

        [Header("Prefabs & Tools")]
        [SerializeField] private BuildPieceType placementPieceType = BuildPieceType.Floor;
        [SerializeField] private GameObject floorTilePrefab;
        [SerializeField] private GameObject wallTilePrefab;
        [SerializeField] private GameObject doorTilePrefab;

        [Space(10)] 
        [Header("GroundPlacement")] 
        [SerializeField] private LayerMask placementMask;
        [SerializeField] private float gridSize = 2.5f;
        [SerializeField] private bool snapFloorToGrid = true;
        
        [Space(10)]
        [Header("Socket Raycast")]
        [SerializeField] private bool useSocketSnapping = true;
        [SerializeField] private LayerMask socketRaycastMask;
        [SerializeField] private float socketRaycastDistance = 500f;
        [SerializeField] private float groundRaycastDistance = 500f;
        [SerializeField] private int socketHitBufferSize = 16;
        
        [Space(10)]
        [Header("Socket Occupany")]
        [SerializeField] private bool skipSocketOccupied = true;

        [Space(10)] [Header("Header")] 
        [SerializeField] private Material previewMaterialValid;
        [SerializeField] private Material previewMaterialGhost;
        [SerializeField] private float previewLiftY = 0.02f;

        private GameObject previewInstance;
        private RaycastHit[] hitsBuffer;
        private readonly List<EdgeSocket> availableSockets = new(8);
        private int placementPieceTypeFrame;
        private Camera mainCamera;

        #endregion

        private GameObject ActivePrefab => placementPieceType switch
        {
            BuildPieceType.Floor => floorTilePrefab,
            BuildPieceType.Wall => wallTilePrefab,
            BuildPieceType.Door => doorTilePrefab
        };

        private void Awake()
        {
            mainCamera = Camera.main;
            hitsBuffer = new RaycastHit[Mathf.Max(1, socketHitBufferSize)];
            if (socketRaycastMask.value == 0) socketRaycastMask = LayerMask.GetMask("Socket");
            if (placementMask.value == 0)
            {
                var sl = LayerMask.NameToLayer("Socket");
                placementMask = sl >= 0 ? ~(1 << sl) : ~0;
            }
        }

        private void Update()
        {
            if (mainCamera == null || Mouse.current == null) return;
            var mouse = Mouse.current;

            if ((int)placementPieceType != placementPieceTypeFrame)
            {
                if (previewInstance != null) Destroy(previewInstance);
                previewInstance = null;
                placementPieceTypeFrame = (int)placementPieceType;
            }

            if (!TryGetPreviewPose(mouse.position.ReadValue(), out var pose, out var placementValid, out var socketSnap))
            {
                if (previewInstance != null) previewInstance.SetActive(false);
                return;
            }
            
            UpdatePreview(pose, placementValid);
            if (mouse.leftButton.wasPressedThisFrame && placementValid) Place(ActivePrefab, pose, socketSnap);
        }

        private void UpdatePreview(Pose pose, bool valid)
        {
            if (previewInstance == null)
            {
                var p = ActivePrefab;
                if (p == null) return;
                previewInstance = Instantiate(p);
                previewInstance.hideFlags = HideFlags.DontSave;
                foreach (var c in previewInstance.GetComponentsInChildren<Collider>()) c.enabled = false;
            }
            
            var pos = pose.position;
            pos.y += previewLiftY;
            previewInstance.transform.SetPositionAndRotation(pos, pose.rotation);
            
            var mat = valid ?  previewMaterialValid : previewMaterialGhost;
            if (mat == null) return;
            foreach (var r in previewInstance.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
        }

        private bool TryGetPreviewPose(Vector2 screenPos, out Pose pose, out bool placementValid, out EdgeSocket socketFromSnap)
        {
            pose = default;
            placementValid = false;
            socketFromSnap = null;
            
            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            
            //Socket Snapping
            if (useSocketSnapping && socketRaycastMask.value != 0 && TrySnapSocket(ray, out pose, out socketFromSnap))
            {
                placementValid = true;
                return true;
            }
            
            //Hitting ground
            if (Physics.Raycast(ray, out var hit, groundRaycastDistance, placementMask, QueryTriggerInteraction.Ignore))
            {
                var position = hit.point;

                if (placementPieceType == BuildPieceType.Floor)
                {
                    if (snapFloorToGrid) SnapGrid(ref position);
                    placementValid = true;
                }
                
                pose = new Pose(position, Quaternion.identity);
                return true;
            }
            
            //Horizontal Plane
            if (TryHorizontalPlane(ray, out var point))
            {
                if (placementPieceType == BuildPieceType.Floor)
                {
                    if (snapFloorToGrid) SnapGrid(ref point);
                    placementValid = true;
                }
                
                pose = new Pose(point, Quaternion.identity);
                return true;
            }
            
            return false;
        }

        private void Place(GameObject prefab, Pose pose, EdgeSocket socketSnap)
        {
            if (prefab == null) return;
            var go = Instantiate(prefab, pose.position, pose.rotation);
            if (socketSnap == null) return;
            var part = go.GetComponent<BuildPart>();
            if (part != null) socketSnap.SetOccupant(part);
        }

        private void SnapGrid(ref Vector3 position)
        {
            if (gridSize <= 0f) return;
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.z = Mathf.Round(position.z / gridSize) * gridSize;
        }

        private bool TrySnapSocket(Ray ray, out Pose bestPose, out EdgeSocket socket)
        {
            bestPose = default;
            socket = null;
            var n = Physics.RaycastNonAlloc(ray, hitsBuffer, socketRaycastDistance, socketRaycastMask,  QueryTriggerInteraction.Collide);
            if (n <= 0) return false;

            var best = float.MaxValue;
            var found = false;

            for (var i = 0; i < n; i++)
            {
                var h = hitsBuffer[i];
                if (h.collider == null) continue;
                
                availableSockets.Clear();
                h.collider.GetComponents(availableSockets);
                EdgeSocket edge = null;

                for (var j = 0; j < availableSockets.Count; j++)
                {
                    var s = availableSockets[j];
                    if (s.CanAcceptPart(placementPieceType))
                    {
                        edge = s;
                        break;
                    }
                }

                if (edge == null) continue;
                if (skipSocketOccupied && edge.IsOccupied) continue;

                var candidate = edge.GetSnapPose();

                if (h.distance < best)
                {
                    best = h.distance;
                    bestPose = candidate;
                    socket = edge;
                    found = true;
                }
            }
            return found;
        }

        private static bool TryHorizontalPlane(Ray ray, out Vector3 hit)
        {
            hit = default;
            if (Mathf.Abs(ray.direction.y) < 1e-5f) return false;
            var t = -ray.origin.y /  ray.direction.y;
            if (t < 0f) return false;
            hit = ray.origin + ray.direction * t;
            hit.y = 0f;
            return true;
        }
    }
}
