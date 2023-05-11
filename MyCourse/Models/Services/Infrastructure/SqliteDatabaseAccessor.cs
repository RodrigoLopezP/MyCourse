using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Options;
using MyCourse.Models.ValueObjects;

namespace MyCourse.Models.Services.Infrastructure
{
     public class SqliteDatabaseAccessor : IDatabaseAccessor
     {
          private readonly ILogger<SqliteDatabaseAccessor> logger;
          private readonly IOptionsMonitor<ConnectionStringsOptions> connectionStrOpts;
          public SqliteDatabaseAccessor(ILogger<SqliteDatabaseAccessor> logger ,IOptionsMonitor<ConnectionStringsOptions> connectionStringsOptions)
          {
               this.logger = logger;
               this.connectionStrOpts=connectionStringsOptions;
          }
          public async IAsyncEnumerable<IDataRecord> QueryAsync(FormattableString fQuery)
          {
               /*Lez 12 - sez 76 - servizio di loggingh
               *Aggiungo un log dentro questo metodo, dato che ho creato il _log di tipo ILogging nel ctor sopra
               */
               logger.LogInformation(fQuery.Format, fQuery.GetArguments());
               #region formattazione query 
               /*Grazie al ciclo sotto è possibile passare la query in stringa con un formato più comprensibile
               in parole semplice cambia i parametri scritti in parentesi grafe con un chiocciola davanti
               il quale è il formato che accetta SQL
               Si creerà una lista,, che nella linea 36 circa verrà passata alla var sqlitecommand cmd
               così la connessione avrà query e argomenti per poter eseguirsi correttamente */
               var queryArguments= fQuery.GetArguments();
               var sqliteParameters= new List<SqliteParameter>();
               for (int i = 0; i < queryArguments.Length; i++)
               {
                    if(queryArguments[i] is Sql){
                         continue; //se è di tipo Sql, cioè la classe che abbiamo creato noi, non lo fa diventare un parametro. Va alla prossima
                    }
                    var parameter = new SqliteParameter(i.ToString(),queryArguments[i]);
                    sqliteParameters.Add(parameter);
                    queryArguments[i]="@"+i;
               }
               string query= fQuery.ToString();
               #endregion

               
               /*invece di usare sqlconnection.open e .close, scrivere USING
               Così in caso di errore di una query, il programm automaticamente chiuderà la connessione*/
               /*Lez-12-72
               cambiare il parametetro del SQLITECONNECTION, usando la config dal file json
               *Se si cambia il valore nel file json, verranno aggiornati anche qui, grazie al IOptionsMonitor
               */
               string connectionString= connectionStrOpts.CurrentValue.Default;
               using (var conn= new SqliteConnection(connectionString))
               {
                    await conn.OpenAsync();
                    using(var cmd= new SqliteCommand(query,conn))
                    {
                         cmd.Parameters.AddRange(sqliteParameters);
                         using(var reader = await cmd.ExecuteReaderAsync())
                         {
                              while (await reader.ReadAsync())
                              {
                                   yield return reader;
                              }
                         }
                    }
               }
          }
     }
}