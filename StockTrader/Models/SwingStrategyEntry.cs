using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//inhereits from entry
namespace StockTrader.Models
{
    class SwingStrategyEntry : StrategyEntry, INotifyPropertyChanged
    {


        public SwingStrategyEntry(string SwingName)
        {
            TypeName = "SwingStrategy";
            StrategyName = SwingName;
            OnPropertyChanged("SwingStrategyEntry");
        }

        public string GetTypeName()
        {
            return TypeName;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}