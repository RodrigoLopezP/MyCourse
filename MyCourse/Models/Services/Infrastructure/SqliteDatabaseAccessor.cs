using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.Options;
using MyCourse.Models.ValueObjects;

namespace MyCourse.Models.Services.Infrastructure
{
     public class SqliteDatabaseAccessor : IDatabaseAccessor
     {
          private readonly ILogger<SqliteDatabaseAccessor> logger;
          readonly IOptionsMonitor<ConnectionStringsOptions> connectionStrOpts;
          public SqliteDatabaseAccessor(ILogger<SqliteDatabaseAccessor> logger, IOptionsMonitor<ConnectionStringsOptions> connectionStringsOptions)
          {
               this.logger = logger;
               this.connectionStrOpts = connectionStringsOptions;
          }

          public async Task<int> CommandAsync(FormattableString formattableCommand, CancellationToken token)
          {
               try
               {
                    //logger.LogInformation(formattableCommand.Format, formattableCommand.GetArguments());

                    //Colleghiamoci al database Sqlite, inviamo la query e leggiamo i risultati
                    using SqliteConnection conn = await GetOpenedConnection(token);
                    using SqliteCommand cmd = GetCommand(formattableCommand, conn);

                    int affectedRows = await cmd.ExecuteNonQueryAsync(token);
                    return affectedRows;
               }
               catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
               {
                    throw new ConstraintViolationException(exc);
               }
          }

          public async Task<DataSet> QueryAsync(FormattableString formattableQuery, CancellationToken token)
          {
               try
               {
                    logger.LogInformation(formattableQuery.Format, formattableQuery.GetArguments());
                    //Colleghiamoci al database Sqlite, inviamo la query e leggiamo i risultati
                    using SqliteConnection conn = await GetOpenedConnection(token);
                    using SqliteCommand cmd = GetCommand(formattableQuery, conn);
                    //Inviamo la query al database e otteniamo un SqliteDataReader
                    //per leggere i risultati
                    using var reader = await cmd.ExecuteReaderAsync(token);
                    var dataSet = new DataSet();

                    dataSet.EnforceConstraints = false;
                    //Creiamo tanti DataTable per quante sono le tabelle
                    //di risultati trovate dal SqliteDataReader
                    do
                    {
                         var dataTable = new DataTable();
                         dataSet.Tables.Add(dataTable);
                         dataTable.Load(reader);
                    } while (!reader.IsClosed);

                    return dataSet;
               }
               catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
               {
                    throw new ConstraintViolationException(exc);
               }
          }
          public async Task<T> QueryScalarAsync<T>(FormattableString formattableQuery, CancellationToken token)
          {
               try
               {
                    logger.LogInformation(formattableQuery.Format, formattableQuery.GetArguments());
                    //Colleghiamoci al database Sqlite, inviamo la query e leggiamo i risultati
                    using SqliteConnection conn = await GetOpenedConnection(token);
                    using SqliteCommand cmd = GetCommand(formattableQuery, conn);

                    object result = await cmd.ExecuteScalarAsync();
                    //object - il tipo alla base di tutti gli altri tipi
                    //poi dobbiamo essere noi a fare il casting necessario agli altri tipi, il metodo ExecuteScalarAsync restituisce quello
                    return (T)Convert.ChangeType(result, typeof(T));
               }
               catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
               {
                    throw new ConstraintViolationException(exc);
               }
          }

          private static SqliteCommand GetCommand(FormattableString formattableQuery, SqliteConnection conn)
          {
               //Creiamo dei SqliteParameter a partire dalla FormattableString
               var queryArguments = formattableQuery.GetArguments();
               var sqliteParameters = new List<SqliteParameter>();
               for (var i = 0; i < queryArguments.Length; i++)
               {
                    if (queryArguments[i] is Sql)
                    {
                         continue;
                    }
                    if (Convert.ToString(queryArguments[i]) == "(null)" || queryArguments[i]==null)
                    {
                         queryArguments[i] = DBNull.Value;
                    }
                    var parameter = new SqliteParameter(name: i.ToString(), value: queryArguments[i]); //value, se è null allora usa DB NULL VALUE
                    sqliteParameters.Add(parameter);
                    queryArguments[i] = "@" + i;
               }
               string query = formattableQuery.ToString();
               var cmd = new SqliteCommand(query, conn);

               //Aggiungiamo i SqliteParameters al SqliteCommand
               cmd.Parameters.AddRange(sqliteParameters);
               return cmd;
          }

          private async Task<SqliteConnection> GetOpenedConnection(CancellationToken token)
          {
               var conn = new SqliteConnection(connectionStrOpts.CurrentValue.Default);
               await conn.OpenAsync(token);
               return conn;
          }

     }
}