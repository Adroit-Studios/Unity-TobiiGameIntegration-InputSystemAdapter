using UnityEngine;

namespace Adroit.Tobii.TGI
{
    /// <summary>
    /// Moves a UI object to follow the TGI gaze position.
    /// Attach to a UI element (like an Image) to create a gaze-controlled cursor.
    /// </summary>
    [AddComponentMenu("Adroit/Tobii/TGI/TGI Gaze Cursor")]
    [RequireComponent(typeof(RectTransform))]
    public class TGIGazeCursor : MonoBehaviour
    {
        #region Fields
        private RectTransform _rectTransform;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (TGIHardwareManager.Instance == null) return;

            Vector2 screenPos = TGIHardwareManager.Instance.GazeScreenPosition;
            screenPos.y = Screen.height - screenPos.y;
            _rectTransform.position = screenPos;
        }
        #endregion
    }
}
