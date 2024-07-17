using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace CaptchaTest
{
    class Captcha
    {
        #region Private Fields
        private const double Pi2 = 6.283185307179586476925286766559;
        private readonly int _length;
        private readonly CodeType _type;
        private readonly float _space = 26.0f;
        private readonly float _height = 36.0f;
        private string _captcha;
        private readonly Random _random = new Random();
        #endregion

        #region Public Property
        public string CaptchaString => _captcha;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Captcha class.
        /// </summary>
        /// <param name="length">Length of the captcha.</param>
        /// <param name="type">Type of the captcha: Letters, Numbers, Letters + Numbers.</param>
        public Captcha(int length, CodeType type)
        {
            _length = length;
            _type = type;
        }
        #endregion

        #region Public Field
        public enum CodeType { Words, Numbers, Characters, Alphas }
        #endregion

        #region Private Methods
        private string GenerateNumbers()
        {
            char[] result = new char[_length];
            for (int i = 0; i < _length; i++)
            {
                result[i] = (char)(_random.Next(10) + '0');
            }
            return new string(result);
        }

        private string GenerateCharacters()
        {
            char[] result = new char[_length];
            for (int i = 0; i < _length; i++)
            {
                result[i] = (char)(_random.Next(26) + 'A');
            }
            return new string(result);
        }

        private string GenerateAlphas()
        {
            char[] result = new char[_length];
            for (int i = 0; i < _length; i++)
            {
                result[i] = _random.Next(2) == 0 ? (char)(_random.Next(10) + '0') : (char)(_random.Next(26) + 'A');
            }
            return new string(result);
        }

        private Bitmap TwistImage(Bitmap srcBmp, bool bXDir, double dMultValue, double dPhase)
        {
            Bitmap destBmp = new Bitmap(srcBmp.Width, srcBmp.Height);

            using (Graphics graph = Graphics.FromImage(destBmp))
            {
                graph.FillRectangle(Brushes.White, 0, 0, destBmp.Width, destBmp.Height);
            }

            double dBaseAxisLen = bXDir ? destBmp.Height : destBmp.Width;

            for (int i = 0; i < destBmp.Width; i++)
            {
                for (int j = 0; j < destBmp.Height; j++)
                {
                    double dx = bXDir ? (Pi2 * j) / dBaseAxisLen : (Pi2 * i) / dBaseAxisLen;
                    dx += dPhase;
                    double dy = Math.Sin(dx);

                    int nOldX = bXDir ? i + (int)(dy * dMultValue) : i;
                    int nOldY = bXDir ? j : j + (int)(dy * dMultValue);

                    if (nOldX >= 0 && nOldX < destBmp.Width && nOldY >= 0 && nOldY < destBmp.Height)
                    {
                        destBmp.SetPixel(nOldX, nOldY, srcBmp.GetPixel(i, j));
                    }
                }
            }

            return destBmp;
        }

        private void DrawNoiseLines(Graphics g, Bitmap image)
        {
            for (int i = 0; i < 12; i++)
            {
                int x1 = _random.Next(image.Width);
                int x2 = _random.Next(image.Width);
                int y1 = _random.Next(image.Height);
                int y2 = _random.Next(image.Height);
                g.DrawLine(new Pen(Color.Gray), x1, y1, x2, y2);
            }
        }

        private void DrawCaptchaText(Graphics g, Bitmap image, string checkCode)
        {
            Font font = new Font("Consolas", 20, FontStyle.Bold);
            using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true))
            {
                for (int i = 0; i < checkCode.Length; i++)
                {
                    g.DrawString(checkCode[i].ToString(), font, brush, 2 + i * _space, 2);
                }
            }
        }

        private void DrawNoiseDots(Bitmap image)
        {
            for (int i = 0; i < 100; i++)
            {
                int x = _random.Next(image.Width);
                int y = _random.Next(image.Height);
                image.SetPixel(x, y, Color.Gray);
            }
        }
        #endregion

        #region Public Methods
        public Stream CreateCaptchaImage()
        {
            // Generate captcha code based on type
            switch (_type)
            {
                case CodeType.Numbers:
                    _captcha = GenerateNumbers();
                    break;
                case CodeType.Characters:
                    _captcha = GenerateCharacters();
                    break;
                default:
                    _captcha = GenerateAlphas();
                    break;
            }

            if (string.IsNullOrEmpty(_captcha)) return null;

            Bitmap image = new Bitmap((int)Math.Ceiling(_captcha.Length * _space), (int)_height);

            using (Graphics g = Graphics.FromImage(image))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.Clear(Color.White);

                DrawNoiseLines(g, image);
                DrawCaptchaText(g, image, _captcha);
                DrawNoiseDots(image);

                if (_type != CodeType.Words)
                {
                    image = TwistImage(image, true, 3, 1);
                }

                g.DrawRectangle(Pens.Silver, 0, 0, image.Width - 1, image.Height - 1);

                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms;
            }
        }
        #endregion
    }
}
