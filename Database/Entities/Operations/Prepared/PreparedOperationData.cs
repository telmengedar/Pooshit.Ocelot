using System;
using System.Linq;
using System.Text;
using NightlyCode.Database.Clients;

namespace NightlyCode.Database.Entities.Operations.Prepared {
    public class PreparedOperationData {

        public string Command { get; set; }

        public object[] Parameters { get; set; }

        public static PreparedOperationData Create(IDBClient dbclient, string commandtext, object[] parameters) {
            object[] localparameters = parameters.Where(p => !(p is Array)).ToArray();
            Array[] arrayparameters = parameters.Where(p => p is Array).Cast<Array>().ToArray();

            int arrayindex = 0;
            for (int i = 0; i < arrayparameters.Length; ++i)
            {
                StringBuilder parameterbuilder = new StringBuilder();
                for (int k = 0; k < arrayparameters[i].Length; ++k)
                {
                    if (parameterbuilder.Length > 0)
                        parameterbuilder.Append(",");
                    parameterbuilder.Append($"{dbclient.DBInfo.Parameter}{localparameters.Length + arrayindex++ + 1}");
                }

                commandtext = commandtext.Replace($"[{i}]", $"({parameterbuilder})");
            }

            return new PreparedOperationData {
                Command = commandtext,
                Parameters = localparameters.Concat(arrayparameters.SelectMany(a => a.Cast<object>())).ToArray()
            };
        }
    }
}