using System;
using System.Collections.Generic;


//Classe usada para armazenar informações da API

namespace ForecastTracker
{
    public class WeatherData
    {
        public string City { get; set; } //Cidade
        public int Temperature { get; set; } //Temperatura
        public string Conditions { get; set; } //Condição climática

        public string ConditionSlug { get; set; } //Condicoes para as imagens//

        public string Daytime { get; set; } //Status de dia ou noite
        public string MoonPhase { get; set; } //Fase da Lua

        public string WindSpeed { get; set; } // Velocidade do vento
        public int Humidity { get; set; } // Umidade
        public int Cloudiness { get; set; } // Nebulosidade

        public int RainProbability { get; set; } // Probabilidade de chuva
        public List<Forecast> Forecast { get; set; }
    }
}
