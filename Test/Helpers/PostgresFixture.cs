// Copyright (c) 2026 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TestSupport.Helpers;
using Testcontainers.PostgreSql;
using Xunit;

namespace Test.Helpers
{
    /// <summary>
    /// Add this as a class fixture to a test class that uses PostgreSQL.
    /// It makes sure that there is a PostgreSQL server at the address in the PostgreSqlConnection
    /// connection string of the appsettings.json file. If no server listens at that address,
    /// then it starts a PostgreSQL container with the port, user name and password of that connection string.
    /// </summary>
    public class PostgresFixture : IAsyncLifetime
    {
        //One container serves the whole test run. Testcontainers removes it when the test process ends
        private static readonly Lazy<Task> ServerIsAvailable = new Lazy<Task>(StartContainerIfNoServerAsync);

        public Task InitializeAsync() => ServerIsAvailable.Value;

        public Task DisposeAsync() => Task.CompletedTask;

        private static async Task StartContainerIfNoServerAsync()
        {
            var connection = new NpgsqlConnectionStringBuilder(AppSettings
                .GetConfiguration(typeof(PostgresFixture).Assembly)
                .GetConnectionString(AppSettings.PostgreSqlConnectionString));

            if (await ServerIsListeningAsync(connection.Host, connection.Port))
                return;

            var container = new PostgreSqlBuilder("postgres:17")
                .WithUsername(connection.Username)
                .WithPassword(connection.Password)
                .WithPortBinding(connection.Port, PostgreSqlBuilder.PostgreSqlPort)
                .Build();
            await container.StartAsync();
        }

        private static async Task<bool> ServerIsListeningAsync(string host, int port)
        {
            using var client = new TcpClient();
            try
            {
                await client.ConnectAsync(host, port).WaitAsync(TimeSpan.FromSeconds(2));
                return true;
            }
            catch (Exception e) when (e is SocketException or TimeoutException)
            {
                return false;
            }
        }
    }
}
