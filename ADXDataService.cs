
using Azure.Identity;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace I3X4Kusto
{
    public class ADXDataService
    {
        private ICslQueryProvider _queryProvider = null;

        public void Connect()
        {
            // connect to ADX cluster
            string adxClusterName = Environment.GetEnvironmentVariable("ADX_HOST");
            string adxDBName = Environment.GetEnvironmentVariable("ADX_DB");
            string aadAppID = Environment.GetEnvironmentVariable("ADX_APPLICATION_ID");

            if (!string.IsNullOrEmpty(adxClusterName) && !string.IsNullOrEmpty(adxDBName))
            {
                KustoConnectionStringBuilder connectionString;
                if (string.IsNullOrEmpty(aadAppID))
                {
                    connectionString = new KustoConnectionStringBuilder(adxClusterName, adxDBName)
                        .WithAadAzureTokenCredentialsAuthentication(new DefaultAzureCredential());
                }
                else
                {
                    connectionString = new KustoConnectionStringBuilder(adxClusterName, adxDBName)
                        .WithAadUserManagedIdentity(aadAppID);
                }

                _queryProvider = KustoClientFactory.CreateCslQueryProvider(connectionString);
            }
        }

        public void Dispose()
        {
            if (_queryProvider != null)
            {
                _queryProvider.Dispose();
                _queryProvider = null;
            }
        }

        public Dictionary<string, object> ADXQueryForSpecificValue(string stationName, string productionLineName, string valueToQuery, double desiredValue)
        {
            string query = "opcua_metadata_lkv\r\n"
                         + "| where Name contains \"" + stationName + "\"\r\n"
                         + "| where Name contains \"" + productionLineName + "\"\r\n"
                         + "| join kind = inner(opcua_telemetry\r\n"
                         + "    | where Name == \"" + valueToQuery + "\"\r\n"
                         + "    | where Timestamp > now(- 1h)\r\n"
                         + ") on DataSetWriterID\r\n"
                         + "| distinct Timestamp, OPCUANodeValue = todouble(Value)\r\n"
                         + "| sort by Timestamp desc";

            return RunQuery(query);
        }

        public Dictionary<string, object> ADXQueryForSpecificTime(string stationName, string productionLineName, string valueToQuery, string timeToQuery, int idealCycleTime)
        {
            string query = "opcua_metadata_lkv\r\n"
                         + "| where Name contains \"" + stationName + "\"\r\n"
                         + "| where Name contains \"" + productionLineName + "\"\r\n"
                         + "| join kind = inner(opcua_telemetry\r\n"
                         + "    | where Name == \"" + valueToQuery + "\"\r\n"
                         + "    | where Timestamp > now(- 1h)\r\n"
                         + ") on DataSetWriterID\r\n"
                         + "| distinct Timestamp, OPCUANodeValue = todouble(Value)\r\n"
                         + "| where around(Timestamp, datetime(" + timeToQuery + "), " + idealCycleTime.ToString() + "s)\r\n"
                         + "| sort by Timestamp desc";

            return RunQuery(query);
        }

        public Dictionary<string, object> RunQuery(string query)
        {
            bool allowMultiRow = false;
            Dictionary<string, object> values = new();

            ClientRequestProperties clientRequestProperties = new ClientRequestProperties()
            {
                ClientRequestId = Guid.NewGuid().ToString()
            };

            try
            {
                if (_queryProvider != null)
                {
                    using (IDataReader reader = _queryProvider.ExecuteQuery(query, clientRequestProperties))
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                try
                                {
                                    if (reader.GetValue(i) != null)
                                    {
                                        if (!allowMultiRow)
                                        {
                                            if (values.ContainsKey(reader.GetName(i)))
                                            {
                                                values[reader.GetName(i)] = reader.GetValue(i);
                                            }
                                            else
                                            {
                                                values.TryAdd(reader.GetName(i), reader.GetValue(i));
                                            }
                                        }
                                        else
                                        {
                                            if (values.ContainsKey(reader.GetValue(i).ToString()))
                                            {
                                                values[reader.GetValue(i).ToString()] = reader.GetValue(i);
                                            }
                                            else
                                            {
                                                values.TryAdd(reader.GetValue(i).ToString(), reader.GetValue(i));
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);

                                    // ignore this field and move on
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RunADXQuery: " + ex.Message);
            }

            return values;
        }
    }
}
