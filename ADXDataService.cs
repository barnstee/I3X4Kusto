using Azure.Identity;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

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
                        .WithAadAzureTokenCredentialsAuthentication(new DefaultAzureCredential(new DefaultAzureCredentialOptions() { TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") }));
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

        /// <summary>
        /// Escapes and quotes an array of values for use in a KQL <c>in()</c> operator.
        /// </summary>
        public static string ToKqlStringList(string[] values)
        {
            return string.Join(", ", values.Select(v =>
                "\"" + v.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""));
        }

        /// <summary>
        /// Executes a KQL query and returns every result row as a column-name → value dictionary.
        /// </summary>
        public List<Dictionary<string, object>> RunQueryRows(string query)
        {
            var rows = new List<Dictionary<string, object>>();

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
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                try
                                {
                                    if (reader.GetValue(i) != null)
                                    {
                                        row[reader.GetName(i)] = reader.GetValue(i);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                }
                            }
                            rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RunADXQuery: " + ex.Message);
            }

            return rows;
        }
    }
}
