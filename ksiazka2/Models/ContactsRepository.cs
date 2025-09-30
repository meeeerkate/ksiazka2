using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace ksiazka2.Models
{
    public class ContactsRepository
    {
#if ANDROID
        private static readonly string DbPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ksiazka.db");
#elif WINDOWS        
        private static readonly string DbPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Ksiazka.db");
#else
        private static readonly string DbPath = null;
#endif
        private const int PageSize = 5;

        public List<Contact> Contacts { get; private set; } = new();
        public int Offset { get; private set; } = 1;
        public int MaxOffset { get; private set; } = 1;

        public void Initialize()
        {
            LoadContactsPage();
        }

        public void LoadContactsPage(bool all = false)
        {
            Contacts.Clear();

#if ANDROID
            string dbPath = DbPath;
            if (!File.Exists(dbPath))
            {
                try
                {
                    using var assetStream = Android.App.Application.Context.Assets.Open("Ksiazka.db");
                    using var destStream = File.Create(dbPath);
                    assetStream.CopyTo(destStream);
                }
                catch (Exception ex)
                {
                    // You can log this or show a message to the user
                    Console.WriteLine($"[ERROR] Could not copy Ksiazka.db from assets: {ex.Message}");
                    throw new InvalidOperationException("Database file Ksiazka.db not found in Android Assets or could not be copied.", ex);
                }
            }
#elif WINDOWS
            string dbPath = DbPath;
            if (!File.Exists(dbPath))
            {
                var sourcePath = Path.Combine(AppContext.BaseDirectory, "Ksiazka.db");
                if (File.Exists(sourcePath))
                {
                    try
                    {
                        File.Copy(sourcePath, dbPath, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Could not copy Ksiazka.db: {ex.Message}");
                        throw new InvalidOperationException("Database file Ksiazka.db could not be copied on Windows.", ex);
                    }
                }
                else
                {
                    Console.WriteLine("Plik źródłowy bazy nie istnieje: " + sourcePath);
                    throw new FileNotFoundException("Source database file Ksiazka.db not found in application directory.");
                }
            }
#else
            throw new PlatformNotSupportedException("ContactsRepository wspiera tylko Windows i Android.");
#endif

            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();
            using var createCmd = connection.CreateCommand();
            createCmd.CommandText = @"
CREATE TABLE IF NOT EXISTS ksiazka (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Imie TEXT NOT NULL,
    Nazwisko TEXT NOT NULL,
    Tel TEXT NOT NULL
);";
            createCmd.ExecuteNonQuery();
            if (all)
            {
                using var selectCmd2 = connection.CreateCommand();
                selectCmd2.CommandText = "SELECT Id, Imie, Nazwisko, Tel FROM ksiazka ORDER BY Nazwisko, Imie";

                using var reader2 = selectCmd2.ExecuteReader();
                while (reader2.Read())
                {
                    Contacts.Add(new Contact
                    {
                        Id = reader2.GetInt32(0),
                        Name = reader2.GetString(1),
                        LastName = reader2.GetString(2),
                        PhoneNumber = reader2.GetString(3)
                    });
                }
                return;
            }
            
            using var countCmd = connection.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM ksiazka";
            var totalRows = Convert.ToInt32(countCmd.ExecuteScalar());
            MaxOffset = totalRows / PageSize;
            if (totalRows % PageSize != 0)
                MaxOffset++;

            if (Offset > MaxOffset)
                Offset = MaxOffset;

           
            using var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"SELECT Id, Imie, Nazwisko, Tel FROM ksiazka 
                                      ORDER BY Nazwisko, Imie
                                      LIMIT @pageSize OFFSET @offset";
            selectCmd.Parameters.AddWithValue("@pageSize", PageSize);
            selectCmd.Parameters.AddWithValue("@offset", (Offset - 1) * PageSize);

            using var reader = selectCmd.ExecuteReader();
            while (reader.Read())
            {
                Contacts.Add(new Contact
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    LastName = reader.GetString(2),
                    PhoneNumber = reader.GetString(3)
                });
            }
        }

        public void AddContact(Contact contact)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO ksiazka (Imie, Nazwisko, Tel) VALUES (@name, @lastname, @phone)";
            insertCmd.Parameters.AddWithValue("@name", contact.Name);
            insertCmd.Parameters.AddWithValue("@lastname", contact.LastName);
            insertCmd.Parameters.AddWithValue("@phone", contact.PhoneNumber);

            insertCmd.ExecuteNonQuery();

            
            Initialize();
        }

        public void RemoveContact(Contact contact)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = @"DELETE FROM ksiazka WHERE Id = @id";
            deleteCmd.Parameters.AddWithValue("@id", contact.Id);

            deleteCmd.ExecuteNonQuery();

           
            Initialize();
        }

        public void EditContact(Contact oldContact, Contact newContact)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"UPDATE ksiazka 
                                      SET Imie = @name, Nazwisko = @lastname, Tel = @phone 
                                      WHERE Id = @id";
            updateCmd.Parameters.AddWithValue("@name", newContact.Name);
            updateCmd.Parameters.AddWithValue("@lastname", newContact.LastName);
            updateCmd.Parameters.AddWithValue("@phone", newContact.PhoneNumber);
            updateCmd.Parameters.AddWithValue("@id", oldContact.Id);

            updateCmd.ExecuteNonQuery();

            
            Initialize();
        }

        public void PageLeft()
        {
            if (Offset > 1)
                Offset--;
            else if (Offset == 1)
                Offset = MaxOffset;

            LoadContactsPage();
        }

        public void PageRight()
        {
            if (Offset < MaxOffset)
                Offset++;
            else if (Offset == MaxOffset)
                Offset = 1;

            LoadContactsPage();
        }
    }
}
