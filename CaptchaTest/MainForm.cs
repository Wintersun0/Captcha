using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace CaptchaTest
{
    public partial class MainForm : Form
    {
        private Captcha _captcha = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _captcha = new Captcha(6, Captcha.CodeType.Alphas);
            pictureBox1.Image = Bitmap.FromStream(_captcha.CreateCaptchaImage());
        }

        private void btnGetNetTime_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                Thread thd = new Thread(() =>
                {
                    DateTime dt = GetNetworkTime();
                    label1.Invoke((MethodInvoker)delegate
                    {
                        label1.Text = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    });
                });
                thd.Start();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private DateTime GetNetworkTime()
        {
            const string ntpServer = "pool.ntp.org";
            const int ntpDataLength = 48;

            byte[] ntpData = new byte[ntpDataLength];

            // Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            IPAddress[] addresses = Dns.GetHostAddresses(ntpServer);
            IPEndPoint endPoint = new IPEndPoint(addresses[0], 123);

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.ReceiveTimeout = 5000; // 设置超时时间为5秒

                try
                {
                    socket.Connect(endPoint);
                    socket.Send(ntpData);
                    socket.Receive(ntpData);

                    // Offset to get to the "Transmit Timestamp" field (time at which the reply 
                    // departed the server for the client, in 64-bit timestamp format.
                    const int serverReplyTime = 40;

                    ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
                    ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                    // Convert From big-endian to little-endian
                    intPart = SwapEndianness(intPart);
                    fractPart = SwapEndianness(fractPart);

                    var milliseconds = (intPart * 1000 + (fractPart * 1000) / 0x100000000L);
                    var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

                    return networkDateTime.ToLocalTime();
                }
                catch (SocketException ex)
                {
                    // Handle socket exception (e.g., timeout)
                    Console.WriteLine($"Socket exception: {ex.SocketErrorCode}");
                    throw; // Rethrow the exception or handle as needed
                }
            }
        }

        private uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                          ((x & 0x0000ff00) << 8) +
                          ((x & 0x00ff0000) >> 8) +
                          ((x & 0xff000000) >> 24));
        }


        private void btnCaptcha_Click(object sender, EventArgs e)
        {
            string captcha = textBox1.Text;
            if (_captcha.CaptchaString == captcha.ToUpper())
            {
                MessageBoxHelper.Show("验证成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information, this);
            }
            else
            {
                MessageBoxHelper.Show("验证失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error, this);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Bitmap.FromStream(_captcha.CreateCaptchaImage());
        }
    }
}
