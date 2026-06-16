using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Unity_TobiiGameIntegration_InputSystemAdapter.Demo
{
    /// <summary>
    /// Moves a target UI RectTransform using the EventSystem's Point action position.
    /// Useful for virtual pointers and any absolute-position source routed into Input System UI input.
    /// </summary>
    [AddComponentMenu("Ken Rampage/Unity/Input/EventSystem Point Cursor Follower")]
    public class EventSystemPointCursorFollower : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [Tooltip("Optional EventSystem. If not set, EventSystem.current is used.")]
        [SerializeField] private EventSystem _eventSystem;

        [Tooltip("UI element to move. If not set, this object's RectTransform is used.")]
        [SerializeField] private RectTransform _targetRect;

        [Header("Behavior")]
        [Tooltip("Clamp position to current screen bounds.")]
        [SerializeField] private bool _clampToScreen = true;

        private InputSystemUIInputModule _uiInputModule;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_targetRect == null)
            {
                _targetRect = GetComponent<RectTransform>();
            }
        }

        private void LateUpdate()
        {
            if (!TryGetPointPosition(out Vector2 screenPosition))
            {
                return;
            }

            if (_clampToScreen)
            {
                screenPosition.x = Mathf.Clamp(screenPosition.x, 0f, Screen.width);
                screenPosition.y = Mathf.Clamp(screenPosition.y, 0f, Screen.height);
            }

            ApplyToTarget(screenPosition);
        }

        #endregion

        #region Public API

        public bool TryGetPointPosition(out Vector2 pointScreenPosition)
        {
            pointScreenPosition = default;

            EventSystem activeEventSystem = _eventSystem != null ? _eventSystem : EventSystem.current;
            if (activeEventSystem == null)
            {
                return false;
            }

            if (_uiInputModule == null || _uiInputModule.gameObject != activeEventSystem.gameObject)
            {
                _uiInputModule = activeEventSystem.currentInputModule as InputSystemUIInputModule;
            }

            if (_uiInputModule == null || _uiInputModule.point == null || _uiInputModule.point.action == null)
            {
                return false;
            }

            pointScreenPosition = _uiInputModule.point.action.ReadValue<Vector2>();
            return true;
        }

        #endregion

        #region Helpers

        private void ApplyToTarget(Vector2 screenPosition)
        {
            if (_targetRect == null)
            {
                return;
            }

            RectTransform parentRect = _targetRect.parent as RectTransform;
            if (parentRect == null)
            {
                _targetRect.position = screenPosition;
                return;
            }

            Canvas canvas = _targetRect.GetComponentInParent<Canvas>();
            Camera uiCamera = null;

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = canvas.worldCamera;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, uiCamera, out Vector2 localPoint))
            {
                _targetRect.anchoredPosition = localPoint;
            }
        }

        #endregion
    }
}
