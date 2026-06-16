using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Unity_TobiiGameIntegration_InputSystemAdapter.Demo
{
    /// <summary>
    /// Controls PlayerInput control scheme switching and auto-switching policy.
    /// </summary>
    [AddComponentMenu("Ken Rampage/Unity/Input/Player Input Scheme Controller")]
    public class PlayerInputSchemeController : MonoBehaviour
    {
        #region Fields
        [Header("References")]
        [SerializeField] private PlayerInput _playerInput;

        [Header("Behavior")]
        [SerializeField] private bool _applySchemeOnStart = true;
        [SerializeField] private string _initialScheme;

        [Header("Events")]
        [SerializeField] private UnityEvent<string> _onSchemeChanged = new UnityEvent<string>();

        [Header("Debug")]
        [SerializeField] private bool _logDebugMessages = true;

        [SerializeField] private string _currentScheme;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (!ValidateConfiguration())
            {
                Debug.LogWarning("[PlayerInputSchemeController] PlayerInput reference is missing. Disabling component.", this);
                enabled = false;
                return;
            }

            UpdateCurrentScheme(raiseEvent: false);
        }

        private void OnEnable()
        {
            if (_playerInput != null)
            {
                _playerInput.onControlsChanged += HandleControlsChanged;
            }
        }

        private void Start()
        {
            if (!_applySchemeOnStart || string.IsNullOrWhiteSpace(_initialScheme))
            {
                return;
            }

            SwitchToScheme(_initialScheme);
        }

        private void OnDisable()
        {
            if (_playerInput != null)
            {
                _playerInput.onControlsChanged -= HandleControlsChanged;
            }
        }

        #endregion

        #region Public API
        public void SwitchToScheme(string schemeName)
        {
            if (string.IsNullOrWhiteSpace(schemeName))
            {
                Log("SwitchToScheme called with empty scheme name.");
                return;
            }

            if (!ValidateConfiguration())
            {
                return;
            }

            if (!HasScheme(schemeName))
            {
                Log($"Scheme '{schemeName}' was not found in PlayerInput actions asset.");
                return;
            }

            if (string.Equals(_playerInput.currentControlScheme, schemeName))
            {
                Log($"Scheme '{schemeName}' is already active.");
                return;
            }

            _playerInput.neverAutoSwitchControlSchemes = true;

            if (_playerInput.user.valid)
            {
                _playerInput.user.ActivateControlScheme(schemeName).AndPairRemainingDevices();
            }
            else
            {
                _playerInput.SwitchCurrentControlScheme(schemeName);
            }

            UpdateCurrentScheme(raiseEvent: true);
            Log($"Switched to scheme '{schemeName}' and disabled auto-switching.");
        }

        public void SwitchToSchemeByIndex(int index)
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            if (_playerInput.actions == null || _playerInput.actions.controlSchemes.Count == 0)
            {
                Log("No control schemes are available.");
                return;
            }

            if (index < 0 || index >= _playerInput.actions.controlSchemes.Count)
            {
                Log($"Scheme index {index} is out of range (0..{_playerInput.actions.controlSchemes.Count - 1}).");
                return;
            }

            SwitchToScheme(_playerInput.actions.controlSchemes[index].name);
        }

        [ContextMenu("Control Schemes/Switch To First Scheme")]
        public void SwitchToFirstScheme()
        {
            SwitchToSchemeByIndex(0);
        }

        [ContextMenu("Control Schemes/Next Scheme")]
        public void NextScheme()
        {
            SwitchByOffset(1);
        }

        [ContextMenu("Control Schemes/Previous Scheme")]
        public void PreviousScheme()
        {
            SwitchByOffset(-1);
        }

        [ContextMenu("Control Schemes/Switch To Default Scheme")]
        public void SwitchToDefaultScheme()
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            string defaultScheme = _playerInput.defaultControlScheme;
            if (string.IsNullOrWhiteSpace(defaultScheme))
            {
                if (_playerInput.actions == null || _playerInput.actions.controlSchemes.Count == 0)
                {
                    Log("No default control scheme is configured and no schemes are available.");
                    return;
                }

                defaultScheme = _playerInput.actions.controlSchemes[0].name;
                Log($"PlayerInput default control scheme is empty. Falling back to first scheme '{defaultScheme}'.");
            }

            SwitchToScheme(defaultScheme);
        }

        [ContextMenu("Control Schemes/Enable Auto Switching")]
        public void EnableAutoSwitching()
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            if (!_playerInput.neverAutoSwitchControlSchemes)
            {
                return;
            }

            _playerInput.neverAutoSwitchControlSchemes = false;
            Log("Auto-switching enabled.");
        }

        [ContextMenu("Control Schemes/Disable Auto Switching")]
        public void DisableAutoSwitching()
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            if (_playerInput.neverAutoSwitchControlSchemes)
            {
                return;
            }

            _playerInput.neverAutoSwitchControlSchemes = true;
            Log("Auto-switching disabled.");
        }
        #endregion

        #region Internal
        private bool ValidateConfiguration()
        {
            if (_playerInput != null)
            {
                return true;
            }

            Debug.LogWarning("[PlayerInputSchemeController] PlayerInput reference is required. Assign it in the inspector.", this);
            return false;
        }

        private bool HasScheme(string schemeName)
        {
            if (_playerInput == null || _playerInput.actions == null)
            {
                return false;
            }

            for (int i = 0; i < _playerInput.actions.controlSchemes.Count; i++)
            {
                if (_playerInput.actions.controlSchemes[i].name == schemeName)
                {
                    return true;
                }
            }

            return false;
        }

        private void SwitchByOffset(int offset)
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            if (_playerInput.actions == null || _playerInput.actions.controlSchemes.Count == 0)
            {
                Log("No control schemes are available to cycle.");
                return;
            }

            int schemeCount = _playerInput.actions.controlSchemes.Count;
            int currentIndex = -1;

            for (int i = 0; i < schemeCount; i++)
            {
                if (_playerInput.actions.controlSchemes[i].name == _playerInput.currentControlScheme)
                {
                    currentIndex = i;
                    break;
                }
            }

            int targetIndex;
            if (currentIndex < 0)
            {
                targetIndex = offset >= 0 ? 0 : schemeCount - 1;
            }
            else
            {
                targetIndex = (currentIndex + offset) % schemeCount;
                if (targetIndex < 0)
                {
                    targetIndex += schemeCount;
                }
            }

            SwitchToScheme(_playerInput.actions.controlSchemes[targetIndex].name);
        }

        private void HandleControlsChanged(PlayerInput input)
        {
            UpdateCurrentScheme(raiseEvent: true);
        }

        private void UpdateCurrentScheme(bool raiseEvent)
        {
            string previous = _currentScheme;
            _currentScheme = _playerInput != null ? _playerInput.currentControlScheme : string.Empty;

            if (!raiseEvent)
            {
                return;
            }

            if (!string.Equals(previous, _currentScheme))
            {
                _onSchemeChanged.Invoke(_currentScheme);
                Log($"Scheme changed: '{previous}' -> '{_currentScheme}'");
            }
        }

        private void Log(string message)
        {
            if (!_logDebugMessages)
            {
                return;
            }

            Debug.Log($"[PlayerInputSchemeController] {message}", this);
        }
        #endregion
    }
}
