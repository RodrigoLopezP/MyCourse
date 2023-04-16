using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace MyCourse.Models.Services.Infrastructure
{
     public class SqliteDatabaseAccessor : IDatabaseAccessor
     {
          public async Task<DataSet> QueryAsync(FormattableString fQuery)
          {
               #region formattazione query 
               /*Grazie al ciclo sotto è possibile passare la query in stringa con un formato più comprensibile
               in parole semplice cambia i parametri scritti in parentesi grafe con un chiocciola davanti
               il quale è il formato che accetta SQL
               Si creerà una lista, che nella linea 36 circa verrà passata alla var sqlitecommand cmd
               così la connessione avrà query e argomenti per poter eseguirsi correttamente */
               var queryArguments= fQuery.GetArguments();
               var sqliteParameters= new List<SqliteParameter>();
               for (int i = 0; i < queryArguments.Length; i++)
               {
                    var parameter = new SqliteParameter(i.ToString(),queryArguments[i]);
                    sqliteParameters.Add(parameter);
                    queryArguments[i]="@"+i;
               }
               string query= fQuery.ToString();
               #endregion

               /*invece di usare sqlconnection.open e .close, scrivere USING
               Così in caso di errore di una query, il programm automaticamente chiuderà la connessione
               */
               using (var conn= new SqliteConnection("Data Source=Data/MyCourse.db"))
               {
                    await conn.OpenAsync();
                    using(var cmd= new SqliteCommand(query,conn)) //la query viene manda al database
                    {
                         cmd.Parameters.AddRange(sqliteParameters);
                         using(var reader = await cmd.ExecuteReaderAsync())
                         {
                              var dataSet=new DataSet();
                              //inizio workaround
                              dataSet.EnforceConstraints=false;
                              //fine workaround

                              do
                              {
                              var dataTable= new DataTable();
                              dataSet.Tables.Add(dataTable);
                              dataTable.Load(reader);
                              } while (!reader.IsClosed);

                              return dataSet;
                         }
                    }
               }
          }
     }
}