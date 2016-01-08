using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Raycaster
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private WriteableBitmap imageBuffer;

        public WriteableBitmap ImageBuffer
        {
            get { return imageBuffer; }
            set
            {
                if (value != imageBuffer)
                {
                    imageBuffer = value;
                    OnPropertyChanged("ImageBuffer");
                }
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            var handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
