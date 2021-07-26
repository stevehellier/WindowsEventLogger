using System;
using System.Data.SqlClient;

namespace uptime_recorder
{
    class App : IApp
    {
        readonly string _connectionsString;
        readonly string _databaseName = Environment.GetEnvironmentVariable("DatabaseName");
        readonly string _databaseServer = Environment.GetEnvironmentVariable("DatabaseServer");
        readonly string _dateTime;
        readonly string _eventId;
        readonly string _event;
        readonly string _machineName;

        public App(string[] args)
        {

            if (string.IsNullOrEmpty(_databaseName) || string.IsNullOrEmpty(_databaseServer))
            {
                Console.WriteLine("Please make sure the environment variables are set correctly!");
                Environment.Exit(0);
            }

            if (!HasArguments(args))
            {
                DisplayHelp();
                Environment.Exit(0);
            }

            _connectionsString = $@"Server={_databaseServer};Database={_databaseName};Trusted_Connection=Yes";
            _dateTime = GetDateTime();
            _event = GetEventType(_eventId);
            _eventId = args[0];
            _machineName = GetMachineName();

        }

        public void Run()
        {
            DoUpdate(_connectionsString);
        }



        private string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private string GetEventType(string id)
        {
            string result;
            int.TryParse(id, out int _id);

            switch (_id)
            {
                case 6005:
                    result = "Startup";
                    break;
                case 6006:
                    result = "Shutdown";
                    break;
                default:
                    result = "Unknown";
                    break;
            }

            return result;

        }

        private string GetMachineName()
        {
            string result;
            if (string.IsNullOrWhiteSpace(System.Net.Dns.GetHostName()))
            {
                result = Environment.MachineName;
            }
            else
            {
                result = System.Net.Dns.GetHostName();
            }
            return result;
        }

        private bool HasArguments(string[] args)
        {
            bool result = false;
            if (args.Length >= 1)
            {
                result = true;
            }

            return result;
        }

        private void DoUpdate(string connectionString)
        {
            try
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    connection.Open();
                    string sql = "INSERT INTO Monitor (server, event_id, event, date_time) values (@server, @event_id, @event, @now)";


                    SqlCommand sqlCommand = new SqlCommand(sql, connection);

                    sqlCommand.Parameters.AddWithValue("server", _machineName);
                    sqlCommand.Parameters.AddWithValue("event_id", _eventId);
                    sqlCommand.Parameters.AddWithValue("event", _event);
                    sqlCommand.Parameters.AddWithValue("now", _dateTime);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter
                    {
                        InsertCommand = sqlCommand
                    };
                    //sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();


                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void DisplayHelp()
        {
            Console.WriteLine();
            Console.WriteLine("UptimeMonitor v0.1");
            Console.WriteLine("Logs Windows EventID to an external database.");
            Console.WriteLine();
            Console.WriteLine("Usage: UptimeMoniter <EventId>");
            Console.WriteLine();
            Console.WriteLine("The database server and name are set via the environment variables");
            Console.WriteLine("\"DatabaseServer\" and \"DatabaseName\"");
        }

    }
}
