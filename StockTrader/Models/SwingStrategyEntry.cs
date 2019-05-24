using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//coped bucket strat
namespace StockTrader.Models
{
    class SwingStrategyEntry : INotifyPropertyChanged
    {
        public string SwingStrategyName { get; set; }

        public SwingStrategyEntry(string bsn)
        {
            SwingStrategyName = bsn;
            OnPropertyChanged("SwingStrategyEntry");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}