using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace SnapShotApp.Models
{
    public class SerialPortModel
    {
        private SerialPort serialPort;

        public event EventHandler<EventArgs> SerialPortStatusChanged;
        public event EventHandler<EventArgs> SerialPortErrorOccurred;

        public bool IsOpen => serialPort?.IsOpen ?? false;

        /// <summary>
        /// シリアルポートをオープンする
        /// </summary>
        /// <param name="com">ポート番号</param>
        /// <returns>オープンしたかどうか</returns>
        public void PortOpen(string com)
        {
            if (string.IsNullOrEmpty(com)) 
            {
                return;
            }

            // シリアルポートの設定
            serialPort = new SerialPort
            {
                PortName = com,              // ポート番号
                BaudRate = 9600,             // ボーレート
                DataBits = 8,                // データビット
                Parity = Parity.None,        // パリティ
                StopBits = StopBits.One,     // ストップビット
                Handshake = Handshake.None,  // ハンドシェイク
                WriteTimeout = 100-00,       // 書き込みタイムアウト
                ReadTimeout = 10000,         // 読み取りタイムアウト
                NewLine = "\n"               // 改行コード指定
            };

            if (IsOpen) { return; }

            // シリアルポートに接続
            try
            {
                serialPort.Open();  // ポートオープン
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                SerialPortStatusChanged?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// シリアルポートをクローズする
        /// </summary>
        public void PortClose()
        {
            serialPort?.Close();    // ポートクローズ
            serialPort?.Dispose();

            SerialPortStatusChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 文字列を出力バッファに書き込む
        /// </summary>
        /// <param name="data"></param>
        public bool Send(byte[] bytes)
        {
            if (!IsOpen || bytes == null)
            {
                return false;
            }

            var result = false;

            try
            {
                serialPort.Write(bytes, 0, bytes.Length);
                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                SerialPortErrorOccurred?.Invoke(this, new EventArgs());
            }

            return result;
        }


        /// <summary>
        /// データを受信する
        /// </summary>
        /// <returns></returns>
        public byte[] Receive()
        {
            // 受信バッファ
            var bufferList = new List<byte>();

            if (!IsOpen) { return null; }

            try
            {
                // 受信データ待機
                while (serialPort.BytesToRead == 0)
                {
                    Thread.Sleep(1500);
                }

                // 受信
                while (serialPort.BytesToRead > 0)
                {
                    // 1 バイト受信してバッファに格納
                    var buffer = (byte)serialPort.ReadByte();
                    bufferList.Add(buffer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                SerialPortErrorOccurred?.Invoke(this, new EventArgs());
            }

            return bufferList.ToArray();
        }


        /// <summary>
        /// データを送受信する
        /// </summary>
        /// <param name="command">コマンド</param>
        /// <returns></returns>
        public byte[] Query(byte[] command)
        {
            if (!Send(command))
            {
                return null;
            }

            return Receive();
        }


        /// <summary>
        /// 現在、利用できるシリアル通信のポート番号を取得する
        /// </summary>
        /// <returns></returns>
        public string[] GetPort()
        {
            return SerialPort.GetPortNames();
        }
    }
}
