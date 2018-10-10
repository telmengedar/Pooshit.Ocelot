using System;

namespace Database.Extern {

    /// <summary>
    /// provides logging functions to the current assembly
    /// </summary>
    internal class Logger {

        static readonly Action<object, string, string> info = (s, m, d) => { };

        static readonly Action<object, string, string> warning = (s, m, d) => { };

        static readonly Action<object, string, Exception> error = (s, m, d) => { };

        static Logger() {

#if NIGHTLYCODE
            // this looks whether the main assembly has referenced "NightlyCode.Core" and links the logs methods automatically
            if(Assembly.GetEntryAssembly()?.GetReferencedAssemblies()?.Any(a => a.Name == "NightlyCode.Core")??false) {
                ILoggerProvider nightlycodeprovider = (ILoggerProvider)Activator.CreateInstance(Type.GetType(typeof(Logger).Namespace + ".NightlyCodeLoggerProvider"));
                info = nightlycodeprovider.Info;
                warning = nightlycodeprovider.Warning;
                error = nightlycodeprovider.Error;
            }
#endif
        }

        /// <summary>
        /// logs an info
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="message">message content</param>
        /// <param name="details">message details</param>
        public static void Info(object sender, string message, string details = null) {
            info(sender, message, details);
        }

        /// <summary>
        /// logs a warning
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="message">message content</param>
        /// <param name="details">message details</param>
        public static void Warning(object sender, string message, string details = null) {
            warning(sender, message, details);
        }

        /// <summary>
        /// logs an error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="message">message content</param>
        /// <param name="details">message details</param>
        public static void Error(object sender, string message, Exception details = null) {
            error(sender, message, details);
        }
    }

    /// <summary>
    /// interface for a provider of logging functions
    /// </summary>
    internal interface ILoggerProvider {

        /// <summary>
        /// method to use to log an info
        /// </summary>
        Action<object, string, string> Info { get; }

        /// <summary>
        /// method to use to log a warning
        /// </summary>
        Action<object, string, string> Warning { get; }

        /// <summary>
        /// method to use to log an error
        /// </summary>
        Action<object, string, Exception> Error { get; }
    }

#if NIGHTLYCODE
    /// <summary>
    /// provides the logger of the core assembly
    /// </summary>
    internal class NightlyCodeLoggerProvider : ILoggerProvider {

        /// <summary>
        /// method to use to log an info
        /// </summary>
        public Action<object, string, string> Info => Core.Logs.Logger.Info;

        /// <summary>
        /// method to use to log a warning
        /// </summary>
        public Action<object, string, string> Warning => Core.Logs.Logger.Warning;

        /// <summary>
        /// method to use to log an error
        /// </summary>
        public Action<object, string, Exception> Error => Core.Logs.Logger.Error;
    }
#endif
}