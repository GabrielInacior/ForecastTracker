using System.Data.SqlClient;

//Classe responsavel pelo Banco de dados
namespace ForecastTracker
{
    public class DatabaseService
    {
        //Variavel para puxar a connection String 
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }


        //Inserindo os dados da API no Banco de dados
        public void InsertWeatherData(string city, string weatherJson)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO WeatherData (City, WeatherJson) VALUES (@City, @WeatherJson)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@City", city);
                command.Parameters.AddWithValue("@WeatherJson", weatherJson);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
