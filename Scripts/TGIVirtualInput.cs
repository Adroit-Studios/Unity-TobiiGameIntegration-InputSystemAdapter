using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Adroit.Tobii.TGI
{
    /// <summary>
    /// Creates virtual input devices for TGI gaze and head tracking.
    /// Allows TGI data to be used in Input Action maps and bindings.
    /// </summary>
    [AddComponentMenu("Adroit/Tobii/TGI/TGI Virtual Input")]
    public class TGIVirtualInput : MonoBehaviour
    {
        #region Singleton

        [Header("Singleton")]
        [Tooltip("If enabled, this singleton persists across scene loads")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        public static TGIVirtualInput Instance { get; private set; }

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

        #region Device Definitions

        public struct TGIGazeState : IInputStateTypeInfo
        {
            public FourCC format => new FourCC('T', 'G', 'I', 'G');

            [InputControl(layout = "Vector2")]
            public Vector2 position;

            [InputControl(layout = "Vector2")]
            public Vector2 delta;
        }

        public struct TGIHeadState : IInputStateTypeInfo
        {
            public FourCC format => new FourCC('T', 'G', 'I', 'H');

            [InputControl(layout = "Vector2")]
            public Vector2 position;

            [InputControl(layout = "Vector2")]
            public Vector2 delta;

            [InputControl(layout = "Vector3")]
            public Vector3 rotation;
        }

        public struct TGIHeadPositionState : IInputStateTypeInfo
        {
            public FourCC format => new FourCC('T', 'G', 'I', 'P');

            [InputControl(layout = "Vector3")]
            public Vector3 position;

            [InputControl(layout = "Vector3")]
            public Vector3 delta;

            [InputControl(layout = "Axis")]
            public float distanceZ;
        }

        [InputControlLayout(displayName = "TGI Gaze", stateType = typeof(TGIGazeState))]
        public class TGIGazeDevice : InputDevice
        {
            public Vector2Control position { get; private set; }
            public Vector2Control delta { get; private set; }

            protected override void FinishSetup()
            {
                base.FinishSetup();
                position = GetChildControl<Vector2Control>("position");
                delta = GetChildControl<Vector2Control>("delta");
            }
        }

        [InputControlLayout(displayName = "TGI Head", stateType = typeof(TGIHeadState))]
        public class TGIHeadDevice : InputDevice
        {
            public Vector2Control position { get; private set; }
            public Vector2Control delta { get; private set; }
            public Vector3Control rotation { get; private set; }

            protected override void FinishSetup()
            {
                base.FinishSetup();
                position = GetChildControl<Vector2Control>("position");
                delta = GetChildControl<Vector2Control>("delta");
                rotation = GetChildControl<Vector3Control>("rotation");
            }
        }

        [InputControlLayout(displayName = "TGI Head Position", stateType = typeof(TGIHeadPositionState))]
        public class TGIHeadPositionDevice : InputDevice
        {
            public Vector3Control position { get; private set; }
            public Vector3Control delta { get; private set; }
            public AxisControl distanceZ { get; private set; }

            protected override void FinishSetup()
            {
                base.FinishSetup();
                position = GetChildControl<Vector3Control>("position");
                delta = GetChildControl<Vector3Control>("delta");
                distanceZ = GetChildControl<AxisControl>("distanceZ");
            }
        }

        #endregion

        #region Fields

        [Header("Virtual Device Toggles")]
        [SerializeField] private bool enableGazeDevice = true;
        [SerializeField] private bool enableHeadDevice = true;
        [SerializeField] private bool enableHeadPositionDevice = true;

        private TGIGazeDevice _gazeDevice;
        private TGIHeadDevice _headDevice;
        private TGIHeadPositionDevice _headPositionDevice;
        
        private Vector2 _lastGazePosition;
        private Vector2 _lastHeadPosition;
        private Vector3 _lastHeadPositionMeters;

        #endregion

        #region Unity Lifecycle

        void OnEnable()
        {
            RegisterDeviceLayouts();
            CreateDevices();
            ApplyDeviceEnableStates();
        }

        void OnValidate()
        {
            if (!Application.isPlaying) return;
            ApplyDeviceEnableStates();
        }

        void OnDisable()
        {
            RemoveDevices();
        }

        void Update()
        {
            if (TGIHardwareManager.Instance == null) return;

            EnsureDevicesAvailable();

            if (IsDeviceEnabled(_gazeDevice)) UpdateGazeDevice();
            if (IsDeviceEnabled(_headDevice)) UpdateHeadDevice();
            if (IsDeviceEnabled(_headPositionDevice)) UpdateHeadPositionDevice();
        }

        #endregion

        #region Device Management

        private void RegisterDeviceLayouts()
        {
            TGIVirtualInputLayoutRegistration.RegisterLayouts();
        }


        private void CreateDevices()
        {
            _gazeDevice = InputSystem.AddDevice<TGIGazeDevice>("TGIGaze");
            _headDevice = InputSystem.AddDevice<TGIHeadDevice>("TGIHead");
            _headPositionDevice = InputSystem.AddDevice<TGIHeadPositionDevice>("TGIHeadPosition");

            if (_gazeDevice == null || _headDevice == null || _headPositionDevice == null)
            {
                Debug.LogError("[TGIVirtualInput] Failed to create virtual input devices!");
            }
        }

        private void ApplyDeviceEnableStates()
        {
            SetDeviceEnabled(_gazeDevice, enableGazeDevice);
            SetDeviceEnabled(_headDevice, enableHeadDevice);
            SetDeviceEnabled(_headPositionDevice, enableHeadPositionDevice);
        }

        private static bool IsDeviceEnabled(InputDevice device)
        {
            return device != null && device.enabled;
        }

        private static bool IsDeviceInSystem(InputDevice device)
        {
            if (device == null) return false;

            var devices = InputSystem.devices;
            for (int i = 0; i < devices.Count; i++)
            {
                if (ReferenceEquals(devices[i], device)) return true;
            }

            return false;
        }

        private void EnsureDevicesAvailable()
        {
            bool needsRecreate = false;

            if (enableGazeDevice && !IsDeviceInSystem(_gazeDevice)) needsRecreate = true;
            if (enableHeadDevice && !IsDeviceInSystem(_headDevice)) needsRecreate = true;
            if (enableHeadPositionDevice && !IsDeviceInSystem(_headPositionDevice)) needsRecreate = true;

            if (!needsRecreate) return;

            RemoveDevices();
            CreateDevices();
            ApplyDeviceEnableStates();
        }

        private static void SetDeviceEnabled(InputDevice device, bool enabled)
        {
            if (device == null) return;

            if (enabled && !device.enabled)
            {
                InputSystem.EnableDevice(device);
            }
            else if (!enabled && device.enabled)
            {
                InputSystem.DisableDevice(device);
            }
        }

        private void RemoveDevices()
        {
            if (_gazeDevice != null)
            {
                InputSystem.RemoveDevice(_gazeDevice);
                _gazeDevice = null;
            }

            if (_headDevice != null)
            {
                InputSystem.RemoveDevice(_headDevice);
                _headDevice = null;
            }

            if (_headPositionDevice != null)
            {
                InputSystem.RemoveDevice(_headPositionDevice);
                _headPositionDevice = null;
            }
        }

        #endregion

        #region Public Controls

        [ContextMenu("TGI/Virtual Input/Enable Gaze Device")]
        public void EnableGazeDevice()
        {
            enableGazeDevice = true;
            SetDeviceEnabled(_gazeDevice, true);
        }

        [ContextMenu("TGI/Virtual Input/Disable Gaze Device")]
        public void DisableGazeDevice()
        {
            enableGazeDevice = false;
            SetDeviceEnabled(_gazeDevice, false);
        }

        [ContextMenu("TGI/Virtual Input/Enable Head Device")]
        public void EnableHeadDevice()
        {
            enableHeadDevice = true;
            SetDeviceEnabled(_headDevice, true);
        }

        [ContextMenu("TGI/Virtual Input/Disable Head Device")]
        public void DisableHeadDevice()
        {
            enableHeadDevice = false;
            SetDeviceEnabled(_headDevice, false);
        }

        [ContextMenu("TGI/Virtual Input/Enable Head Position Device")]
        public void EnableHeadPositionDevice()
        {
            enableHeadPositionDevice = true;
            SetDeviceEnabled(_headPositionDevice, true);
        }

        [ContextMenu("TGI/Virtual Input/Disable Head Position Device")]
        public void DisableHeadPositionDevice()
        {
            enableHeadPositionDevice = false;
            SetDeviceEnabled(_headPositionDevice, false);
        }

        public void SetAllDevicesEnabled(bool enabled)
        {
            enableGazeDevice = enabled;
            enableHeadDevice = enabled;
            enableHeadPositionDevice = enabled;
            ApplyDeviceEnableStates();
        }

        [ContextMenu("TGI/Virtual Input/Enable All Devices")]
        public void EnableAllDevices()
        {
            SetAllDevicesEnabled(true);
        }

        [ContextMenu("TGI/Virtual Input/Disable All Devices")]
        public void DisableAllDevices()
        {
            SetAllDevicesEnabled(false);
        }

        #endregion

        #region Device Updates

        private void UpdateGazeDevice()
        {
            if (!IsDeviceInSystem(_gazeDevice)) return;

            Vector2 currentPosition = TGIHardwareManager.Instance.GazeScreenPosition;
            currentPosition.y = Screen.height - currentPosition.y;
            Vector2 delta = currentPosition - _lastGazePosition;

            using (StateEvent.From(_gazeDevice, out InputEventPtr eventPtr))
            {
                _gazeDevice.position.WriteValueIntoEvent(currentPosition, eventPtr);
                _gazeDevice.delta.WriteValueIntoEvent(delta, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }

            _lastGazePosition = currentPosition;
        }

        private void UpdateHeadDevice()
        {
            if (!IsDeviceInSystem(_headDevice)) return;

            Vector2 currentPosition = TGIHardwareManager.Instance.HeadAngleScreenPosition;
            currentPosition.y = Screen.height - currentPosition.y;
            Vector2 delta = currentPosition - _lastHeadPosition;
            Vector3 rotation = TGIHardwareManager.Instance.HeadRotationDegrees;

            using (StateEvent.From(_headDevice, out InputEventPtr eventPtr))
            {
                _headDevice.position.WriteValueIntoEvent(currentPosition, eventPtr);
                _headDevice.delta.WriteValueIntoEvent(delta, eventPtr);
                _headDevice.rotation.WriteValueIntoEvent(rotation, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }

            _lastHeadPosition = currentPosition;
        }

        private void UpdateHeadPositionDevice()
        {
            if (!IsDeviceInSystem(_headPositionDevice)) return;

            Vector3 currentPosition = TGIHardwareManager.Instance.HeadPositionMeters;
            Vector3 delta = currentPosition - _lastHeadPositionMeters;

            using (StateEvent.From(_headPositionDevice, out InputEventPtr eventPtr))
            {
                _headPositionDevice.position.WriteValueIntoEvent(currentPosition, eventPtr);
                _headPositionDevice.delta.WriteValueIntoEvent(delta, eventPtr);
                _headPositionDevice.distanceZ.WriteValueIntoEvent(currentPosition.z, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }

            _lastHeadPositionMeters = currentPosition;
        }

        #endregion
    }
}
