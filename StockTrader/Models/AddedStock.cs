using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrader.Models
{
    class AddedStock : INotifyPropertyChanged
    {
        public string Ticker { get; set; }

        public AddedStock(string ticker)
        {
            Ticker = ticker;
            OnPropertyChanged("Ticker");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
