using UnityEngine;
using Tobii.GameIntegration.Net;

namespace Adroit.Tobii.TGI
{
    /// <summary>
    /// Manages TGI eye tracker hardware connection and data retrieval.
    /// NOTE: Use "Free Aspect" in Unity Editor Game view for accurate tracking.
    /// Fixed aspect ratios (16:9, 4:3, etc.) add letterboxing that breaks coordinate accuracy.
    /// </summary>
    [AddComponentMenu("Adroit/Tobii/TGI/TGI Hardware Manager")]
    public class TGIHardwareManager : MonoBehaviour
    {
        #region Singleton
        [Header("Singleton")]
        [Tooltip("If enabled, this singleton persists across scene loads")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        public static TGIHardwareManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        #endregion

        #region Constants
        private const float HEAD_ANGLE_TO_SCREEN_SCALE = 0.03f;
        private const float HEAD_POSITION_TO_SCREEN_SCALE = 2000f;
        private const float TGI_MM_TO_METERS = 0.001f;
        private const float HEAD_MARKER_REFERENCE_DISTANCE = 0.7f; // meters
        private const float HEAD_MARKER_MIN_SCALE = 0.5f;
        private const float HEAD_MARKER_MAX_SCALE = 2.0f;
        [Tooltip("Show debug markers for gaze, head angle, and head position")]
        public bool showDebugMarkers = true;

        [Header("Editor Settings")]
        [Tooltip("Y-axis correction for Unity Editor's top GUI bar (pixels)")]
        public int editorYCorrection = 40;
        #endregion

        #region Private Fields
        private GazePoint _gazePoint;
        private HeadPose _headPose;
        private Vector2Int _windowPos;
        private Vector2Int _windowSize;
        #endregion

        #region Properties
        /// <summary>Latest gaze point in screen pixel coordinates</summary>
        public Vector2 GazeScreenPosition { get; private set; }

        /// <summary>Latest head angle position in screen pixel coordinates</summary>
        public Vector2 HeadAngleScreenPosition { get; private set; }

        /// <summary>Latest head position in tracker space (meters)</summary>
        public Vector3 HeadPositionMeters { get; private set; }

        /// <summary>Latest head rotation in degrees (Pitch, Yaw, Roll)</summary>
        public Vector3 HeadRotationDegrees { get; private set; }

        /// <summary>Is the TGI tracker currently connected?</summary>
        public bool IsConnected => TobiiGameIntegrationApi.IsTrackerConnected();
        #endregion

        #region Public Controls
        [ContextMenu("TGI/Debug/Show Markers")]
        public void ShowDebugMarkers()
        {
            showDebugMarkers = true;
        }

        [ContextMenu("TGI/Debug/Hide Markers")]
        public void HideDebugMarkers()
        {
            showDebugMarkers = false;
        }

        [ContextMenu("TGI/Debug/Toggle Markers")]
        public void ToggleDebugMarkers()
        {
            showDebugMarkers = !showDebugMarkers;
        }
        #endregion

        #region Unity Lifecycle
        void Update()
        {
            UpdateTrackingRect();
            UpdateTGIData();
        }

        void OnGUI()
        {
            if (!showDebugMarkers) return;
            DrawDebugVisualization();
        }
        #endregion

        #region TGI Updates
        private void UpdateTrackingRect()
        {
            var newWindowPos = Screen.mainWindowPosition;
            var newWindowSize = new Vector2Int(Screen.width, Screen.height);

            if (newWindowPos != _windowPos || newWindowSize != _windowSize)
            {
                _windowPos = newWindowPos;
                _windowSize = newWindowSize;

                int yCorrection = (!Screen.fullScreen && Application.isEditor) ? editorYCorrection : 0;

                TobiiGameIntegrationApi.TrackRectangle(new TobiiRectangle
                {
                    Left = _windowPos.x,
                    Top = _windowPos.y + yCorrection,
                    Right = _windowPos.x + _windowSize.x,
                    Bottom = _windowPos.y + yCorrection + _windowSize.y
                });
                TobiiGameIntegrationApi.Update();
            }

            TobiiGameIntegrationApi.Update();
        }

        private void UpdateTGIData()
        {
            // Get gaze data
            if (TobiiGameIntegrationApi.TryGetLatestGazePoint(out _gazePoint))
            {
                GazeScreenPosition = TGIToScreenCoordinates(_gazePoint);
            }

            // Get head pose data
            if (TobiiGameIntegrationApi.TryGetLatestHeadPose(out _headPose))
            {
                HeadPositionMeters = new Vector3(
                    _headPose.Position.X * TGI_MM_TO_METERS,
                    _headPose.Position.Y * TGI_MM_TO_METERS,
                    _headPose.Position.Z * TGI_MM_TO_METERS
                );

                HeadRotationDegrees = new Vector3(
                    _headPose.Rotation.PitchDegrees,
                    _headPose.Rotation.YawDegrees,
                    _headPose.Rotation.RollDegrees
                );

                HeadAngleScreenPosition = HeadAngleToScreenPosition(_headPose);
            }
        }
        #endregion

        #region Coordinate Conversion Helpers
        /// <summary>Converts TGI normalized coordinates to screen pixel coordinates</summary>
        private Vector2 TGIToScreenCoordinates(GazePoint gazePoint)
        {
            return new Vector2(
                (0.5f + 0.5f * gazePoint.X) * Screen.width,
                (0.5f + 0.5f * -gazePoint.Y) * Screen.height
            );
        }

        /// <summary>Calculates screen position from head angle (yaw/pitch)</summary>
        private Vector2 HeadAngleToScreenPosition(HeadPose headPose)
        {
            return new Vector2(
                Screen.width * (0.5f + headPose.Rotation.YawDegrees * HEAD_ANGLE_TO_SCREEN_SCALE),
                Screen.height * (0.5f - headPose.Rotation.PitchDegrees * HEAD_ANGLE_TO_SCREEN_SCALE)
            );
        }

        /// <summary>Calculates screen position from head X/Y position relative to tracker</summary>
        private Vector2 HeadPositionToScreenPosition(HeadPose headPose)
        {
            return new Vector2(
                Screen.width / 2f + (headPose.Position.X * TGI_MM_TO_METERS) * HEAD_POSITION_TO_SCREEN_SCALE,
                Screen.height - (headPose.Position.Y * TGI_MM_TO_METERS) * HEAD_POSITION_TO_SCREEN_SCALE
            );
        }

        /// <summary>Calculates marker scale based on distance from tracker</summary>
        private float CalculateHeadMarkerDistanceScale()
        {
            if (HeadPositionMeters.z <= 0.01f) return 1f;

            float distanceScale = HEAD_MARKER_REFERENCE_DISTANCE / HeadPositionMeters.z;
            return Mathf.Clamp(distanceScale, HEAD_MARKER_MIN_SCALE, HEAD_MARKER_MAX_SCALE);
        }
        #endregion

        #region Debug Visualization
        private void DrawDebugVisualization()
        {
            // Eye tracker origin indicator (bottom center)
            Vector2 trackerOrigin = new Vector2(Screen.width / 2f, Screen.height);
            DrawBox(trackerOrigin, 15f, Color.white);
            DrawLabel(trackerOrigin, "Eye Tracker Origin", Color.white, 10f, -20f);

            // Gaze marker
            DrawCrosshair(GazeScreenPosition, 30f, Color.green);
            DrawLabel(GazeScreenPosition, "Gaze Point", Color.green, 20f, -10f);

            // Head angle marker
            var headAnglePos = HeadAngleToScreenPosition(_headPose);
            DrawBox(headAnglePos, 25f, Color.yellow);
            DrawLabel(headAnglePos, "Head Angle", Color.yellow, 20f, -10f);

            // Head position marker (scales with distance)
            var headPos = HeadPositionToScreenPosition(_headPose);
            float distanceScale = CalculateHeadMarkerDistanceScale();
            float headMarkerSize = 20f * distanceScale;
            DrawBox(headPos, headMarkerSize, Color.cyan);
            DrawLabel(headPos, "Head Position", Color.cyan, 20f, -10f);
        }

        private void DrawCrosshair(Vector2 pos, float size, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(pos.x - size / 2, pos.y - 1, size, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(pos.x - 1, pos.y - size / 2, 2, size), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawBox(Vector2 pos, float size, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(pos.x - size / 2, pos.y - size / 2, size, size), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawLabel(Vector2 pos, string text, Color color, float offsetX, float offsetY)
        {
            GUI.color = color;
            GUI.Label(new Rect(pos.x + offsetX, pos.y + offsetY, 150, 20), text);
            GUI.color = Color.white;
        }
        #endregion
    }
}
