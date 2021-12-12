using System;
using System.Linq;
using System.Text;
using NightlyCode.Database.Clients;

namespace NightlyCode.Database.Entities.Operations.Prepared {

    /// <summary>
    /// operation data prepared for execution
    /// </summary>
    public class PreparedOperationData {

        /// <summary>
        /// command text
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// final parameters of operation
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// prepares an operation for execution
        /// </summary>
        /// <param name="dbclient">db client used to execute operation</param>
        /// <param name="commandtext">command text</param>
        /// <param name="constantparameters">constant parameters</param>
        /// <param name="constantarrayparameters">constant array parameters</param>
        /// <param name="parameters">user parameters</param>
        /// <param name="arrayparameters">user array parameters</param>
        /// <returns>prepared operation data which can get executed</returns>
        public static PreparedOperationData Create(IDBClient dbclient, string commandtext, object[] constantparameters, Array[] constantarrayparameters, object[] parameters, Array[] arrayparameters) {
            int i = 0;
            int arrayindex = 0;
            foreach(Array parameter in constantarrayparameters.Concat(arrayparameters))
            { 
                StringBuilder parameterbuilder = new StringBuilder();
                for (int k = 0; k < parameter.Length; ++k)
                {
                    if (parameterbuilder.Length > 0)
                        parameterbuilder.Append(",");
                    parameterbuilder.Append($"{dbclient.DBInfo.Parameter}{constantparameters.Length + parameters.Length + arrayindex++ + 1}");
                }

                commandtext = commandtext.Replace($"[{i++}]", $"{parameterbuilder}");
            }

            return new PreparedOperationData {
                Command = commandtext,
                Parameters = constantparameters.Concat(parameters)
                    .Concat(constantarrayparameters.SelectMany(a => a.Cast<object>()))
                    .Concat(arrayparameters.SelectMany(a => a.Cast<object>())).ToArray()
            };
        }
    }
}