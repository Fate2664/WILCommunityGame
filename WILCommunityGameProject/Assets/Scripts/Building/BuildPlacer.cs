using System;
using System.Collections.Generic;
using Nova;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WILCommunityGame
{
    public class BuildPlacer : MonoBehaviour
    {
        #region Class Variables

        [Header("Prefabs & Tools")] [SerializeField]
        public BuildPieceType placementPieceType = BuildPieceType.Floor;

        [SerializeField] private GameObject floorTilePrefab;
        [SerializeField] private GameObject wallTilePrefab;
        [SerializeField] private GameObject doorTilePrefab;
        [SerializeField] private ObjectiveItem hoeGroundObjective;
        [SerializeField] private ObjectiveListUI objectiveListUI;

        [Space(10)] [Header("GroundPlacement")] [SerializeField]
        private LayerMask placementMask;

        [SerializeField] private LayerMask floorOverlapMask;
        [SerializeField] private float gridSize = 2.5f;
        [SerializeField] private bool snapFloorToGrid = true;

        [Space(10)] [Header("Socket Raycast")] [SerializeField]
        private bool useSocketSnapping = true;

        [SerializeField] private LayerMask socketRaycastMask;
        [SerializeField] private float socketRaycastDistance = 500f;
        [SerializeField] private float groundRaycastDistance = 500f;
        [SerializeField] private int socketHitBufferSize = 16;

        [Space(10)] [Header("Socket Occupancy")] [SerializeField]
        private bool skipSocketOccupied = true;

        [Space(10)] [Header("Materials")] [SerializeField]
        private Material previewMaterialValid;

        [SerializeField] private Material previewMaterialInvalid;
        [SerializeField] private float previewLiftY = 0.02f;

        private GameObject previewInstance;
        private bool destroyMode;
        private BuildPart hoveredBuild;
        private readonly List<Renderer> hoveredRenderers = new();
        private readonly List<Material[]> hoveredMaterials = new();
        private RaycastHit[] hitsBuffer;
        private readonly List<EdgeSocket> availableSockets = new(8);
        private int placementPieceTypeFrame;
        private Camera mainCamera;
        private BuildableObjectItemSO selectedBuildable;
        private UIManager placementInventory;
        private int placementStartFrame;

        #endregion

        private bool IsGroundPiece => placementPieceType == BuildPieceType.Floor ||
                                      placementPieceType == BuildPieceType.BuildableObject;

        private GameObject ActivePrefab => placementPieceType switch
        {
            BuildPieceType.Floor => floorTilePrefab,
            BuildPieceType.Wall => wallTilePrefab,
            BuildPieceType.Door => doorTilePrefab,
            BuildPieceType.BuildableObject => selectedBuildable != null ? selectedBuildable.prefab : null,
            _ => null
        };

        public void SetDestroyMode(bool value)
        {
            if (destroyMode == value) return;

            destroyMode = value;
            ClearPreview();
            ClearHoveredBuild();
        }

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

            if (floorOverlapMask.value == 0)
            {
                var sl = LayerMask.NameToLayer("Socket");
                floorOverlapMask = sl >= 0 ? ~(1 << sl) : ~0;
            }
        }

        private void Update()
        {
            if (mainCamera == null || Mouse.current == null) return;
            var mouse = Mouse.current;

            if (destroyMode)
            {
                HandleDestroyMode(mouse);
                return;
            }

            if (placementPieceType == BuildPieceType.BuildableObject)
            {
                if (!HasBuildableToPlace())
                {
                    enabled = false;
                    return;
                }

                bool cancelPressed = mouse.rightButton.wasPressedThisFrame ||
                                     (Keyboard.current != null &&
                                      Keyboard.current.escapeKey.wasPressedThisFrame);

                if (cancelPressed)
                {
                    FinishBuildablePlacement();
                    return;
                }

                if (Time.frameCount <= placementStartFrame)
                    return;
            }

            if ((int)placementPieceType != placementPieceTypeFrame)
            {
                if (previewInstance != null) Destroy(previewInstance);
                previewInstance = null;
                placementPieceTypeFrame = (int)placementPieceType;
            }

            if (!TryGetPreviewPose(mouse.position.ReadValue(), out var pose, out var placementValid,
                    out var socketSnap))
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
                GameObject prefab = ActivePrefab;
                if (prefab == null)
                    return;

                previewInstance = Instantiate(prefab);
                previewInstance.hideFlags = HideFlags.DontSave;

                foreach (Collider collider in previewInstance.GetComponentsInChildren<Collider>(true))
                {
                    collider.enabled = false;
                }

                foreach (Rigidbody body in previewInstance.GetComponentsInChildren<Rigidbody>(true))
                {
                    body.isKinematic = true;
                    body.detectCollisions = false;
                }

                foreach (MonoBehaviour behaviour in previewInstance.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour is IBuildPreview preview)
                    {
                        preview.PrepareAsPreview();
                    }
                }
            }

            previewInstance.SetActive(true);

            Vector3 position = pose.position;
            position.y += previewLiftY;
            previewInstance.transform.SetPositionAndRotation(position, pose.rotation);

            Material material = valid ? previewMaterialValid : previewMaterialInvalid;

            if (material == null)
                return;

            foreach (Renderer renderer in previewInstance.GetComponentsInChildren<Renderer>())
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private void Place(GameObject prefab, Pose pose, EdgeSocket socketSnap)
        {
            if (prefab == null || (IsGroundPiece && IsGroundPlacementOccupied(pose.position))) return;
            if (socketSnap != null && skipSocketOccupied && socketSnap.IsOccupied) return;

            bool consumesInventory = placementPieceType == BuildPieceType.BuildableObject;

            if (consumesInventory && !HasBuildableToPlace())
                return;

            UIManager inventory = placementInventory;
            GameObject placed = Instantiate(prefab, pose.position, pose.rotation);
            BuildPart part = placed.GetComponent<BuildPart>();

            if (consumesInventory)
            {
                if (part == null || !inventory.TryUseEquippedItem(1))
                {
                    placed.SetActive(false);
                    Destroy(placed);
                    return;
                }
            }

            if (socketSnap != null && part != null)
            {
                socketSnap.SetOccupant(part);
            }

            if (placementPieceType == BuildPieceType.Floor)
            {
                AudioManager.Instance.Play("Hoeing");
                objectiveListUI.CompleteItem(hoeGroundObjective);
            }

            if (consumesInventory)
            {
                FinishBuildablePlacement();
            }
        }

        private bool IsGroundPlacementOccupied(Vector3 position)
        {
            var halExtent = new Vector3(gridSize * .45f, 0.25f, gridSize * .45f);
            var center = position + Vector3.up * .1f;

            var overlaps = Physics.OverlapBox(center, halExtent, Quaternion.identity, floorOverlapMask,
                QueryTriggerInteraction.Collide);

            foreach (var overlap in overlaps)
            {
                if (overlap.GetComponent<EdgeSocket>() != null) continue;
                var part = overlap.GetComponentInParent<BuildPart>();
                if (part != null && (part.Type == BuildPieceType.Floor || part.Type == BuildPieceType.BuildableObject))
                    return true;
            }

            return false;
        }

        private void SnapGrid(ref Vector3 position)
        {
            if (gridSize <= 0f) return;
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.z = Mathf.Round(position.z / gridSize) * gridSize;
        }

        private void HandleDestroyMode(Mouse mouse)
        {
            if (!TryGetHoveredBuild(mouse.position.ReadValue(), out var build))
            {
                SetHoveredBuild(null);
                return;
            }

            SetHoveredBuild(build);

            if (mouse.leftButton.wasPressedThisFrame)
            {
                DestroyBuild(build);
            }
        }

        private void SetHoveredBuild(BuildPart build)
        {
            if (hoveredBuild == build) return;

            ClearHoveredBuild();
            hoveredBuild = build;

            if (hoveredBuild == null) return;
            foreach (var renderer in hoveredBuild.GetComponentsInChildren<Renderer>())
            {
                hoveredRenderers.Add(renderer);
                hoveredMaterials.Add(renderer.sharedMaterials);

                var invalidMaterials = new Material[renderer.sharedMaterials.Length];
                for (var i = 0; i < invalidMaterials.Length; i++)
                {
                    invalidMaterials[i] = previewMaterialInvalid;
                }

                renderer.sharedMaterials = invalidMaterials;
            }
        }

        private void DestroyBuild(BuildPart build)
        {
            if (build == null) return;

            ClearHoveredBuild();
            Destroy(build.gameObject);
        }

        #region Buildable Object Methods

        public bool BeginBuildablePlacement(BuildableObjectItemSO item, UIManager inventory)
        {
            if (item == null || item.prefab == null || inventory == null)
                return false;

            InventoryItem equipped = inventory.EquippedItem;

            if (equipped == null || equipped.item != item || equipped.count <= 0)
                return false;

            BuildPart part = item.prefab.GetComponent<BuildPart>();

            if (part == null || part.Type != BuildPieceType.BuildableObject)
                return false;

            SetDestroyMode(false);
            ClearPreview();
            ClearHoveredBuild();

            selectedBuildable = item;
            placementInventory = inventory;
            placementPieceType = BuildPieceType.BuildableObject;
            placementStartFrame = Time.frameCount;

            enabled = true;
            return true;
        }

        private bool HasBuildableToPlace()
        {
            if (selectedBuildable == null || selectedBuildable.prefab == null || placementInventory == null)
                return false;

            InventoryItem equipped = placementInventory.EquippedItem;
            return equipped != null && equipped.item == selectedBuildable && equipped.count > 0;
        }

        private void FinishBuildablePlacement()
        {
            UIManager inventory = placementInventory;
            BuildableObjectItemSO item = selectedBuildable;

            enabled = false;

            if (inventory != null && item != null && inventory.EquippedItem != null &&
                inventory.EquippedItem.item == item)
            {
                inventory.UnEquipItem();
            }
        }

        #endregion

        #region Try Methods

        private bool TryGetPreviewPose(Vector2 screenPos, out Pose pose, out bool placementValid,
            out EdgeSocket socketFromSnap)
        {
            pose = default;
            placementValid = false;
            socketFromSnap = null;

            Ray ray = mainCamera.ScreenPointToRay(screenPos);

            //Socket Snapping
            if (useSocketSnapping && socketRaycastMask.value != 0 && TrySnapSocket(ray, out pose, out socketFromSnap))
            {
                placementValid = !IsGroundPiece || !IsGroundPlacementOccupied(pose.position);
                return true;
            }

            //Hitting ground
            if (Physics.Raycast(ray, out var hit, groundRaycastDistance, placementMask, QueryTriggerInteraction.Ignore))
            {
                var position = hit.point;

                if (IsGroundPiece && snapFloorToGrid)
                {
                    SnapGrid(ref position);
                }

                pose = new Pose(position, Quaternion.identity);
                placementValid = IsGroundPiece && !IsGroundPlacementOccupied(position);
                return true;
            }

            //Horizontal Plane
            if (TryHorizontalPlane(ray, out var point))
            {
                if (IsGroundPiece && snapFloorToGrid)
                {
                    SnapGrid(ref point);
                }

                pose = new Pose(point, Quaternion.identity);
                placementValid = IsGroundPiece && !IsGroundPlacementOccupied(point);
                return true;
            }

            return false;
        }

        private bool TrySnapSocket(Ray ray, out Pose bestPose, out EdgeSocket socket)
        {
            bestPose = default;
            socket = null;
            var n = Physics.RaycastNonAlloc(ray, hitsBuffer, socketRaycastDistance, socketRaycastMask,
                QueryTriggerInteraction.Collide);
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
            var t = -ray.origin.y / ray.direction.y;
            if (t < 0f) return false;
            hit = ray.origin + ray.direction * t;
            hit.y = 0f;
            return true;
        }

        private bool TryGetHoveredBuild(Vector2 screenPos, out BuildPart build)
        {
            build = null;

            var ray = mainCamera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, groundRaycastDistance, floorOverlapMask,
                    QueryTriggerInteraction.Collide)) return false;

            build = hit.collider.GetComponent<BuildPart>();
            return build != null;
        }

        #endregion

        #region Clear Methods

        private void ClearPreview()
        {
            if (previewInstance == null) return;

            Destroy(previewInstance);
            previewInstance = null;
        }

        private void ClearHoveredBuild()
        {
            for (var i = 0; i < hoveredRenderers.Count; i++)
            {
                if (hoveredRenderers[i] != null)
                {
                    hoveredRenderers[i].sharedMaterials = hoveredMaterials[i];
                }
            }

            hoveredBuild = null;
            hoveredRenderers.Clear();
            hoveredMaterials.Clear();
        }

        private void OnDisable()
        {
            ClearPreview();
            ClearHoveredBuild();
            selectedBuildable = null;
            placementInventory = null;
        }

        private void OnDestroy()
        {
            ClearPreview();
            ClearHoveredBuild();
        }

        #endregion
    }
}