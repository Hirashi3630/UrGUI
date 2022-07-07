using System;
using UnityEngine;

namespace UrGUI.Utils
{
    /// <summary>
    /// Debugging class to log warning, errors or info messages used by UrGUI 
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Invoked on any UrGUI debug message (filtered messages don't invoke)
        /// </summary>
        public static readonly Action<object, PrintType> OnLoggerPrint = delegate {  };

        /// <summary>
        /// used to filter debug messages coming from UrGUI 
        /// </summary>
        public static PrintType DebugFilter
        {
            get => _debugFilter;
            set => _debugFilter = value;
        }

#if DEBUG
        private static PrintType _debugFilter = PrintType.Error | PrintType.Warnings | PrintType.Info; // debug
#else
        private static PrintType _debugFilter = PrintType.Error | PrintType.Warnings | PrintType.Info; // release
#endif

        
        internal static void log(object msg)
        {
            if (!_debugFilter.HasFlag(PrintType.Info)) return;
            Debug.Log(msg);
            if (OnLoggerPrint != null) OnLoggerPrint(msg, PrintType.Info);
        }

        internal static void war(object msg)
        {
            if (!_debugFilter.HasFlag(PrintType.Warnings)) return;
            Debug.LogWarning(msg);
            if (OnLoggerPrint != null) OnLoggerPrint(msg, PrintType.Warnings);
        }

        internal static void err(object msg)
        {
            if (!_debugFilter.HasFlag(PrintType.Error)) return;
            Debug.LogError(msg);
            if (OnLoggerPrint != null) OnLoggerPrint(msg, PrintType.Error);
        }
        
        /// <summary>
        /// Types of debugging messages
        /// *[Flags]
        /// </summary>
        [Flags]
        public enum PrintType
        {
            /// <summary> no debug messages </summary>
            None = 0,
            /// <summary> Critical Error messages mostly exceptions </summary>
            Error = 1,
            /// <summary> Warning messages </summary>
            Warnings = 2,
            /// <summary> Informative messages also called simply log/print </summary>
            Info = 4
        }
    }
}