using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using moo.common.Models;

namespace moo.common.Database
{
    public class SqliteStorageProvider : IStorageProvider
    {
        public void Initialize()
        {
            using var connection = new SqliteConnection("Data Source=./objects.sqlite");
            connection.Open();

            using (var command = new SqliteCommand("CREATE TABLE IF NOT EXISTS changelog (version int)", connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqliteCommand("CREATE TABLE IF NOT EXISTS objects ([id] INTEGER PRIMARY KEY NOT NULL, [type] TEXT NOT NULL, [data] TEXT NOT NULL)", connection))
            {
                command.ExecuteNonQuery();
            }

            connection.Close();
        }

        public void Overwrite(Dictionary<int, string> serialized)
        {
            throw new System.NotImplementedException();
        }

        public async Task<StorageProviderRetrieveResult> LoadAsync(Dbref id, CancellationToken cancellationToken)
        {
            var result = default(StorageProviderRetrieveResult);

            using (var connection = new SqliteConnection("Data Source=./objects.sqlite"))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqliteCommand("SELECT [type], [data] FROM [objects] WHERE [id]=@id;", connection))
                {
                    command.Parameters.AddWithValue("@id", (int)id);
                    SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        result = new StorageProviderRetrieveResult(id, reader.GetString(0), reader.GetString(1));
                        reader.Close();
                    }
                }

                connection.Close();
            }

            return default(StorageProviderRetrieveResult).Equals(result) ? new StorageProviderRetrieveResult("Not found.") : result;
        }

        public async Task<StorageProviderRetrieveResult> LoadPlayerByNameAsync(string name, CancellationToken cancellationToken)
        {
            var result = default(StorageProviderRetrieveResult);

            using (var connection = new SqliteConnection("Data Source=./objects.sqlite"))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqliteCommand("SELECT [type], [data], [id] FROM [objects] WHERE [name]=@name;", connection))
                {
                    command.Parameters.AddWithValue("@name", (string)name);
                    SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        var dbref = Dbref.Parse(reader.GetString(2));
                        result = new StorageProviderRetrieveResult(dbref, reader.GetString(0), reader.GetString(1));
                        reader.Close();
                    }
                }

                connection.Close();
            }

            return default(StorageProviderRetrieveResult).Equals(result) ? new StorageProviderRetrieveResult("Not found.") : result;
        }

        public async Task<bool> SaveAsync(Dbref id, string type, string serialized, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = new SqliteConnection("Data Source=./objects.sqlite");
                await connection.OpenAsync(cancellationToken);

                int updated = 0;
                using (var command = new SqliteCommand("INSERT OR REPLACE INTO [objects] ([id], [type], [data]) VALUES (@id, @type, @data);", connection))
                {
                    command.Parameters.AddWithValue("@id", (int)id);
                    command.Parameters.AddWithValue("@type", type);
                    command.Parameters.AddWithValue("@data", serialized);
                    updated = await command.ExecuteNonQueryAsync(cancellationToken);
                }

                connection.Close();

                return updated > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION ON SAVE:\r\n{ex}");
                return false;
            }
        }
    }
}