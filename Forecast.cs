using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//Classe usada para armazenar informações da API
namespace ForecastTracker
{
    public class Forecast
    {
        public string Date { get; set; }
        public int MaxTemperature { get; set; }
        public int MinTemperature { get; set; }

        public string Vento { get; set; }
        public string Weekday { get; set; }
        public int RainChance { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }
    }
}
