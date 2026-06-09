using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Adroit.Tobii.TGI
{
    /// <summary>
    /// Registers Input System layouts for TGI virtual devices, including editor-time registration.
    /// </summary>
    public static class TGIVirtualInputLayoutRegistration
    {
        #region Registration
        public static void RegisterLayouts()
        {
            RegisterLayout<TGIVirtualInput.TGIGazeDevice>("TGIGaze", "TGIGaze");
            RegisterLayout<TGIVirtualInput.TGIHeadDevice>("TGIHead", "TGIHead");
            RegisterLayout<TGIVirtualInput.TGIHeadPositionDevice>("TGIHeadPosition", "TGIHeadPosition");
        }

        private static void RegisterLayout<TDevice>(string layoutName, string interfaceName)
            where TDevice : InputDevice
        {
            try
            {
                InputSystem.RegisterLayout<TDevice>(
                    layoutName,
                    matches: new InputDeviceMatcher().WithInterface(interfaceName));
            }
            catch (Exception)
            {
                // Ignore duplicate registration or unsupported layout registration errors
            }
        }
        #endregion
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    internal static class TGIVirtualInputLayoutRegistrationEditor
    {
        #region Editor Hooks
        static TGIVirtualInputLayoutRegistrationEditor()
        {
            TGIVirtualInputLayoutRegistration.RegisterLayouts();
        }
        #endregion
    }
#endif
}
