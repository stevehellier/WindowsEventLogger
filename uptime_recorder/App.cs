using System;
using System.Data.SqlClient;

namespace uptime_recorder
{
    class App : IApp
    {
        readonly string[] _args;
        readonly string _connectionsString;
        readonly string _databaseName = "";
        readonly string _databaseServer = "";
        readonly string _dateTime;
        readonly string _eventId;
        readonly string _event;
        readonly string _machineName;

        public App(string[] args)
        {
            _args = args;
            _dateTime = GetDateTime();
            _machineName = GetMachineName();

            _connectionsString = $@"Server={_databaseServer};Database={_databaseName};Trusted_Connection=Yes";

            if (HasArguments())
            {
                _eventId = _args[0];
                _event = GetEventType(_eventId);
            }
        }

        public void Run()
        {
            if (HasArguments())
            {
                DoUpdate(_connectionsString);
            }
            else
            {
                Console.WriteLine("Please specify an event ID to log");
                return;
            }
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

        private bool HasArguments()
        {
            bool result = false;
            if (_args.Length >= 1)
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
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Dispose();


                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
