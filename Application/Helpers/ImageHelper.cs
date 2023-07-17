using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Application.Helpers
{
    public static class ImageHelper
    {
        public static Bitmap? Base64ToImage(string base64)
        {
            Bitmap? bm = null;
            try
            {
                // var match = Regex.Match(base64, @"(?:data:image\/[A-Za-z+]+;base64,){0,1}([0-9A-Za-z]+)");
                // if (match.Groups.Count <= 0)
                // {
                //   throw new Exception("Base64 is not valid!");
                // }

                // var base64Fix = match.Groups[0].Value;

                byte[] imageBytes = Convert.FromBase64String(base64);
                var ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

                bm = new Bitmap(ms);
            }
            catch { }
            return bm;
        }

        public static string? ImageToBase64(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Jpeg);
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            var graphics = Graphics.FromImage(destImage);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);

            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            graphics.Save();
            // graphics.DrawImage(uniWm, new Rectangle(10, 960, 120, 48), 0, 0, 120, 48, GraphicsUnit.Pixel);
            // graphics.Save();

            return destImage;
        }
    }
}