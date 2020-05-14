using System;

namespace NightlyCode.Database.Extern {

    /// <summary>
    /// provides logging functions to the current assembly
    /// </summary>
    public class Logger {

        /// <summary>
        /// triggered when an info message is generated
        /// </summary>
        public static event Action<object, string, string> InfoMessage;

        /// <summary>
        /// triggered when a warning message is generated
        /// </summary>
        public static event Action<object, string, string> WarningMessage;

        /// <summary>
        /// triggered when an error message is generated
        /// </summary>
        public static event Action<object, string, Exception> ErrorMessage;

        /// <summary>
        /// logs an info
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="message">message content</param>
        /// <param name="details">message details</param>
        public static void Info(object sender, string message, string details = null) {
            InfoMessage?.Invoke(sender, message, details);
        }

        /// <summary>
        /// logs a warning
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="message">message content</param>
        /// <param name="details">message details</param>
        public static void Warning(object sender, string message, string details = null) {
            WarningMessage?.Invoke(sender, message, details);
        }

        /// <summary>
        /// logs an error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="message">message content</param>
        /// <param name="details">message details</param>
        public static void Error(object sender, string message, Exception details = null) {
            ErrorMessage?.Invoke(sender, message, details);
        }
    }
}