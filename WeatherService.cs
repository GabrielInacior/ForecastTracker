using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

//Classe responsavel por fazer a requisição da API, Armazenar no Banco local, e Manipular os dados em JSON
namespace ForecastTracker
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService()

        {
            _httpClient = new HttpClient();
        }
        public async Task<WeatherData> GetWeatherDataAsync(string city)
        {
            try
            {
                //Requisição da API
                string apiKey = "24b1156e";
                string apiUrl = $"https://api.hgbrasil.com/weather?key={apiKey}&city_name={city}";

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {   
                    //Tratando os dados em JSON usando NewtonSoft
                    string weatherJson = await response.Content.ReadAsStringAsync();
                    dynamic jsonData = JsonConvert.DeserializeObject(weatherJson);

                    var results = jsonData.results;

                    WeatherData weatherData = new WeatherData
                    {  
                        //Armazenando dados da API nos atributos da classe WeatherData 
                        City = results.city,
                        Temperature = results.temp,
                        Conditions = results.description,
                        WindSpeed = results.wind_speedy,
                        Humidity = results.humidity,
                        Cloudiness = results.cloudiness,
                        RainProbability = results.forecast[0].rain_probability,
                        MoonPhase = results.moon_phase,
                        ConditionSlug = results.condition_slug,
                        Daytime = results.currently,
                        Forecast = new List<Forecast>()
                    };

                    //Salvando os valores de Previsao na Lista Forecast para o Grid
                    //Cada instancia da lista Forecast: Forecast[0],[1]... Armazena resultados referentes a cada dia da Previsão
                    //Por padrão a API manda 9 dias
                    foreach (var forecastJson in results.forecast)
                    {
                        Forecast forecast = new Forecast
                        {
                            Date = forecastJson.date,
                            Weekday = forecastJson.weekday,
                            MaxTemperature = forecastJson.max,
                            MinTemperature = forecastJson.min,
                            RainChance = forecastJson.rain_probability,
                            Vento = forecastJson.wind_speedy,
                            Condition = forecastJson.condition,
                            Description = forecastJson.description,
                        };

                        weatherData.Forecast.Add(forecast);  // Adiciona cada previsão à lista
                    }

                    return weatherData;
                }
                else
                {   //Mensagem de Exception
                    throw new Exception("Falha ao obter dados da API.");
                }
            }
            //Mensagem de Exception
            catch (Exception ex)
            {
                throw new Exception("Erro ao processar os dados da API.", ex);
            }
        }

    }
}
