using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Инкапсуляция логики идентификатора для возможных будущих изменений
    /// </summary>
    public class Identificator
    {
        private static int _ID { get; set; } = 0;
        private int ID { get; set; }
        public override string ToString() => ID.ToString();
        public Identificator()
        {
            ID = _ID;
            _ID++;
        }
    }
}
