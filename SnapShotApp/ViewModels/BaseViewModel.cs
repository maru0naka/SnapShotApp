using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SnapShotApp.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ変更通知イベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更通知イベント呼び出しメソッド
        /// </summary>
        /// <param name="propertyName">変更対象プロパティ名を表す文字列</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
