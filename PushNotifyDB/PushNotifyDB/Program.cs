using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushNotifyDB
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbChange = new DBNotification();
            if(HasEnoughPermission())
            {
                dbChange.StartNotification();
            }           
            Console.WriteLine("To Stop Query Notification Press Enter");
            Console.ReadLine();
            dbChange.StopNotification();
        }

        static private bool HasEnoughPermission()
        {

            SqlClientPermission permsission = new SqlClientPermission(System.Security.Permissions.PermissionState.Unrestricted);
            try
            {
                permsission.Demand();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
    
    class DBNotification
    {
        private delegate void PersonEventNotification(DataTable table);
        private SqlDependency dependency;
        string ConnectionString = @"Data Source=.;Initial Catalog=NotificationDB;Integrated Security=True";
        SqlCommand Command;
        String CommandText = @"select id,name from dbo.Persons";
        public void StartNotification()
        {
            StopNotification();
            SqlDependency.Start(ConnectionString, "PersonChangeMessages");
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            Command = new SqlCommand(CommandText);
            Command.Notification = null;
            Command.Connection = connection;                     
            Command.CommandType = CommandType.Text;
            dependency = new SqlDependency(Command);
            dependency.OnChange += OnPersonEvent;
            SqlDependency.Start(ConnectionString);
            Command.ExecuteReader();
            connection.Close();
        }

        private void OnPersonEvent(object sender, SqlNotificationEventArgs e)
        {
            if (dependency.HasChanges && e.Info != SqlNotificationInfo.AlreadyChanged)
            {
                var dep = sender as SqlDependency;
                dep.OnChange -= OnPersonEvent;
                Console.WriteLine("Data Changed : {0}", e.Info.ToString());               
            }
            StartNotification();

        }        

        public void StopNotification()
        {
            SqlDependency.Stop(ConnectionString, "PersonChangeMessages");
        }
    }
}
