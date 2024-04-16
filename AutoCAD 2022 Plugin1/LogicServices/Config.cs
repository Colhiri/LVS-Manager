using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace AutoCAD_2022_Plugin1.LogicServices
{
    /// <summary>
    /// Конфигурация для параметров отрисовки объектов на полилинии
    /// </summary>
    public class Config
    {
        // Путь к конфигурации (Автоматический путь)
        private string PathToConfigFile = $"ConfigFile.json";

        // Единственный экземпляр класса (Синглтон)
        private static Config instance;
        // Цвет полилинии макета
        public int ColorIndexForField { get; set; }
        // Цвет полилинии видового экрана
        public int ColorIndexForViewport { get; set; }
        // Граница между полилинями макетов
        public double BorderValueLayout { get; set; }
        // Дополнительное общее уменьшение размера полилиний макетов и видовых экранов
        public string DownScale { get; set; }
        // Стартовый плоттер
        public string DefaultPlotter { get; set; }
        // Стартовая точка для полилиний
        public Point2d StartPointModel { get; set; }

        /// <summary>
        /// Получить текущий экземпляр класса
        /// </summary>
        /// <param name="PathToConfigFile"></param>
        /// <returns></returns>
        public static Config GetConfig()
        {
            if (instance == null)
            {
                instance = new Config();
            }
            return instance;
        }

        /// <summary>
        /// Закрытый конструктор для синглтона
        /// </summary>
        private Config()
        {
            if (!File.Exists(PathToConfigFile))
            {
                Application.ShowAlertDialog("Configuration does not exists file. \nI will create a new file with default parameters!");
                DefaultInitialize();
                using (FileStream f = new FileStream(PathToConfigFile, FileMode.OpenOrCreate))
                {
                    JsonSerializer.Serialize<Config>(f, this);
                }
            }
            else
            {
                using (FileStream f = new FileStream(PathToConfigFile, FileMode.OpenOrCreate))
                {
                    Config config = JsonSerializer.Deserialize<Config>(f);
                    InitializeWithCopy(config);
                }
            }
        }

        [JsonConstructor]
        private Config(int ColorIndexForField, int ColorIndexForViewport, double BorderValueLayout, string DownScale, string DefaultPlotter, Point2d StartPointModel) 
        {
            this.ColorIndexForField = ColorIndexForField ; 
            this.ColorIndexForViewport = ColorIndexForViewport ; 
            this.BorderValueLayout = BorderValueLayout ; 
            this.DownScale = DownScale ; 
            this.DefaultPlotter = DefaultPlotter ;
            this.StartPointModel = StartPointModel;
        }

        /// <summary>
        /// Задать стандартные параметры работы
        /// </summary>
        private void DefaultInitialize()
        {
            ColorIndexForField = 3;
            ColorIndexForViewport = 4;
            BorderValueLayout = 300;
            DownScale = "1:1";
            DefaultPlotter = "Нет";
            StartPointModel = new Point2d(0, 0);
        }

        /// <summary>
        /// Задает параметры из переданной копии класса (используется при инициализации после десериализации)
        /// </summary>
        /// <param name="config"></param>
        private void InitializeWithCopy(Config config)
        {
            ColorIndexForField = config.ColorIndexForField;
            ColorIndexForViewport = config.ColorIndexForViewport;
            BorderValueLayout = config.BorderValueLayout;
            DownScale = config.DownScale;
            DefaultPlotter = config.DefaultPlotter;
            StartPointModel = config.StartPointModel;
        }
    }
}
