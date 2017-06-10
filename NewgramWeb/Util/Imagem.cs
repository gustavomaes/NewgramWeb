using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace NewgramWeb.Util
{
    public static class Imagem
    {
        public static byte[] ToByteArray(this string dadoBase64)
        {
            return Convert.FromBase64String(dadoBase64);
        }

        public static byte[] RedimencionaProporcional(this byte[] imagem, int Largura)
        {
            using (MemoryStream ms = new MemoryStream(imagem))
            {
                Image IMG = Image.FromStream(ms);
                int Altura = int.Parse((double.Parse(Largura.ToString()) / double.Parse(IMG.Width.ToString()) * double.Parse(IMG.Height.ToString())).ToString("0"));
                return Redimenciona(imagem, Largura, Altura);
            }
        }

        public static byte[] Redimenciona(this byte[] imagem, int Largura, int Altura)
        {
            if (imagem == null)
                return null;

            Size S = new Size();
            S.Width = Largura;
            S.Height = Altura;

            using (MemoryStream ms = new MemoryStream(imagem))
            {
                Image IMG = Image.FromStream(ms);

                using (Bitmap dst = new Bitmap(S.Width, S.Height))
                {
                    using (Graphics g = Graphics.FromImage(dst))
                    {
                        g.SmoothingMode = SmoothingMode.Default;
                        g.InterpolationMode = InterpolationMode.Default;
                        g.DrawImage(IMG, 0, 0, S.Width, S.Height);
                    }

                    int Quality = 50;

                    EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality);
                    ImageCodecInfo jpegCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(ee => ee.FormatID == ImageFormat.Jpeg.Guid);

                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = qualityParam;

                    using (MemoryStream RET = new MemoryStream())
                    {
                        dst.Save(RET, jpegCodec, encoderParams);
                        return RET.ToArray();
                    }
                }
            }
        }

    }
}