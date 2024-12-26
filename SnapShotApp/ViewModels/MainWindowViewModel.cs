using SnapShotApp.Models;
using SnapShotApp.ViewModels.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace SnapShotApp.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private SerialPortModel serialPortModel;
        private bool isSerialPortOpen;
        private BitmapImage imageCapture;

        public ObservableCollection<string> SerialPorts { get; }

        public bool IsSerialPortOpen
        {
            get => isSerialPortOpen;
            set
            {
                isSerialPortOpen = value;
                ConnectCommand.DelegateCanExecute();
                DisConnectCommand.DelegateCanExecute();
                ShootCommand.DelegateCanExecute();
            }
        }

        public BitmapImage ImageCapture 
        {
            get => imageCapture;
            set
            {
                imageCapture = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand<string> ConnectCommand { get; }

        public DelegateCommand DisConnectCommand { get; }

        public DelegateCommand ShootCommand { get; }

        public MainWindowViewModel() 
        {
            SerialPorts = new ObservableCollection<string>();
            ConnectCommand = new DelegateCommand<string>(OnConnectCommand, (string str) => !IsSerialPortOpen);
            DisConnectCommand = new DelegateCommand(OnDisConnectCommand, () => IsSerialPortOpen);
            ShootCommand = new DelegateCommand(OnShootCommand, () => IsSerialPortOpen);

            serialPortModel = new SerialPortModel();
            serialPortModel.SerialPortStatusChanged += SerialPortModel_SerialPortStatusChanged;
            serialPortModel.SerialPortErrorOccurred += SerialPortModel_SerialPortErrorOccurred;

            foreach (var port in serialPortModel.GetPort())
            {
                SerialPorts.Add(port);
            };
        }


        private void OnConnectCommand(string port)
        {
            serialPortModel.PortOpen(port);
            IsSerialPortOpen = serialPortModel.IsOpen;
        }


        private void OnDisConnectCommand()
        {
            serialPortModel.PortClose();
            IsSerialPortOpen = serialPortModel.IsOpen;
        }


        private void OnShootCommand() 
        {
            var command = "\x16\x4D\x0DIMGSNP1L;IMGSHP.";
            var commandBytes = ConvertStringToByteArray(command);

            var data = serialPortModel.Query(commandBytes);

            if (data == null || data.Length < 36) 
            {
                return;
            }

            var imageData = data.Skip(36).ToArray();
            ImageCapture = LoadFromBytes(imageData);
        }


        private void SerialPortModel_SerialPortErrorOccurred(object sender, EventArgs e)
        {
            IsSerialPortOpen = serialPortModel.IsOpen;
        }

        private void SerialPortModel_SerialPortStatusChanged(object sender, EventArgs e)
        {
            IsSerialPortOpen = serialPortModel.IsOpen;
        }


        private byte[] ConvertStringToByteArray(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            var bytes = new List<byte>();

            for (int i = 0; i < str.Length; i++)
            {
                var value = ((int)str[i]).ToString("X2");
                bytes.Add(Convert.ToByte(value, 16));
            }

            return bytes.ToArray();
        }


        private BitmapImage LoadFromBytes(byte[] bytes)
        {
            try
            {
                using (var stram = new MemoryStream(bytes))
                {
                    stram.Seek(0, SeekOrigin.Begin);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = stram;
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.EndInit();

                    return image;
                }
            }
            catch (Exception ex)
            {
                return new BitmapImage();
            }
        }
    }
}