using Newtonsoft.Json;
using ForecastTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

/*
 * ------------------------------------------------------------------------------------------- *
  Funcionalidades Implementadas:
    Dados da API Desserializados em JSON no Banco LocalDB e exibidos Corretamente na tela
    Botão Refresh que reinicia o método Form1_Load requisitando uma resposta da API novamente
    Icone de Clima que muda conforme o clima atual retornado pela API
    Background Interativo de Dia e noite conforme a resposta da API
    Previsão do Tempo de 9 Datas contando com a data de hoje
 * ------------------------------------------------------------------------------------------- *
 */

namespace ForecastTracker
{
    public partial class Form1 : Form
    {
        //Definindo a variavel timerRelogio do tipo Timer
        private System.Windows.Forms.Timer timerRelogio = new System.Windows.Forms.Timer();

        //Chamando as classes
        private readonly WeatherService _weatherService;
        private readonly DatabaseService _databaseService;

        //Estilização do form

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn
         (
            int nLeftRect,
            int nTopRect,
            int nRigthRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
         );

        private void ApplyRoundCornerToPanel(Panel panel, int radius)
        {
            IntPtr ptr = CreateRoundRectRgn(0, 0, panel.Width, panel.Height, radius, radius);
            panel.Region = Region.FromHrgn(ptr);
        }

        public Form1()
        {
            InitializeComponent();

            //Definindo o Panel de Previão inicialmente Fechado
            panelPrevisao.Visible = false;

            //Aplicando o codigo de cantos arredondados do Form
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 50, 50));

            //Aplicando codigo de cantos arredonados dos Panels
            ApplyRoundCornerToPanel(panel1, 16);
            ApplyRoundCornerToPanel(panel3, 40);
            ApplyRoundCornerToPanel(panel4, 30);
            ApplyRoundCornerToPanel(panel5, 16);
            ApplyRoundCornerToPanel(panel6, 30);
            ApplyRoundCornerToPanel(panel7, 30);
            ApplyRoundCornerToPanel(panel8, 30);
            ApplyRoundCornerToPanel(panel9, 30);
            ApplyRoundCornerToPanel(panel10, 30);
            ApplyRoundCornerToPanel(panel11, 30);
            ApplyRoundCornerToPanel(panel12, 30);
            ApplyRoundCornerToPanel(ClimaPanel, 40);

            //Logica para o sistema de relogio
            timerRelogio.Interval = 1000;
            timerRelogio.Tick += TimerRelogio_Tick;

            //Chamando o Banco LocalDB e a String de Conexão
            _weatherService = new WeatherService();
            _databaseService = new DatabaseService("Data Source=(localdb)\\ForecastTracker;Integrated Security=True;Initial Catalog=WeatherForecastDB;MultipleActiveResultSets=True;Application Name=ForecastTracker;Pooling=False;Min Pool Size=0;Max Pool Size=100;Connect Timeout=30;");

            this.Load += Form1_Load;

            labelDate.Text = ($"{DateTime.Now.ToString("dd/MM/yyyy")}");
            labelDate2.Text = ($"{DateTime.Now.ToString("dd/MM/yyyy")}");
            timerRelogio.Start();
        }

        //Metodo Para traduzir as Fases da Lua recebidas pela API
        private string TranslateMoonPhase(string moonPhaseEnglish)
        {
            Dictionary<string, string> translations = new Dictionary<string, string>
                {
                    {"new", "Nova"},
                    {"waxing_crescent", "Crescente"},
                    {"first_quarter", "Quarto Crescente"},
                    {"waxing_gibbous", "Gibosa Crescente"},
                    {"full", "Lua Cheia"},
                    {"waning_gibbous", "Gibosa Minguante"},
                    {"last_quarter", "Quarto Minguante"},
                    {"waning_crescent", "Minguante Crescente"}
                };

            if (translations.ContainsKey(moonPhaseEnglish))
            {
                return translations[moonPhaseEnglish];
            }
            else
            {
                return "Desconhecido";
            }
        }

        private void ShowForecastInfo(WeatherData weatherData, int index)
        {
            int forecastIndex = index;

            // Verifica se o índice está dentro do intervalo válido (Até 10 datas retornadas pela API)
            if (forecastIndex > -1 && forecastIndex < weatherData.Forecast.Count)
            {
                var forecast = weatherData.Forecast[forecastIndex];

                // Assim como no metodo das Picture Boxes, aqui procurei as Labels correspondentes pelo nome
                Label lblWeek = this.Controls.Find($"LblWeek{index}", true).FirstOrDefault() as Label;
                Label lblMax = this.Controls.Find($"LblMax{index}", true).FirstOrDefault() as Label;
                Label lblMin = this.Controls.Find($"LblMin{index}", true).FirstOrDefault() as Label;
                Label lblDescript = this.Controls.Find($"LblDescript{index}", true).FirstOrDefault() as Label;
                Label lblDia = this.Controls.Find($"LblDia{index}", true).FirstOrDefault() as Label;
                Label lblOperation = this.Controls.Find($"LblOperation{index}", true).FirstOrDefault() as Label;



                // Verifique se as Labels foram encontradas
                if (lblWeek != null && lblMax != null && lblMin != null && lblDescript != null && lblDia != null && lblOperation != null)
                {
                    // Preenchendo as labels com os dados da lista
                    lblWeek.Text = forecast.Weekday;
                    lblMax.Text = $"{forecast.MaxTemperature}°C";
                    lblMin.Text = $"{forecast.MinTemperature}°C";
                    lblDescript.Text = $"{forecast.Description}";
                    lblDia.Text = $"{forecast.Date}";

                    // Calculando a diferença de temperatura com base nas temperaturas maximas
                    double temperatureDifference = 0.0;
                    if (index > 0 && index < weatherData.Forecast.Count)
                    {
                        temperatureDifference = weatherData.Forecast[index].MaxTemperature - weatherData.Forecast[index - 1].MaxTemperature;
                            //Se a diferença for positiva o texto é vermelho
                            if(temperatureDifference > 0)
                            {
                                lblOperation.ForeColor = Color.Red;
                            }
                             //Se a diferença for nula o texto é cinza
                            else if (temperatureDifference == 0)
                            {
                                lblOperation.ForeColor = Color.DarkGray;
                            }
                            //Se a diferença for negativa o texto é azul
                             else
                            {
                                lblOperation.ForeColor = Color.FromArgb(128, 128, 255);
                            }
                    }

                    lblOperation.Text = $"{temperatureDifference}°C";

                    //Adicionando um + caso a diferença de temperatura for positiva
                    if (temperatureDifference > 0)
                    {
                        lblOperation.Text = $"+{temperatureDifference}°C";
                    }


                    // Deixar as  Labels visíveis
                    lblWeek.Visible = true;
                    lblMax.Visible = true;
                    lblDescript.Visible = true;
                    lblMin.Visible = true;
                    lblDia.Visible = true;
                    lblOperation.Visible = true;
                }
                //Mensagem de Exception
                else
                {
                    Debug.WriteLine($"Labels not found for index {index}");
                }
            }
            else
            {
                Debug.WriteLine($"Forecast index {forecastIndex} out of range.");
            }
        }


        //Metodo para alterar o icone do Clima com base na resposta da api para cada data
        private void SetConditionImage(WeatherData weatherData, int index)
        {
            //Dicionario que armazena as respostas da API e associa uma icone diferente para cada
            Dictionary<string, Image> conditionImages = new Dictionary<string, Image>
                {
                    { "storm", Properties.Resources.storm },
                    { "snow",  Properties.Resources.snow  },
                    { "hail",  Properties.Resources.hail },
                    { "rain",  Properties.Resources.rain  },
                    { "fog",  Properties.Resources.fog  },
                    { "clear_day",  Properties.Resources.clear_day  },
                    { "clear_night",  Properties.Resources.clear_night  },
                    { "cloud",  Properties.Resources.cloud  },
                    { "cloudly_day",  Properties.Resources.cloudly_day  },
                    { "cloudly_night",  Properties.Resources.cloudly_night  }
                };

            // Obtém o índice do forecast correspondente à PictureBox
            int forecastIndex = index;

            // Verifica se o índice está dentro do intervalo válido
            if (forecastIndex > -1 && forecastIndex < weatherData.Forecast.Count)
            {
                string condition = weatherData.Forecast[forecastIndex].Condition.ToLower();

                if (conditionImages.ContainsKey(condition))
                {
                    PictureBox pictureBox = this.Controls.Find($"PrevImage{index}", true).FirstOrDefault() as PictureBox;

                    if (pictureBox != null)
                    {
                        pictureBox.Image = conditionImages[condition];
                        pictureBox.Visible = true;
                    }

                    //Mensagem de Exception
                    else
                    {
                        Debug.WriteLine($"PictureBox PrevImage{index} not found.");
                    }
                }
                //Mensagem de Exception
                else
                {
                    Debug.WriteLine($"Condition image not found for condition: {condition}");
                }
            }
            //Mensagem de Exception
            else
            {
                Debug.WriteLine($"Forecast index {forecastIndex} out of range.");
            }
        }


        //Metodo para mudar a Imagem de Background com base na resposta "currently"(Dia ou Noite)
        private void SetDaytimeBackground(WeatherData weatherData)
        {
            Dictionary<string, Image> DaytimeBackground = new Dictionary<string, Image>
            {
                { "dia", Properties.Resources.DayBackground },
                { "noite",  Properties.Resources.NightBacground1 }
            };

            string condition = weatherData.Daytime.ToLower();
            if (DaytimeBackground.ContainsKey(condition))
            {
                this.DayBackground.Image = DaytimeBackground[condition];           
                if (condition == "dia")
                {

                    //Para a tela Hoje
                    PanelCover.BackColor = Color.FromArgb(61, 138, 201);
                    labelCity.BackColor = Color.FromArgb(61, 138, 201);
                    labelCity.ForeColor = Color.FromArgb(24, 30, 54);
             
          

                }
                this.DayBackground.Visible = true;
            }
            //Caso a condição não seja encontrada, PictureBox exibe a Imagem de clima Padrão(Cloudly Day)
            else
            {
                this.DayBackground.Visible = false;
                Debug.WriteLine($"Condition image not found for condition: {condition}");
            }

        }

        //Trocar Background para a Tela Previsões (imagens diferentes por isso outro método)

        private void SetDaytimeBackground2(WeatherData weatherData)
        {
            Dictionary<string, Image> DaytimeBackground = new Dictionary<string, Image>
            {
                { "dia", Properties.Resources.DayBackgroundPrev1 },
                { "noite",  Properties.Resources.NIghtBackgroundPrev1 }
            };

            string condition = weatherData.Daytime.ToLower();
            if (DaytimeBackground.ContainsKey(condition))
            {      
                this.DayBackground2.Image = DaytimeBackground[condition];     
                this.DayBackground.Visible = true;
            }
            //Caso a condição não seja encontrada, PictureBox exibe a Imagem de clima Padrão(Cloudly Day)
            else
            {
                this.DayBackground.Visible = false;
                Debug.WriteLine($"Condition image not found for condition: {condition}");
            }

        }


        //metodo para Atualizar os Dados e Inseri-los na Tela
        private async void RefreshWeatherData()
        {
            try
            {
                string city = "Jales";

                WeatherData weatherData = await _weatherService.GetWeatherDataAsync(city);

                _databaseService.InsertWeatherData(city, $"{weatherData.City} - {weatherData.Temperature}°C, {weatherData.Conditions}");

                // Atualizar a UI com novos dados
                labelCity.Text = ($"{weatherData.City}");
                labelCity2.Text = ($"{weatherData.City}");
                labelCondicao.Text = ($"{weatherData.Conditions}");
                labelTemp.Text = ($"{weatherData.Temperature}°C");
                labelChuva.Text = ($"Chance de chuva: {weatherData.RainProbability}%");
                labelUmi.Text = ($"Umidade: {weatherData.Humidity}%");
                labelVento.Text = ($"Vento: {weatherData.WindSpeed}");
                MoonLabel.Text = ($"Lua: {TranslateMoonPhase(weatherData.MoonPhase)}");
                labelMist.Text = ($"Nebulosidade: {weatherData.Cloudiness}%");
                SetConditionImage(weatherData, 0);
                SetDaytimeBackground(weatherData);
                SetDaytimeBackground2(weatherData);

                //Exibindo as imagens de Clima na tela
                SetConditionImage(weatherData, 0);
                SetConditionImage(weatherData, 1);
                SetConditionImage(weatherData, 2);
                SetConditionImage(weatherData, 3);
                SetConditionImage(weatherData, 4);
                SetConditionImage(weatherData, 5);
                SetConditionImage(weatherData, 6);
                SetConditionImage(weatherData, 7);
                SetConditionImage(weatherData, 8);
                SetDaytimeBackground(weatherData);

                //Exibindo as Informações da Tela de Previsão do tempo

                ShowForecastInfo(weatherData, 1);
                ShowForecastInfo(weatherData, 2);
                ShowForecastInfo(weatherData, 3);
                ShowForecastInfo(weatherData, 4);
                ShowForecastInfo(weatherData, 5);
                ShowForecastInfo(weatherData, 6);
                ShowForecastInfo(weatherData, 7);
                ShowForecastInfo(weatherData, 8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter dados da previsão do tempo: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //Chamando o Metodo GetWeatherData para invocar a API
                string city = "Jales";

                WeatherData weatherData = await _weatherService.GetWeatherDataAsync(city);

                _databaseService.InsertWeatherData(city, $"{weatherData.City} - {weatherData.Temperature}°C, {weatherData.Conditions}");

             
            }
            //Mensagem de Exception//
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter dados da previsão do tempo: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshWeatherData();
            timerRelogio.Start();

            //Estilizações de Panels por cima dos dados para "Mascarar" os dados sendo carregados
            panelLoad.Visible = false;
            panelLoad2.Visible = false;
            panelLoad3.Visible = false;
            panelLoad4.Visible = false;

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
        }

       
        //Estilização do Botão Home
        private void btnHome_Click(object sender, EventArgs e)
        {
            //Fechando o Panel De Previsão do Tempo ao clicar no botao
            panelPrevisao.Visible = false;
            PanelCover.Visible = true;
            pnlNav.Height = btnHome.Height;
            pnlNav.Top = btnHome.Top;
            pnlNav.Left = btnHome.Left;
            btnHome.BackColor = Color.FromArgb(46, 51, 73);
        }

        //Funcionalidade de Relógio
        private void TimerRelogio_Tick(object sender, EventArgs e)
        {
            // Atualiza o texto da Label com o horário atual formatado
            labelRelogio.Text = DateTime.Now.ToString("HH:mm:ss");
            labelRelogio2.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        //Estilização do Botão de Fase da Lua atual
        private void btnPrevisao_Click(object sender, EventArgs e)
        {
            //Exibindo o Panel De Previsão do Tempo ao clicar no botao
            panelPrevisao.Visible = true;
            PanelCover.Visible = false;
            pnlNav.Height = btnPrevisao.Height;
            pnlNav.Top = btnPrevisao.Top;
            pnlNav.Left = btnPrevisao.Left;
            btnPrevisao.BackColor = Color.FromArgb(46, 51, 73);
        }

        //Funcionalidade do botão Sair
        private void btnQuit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        //Retornar ao background original do botao Home
        private void btnHome_Leave(object sender, EventArgs e)
        {
            btnHome.BackColor = Color.FromArgb(24, 30, 54);
        }

        //Retornar ao background original do botao Lua
        private void btnPrevisao_Leave(object sender, EventArgs e)
        {
            btnPrevisao.BackColor = Color.FromArgb(24, 30, 54);
        }

        //Botao para atualizar os dados e solicitar outra resposta da API
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshWeatherData();
            btnRefresh.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void btnRefresh_Leave(object sender, EventArgs e)
        {
            btnRefresh.BackColor = Color.FromArgb(46, 51, 73);
        }
       
    }
}
