using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Web.UI;


namespace Axelerate.BusinessLayerUITools.ImageProcessing
{
    public class ImgCmpHandler : IHttpHandler
    {
        

        public void ProcessRequest(HttpContext ctx)
        {
            // let's cache this for 1 day
            ctx.Response.ContentType = "image/png";
            ctx.Response.Cache.SetCacheability(HttpCacheability.Public);
            ctx.Response.Cache.SetExpires(DateTime.Now.AddDays(1));
            int buttonWidth = 100;
            int buttonHeight = 30;
            if (ctx.Request.Params["btWidth"] != null)
            {
                string strWidth = (string)ctx.Request.Params["btWidth"];
                int.TryParse(strWidth, out buttonWidth);
            }
            if (ctx.Request.Params["btHeight"] != null)
            {
                string strHeight = (string)ctx.Request.Params["btHeight"];
                int.TryParse(strHeight, out buttonHeight);
            }

            string Text = "";
            if (ctx.Request.Params["Text"] != null)
            {
                Text = (string)ctx.Request.Params["Text"];
            }
            string ImagePath = "";
            Uri ImageUri = null;
            if (ctx.Request.Params["ImagePath"] != null)
            {
                ImagePath = (string)ctx.Request.Params["ImagePath"];
                ImagePath = ImagePath.Replace("$and;", "&");
                if (!ImagePath.ToLower().StartsWith("resources:"))
                {
                    if (ImagePath.ToLower().StartsWith("http"))
                    {
                        ImageUri = new Uri(ImagePath);
                    }
                    else
                    {
                        ImageUri = new Uri(ctx.Request.Url, ImagePath);
                    }
                }

            }
            int FontSize = 8;
            if (ctx.Request.Params["FontSize"] != null)
            {
                string strFontSize = (string)ctx.Request.Params["FontSize"];
                int.TryParse(strFontSize, out FontSize);
            }
            FontFamily FntFamilly = new FontFamily("Arial");
            if (ctx.Request.Params["FontFamilyName"] != null)
            {
                string FontFamilyName = (string)ctx.Request.Params["FontFamilyName"];
                try
                {
                    FntFamilly = new FontFamily(FontFamilyName);
                }
                catch
                {
                }
            }
            bool DropShadow = true;
            if (ctx.Request.Params["DropShadow"] != null)
            {
                string strDropShadow = (string)ctx.Request.Params["DropShadow"];
                bool.TryParse(strDropShadow, out DropShadow);
            }
            bool Bold = true;
            if (ctx.Request.Params["Bold"] != null)
            {
                string strBold = (string)ctx.Request.Params["Bold"];
                bool.TryParse(strBold, out Bold);
            }
            bool RenderAsEnable = true;
            if (ctx.Request.Params["Enabled"] != null)
            {
                string strEnable = (string)ctx.Request.Params["Enabled"];
                bool.TryParse(strEnable, out RenderAsEnable);
            }

            System.Drawing.Color TextColor = Color.Black;
            if (ctx.Request.Params["TextColor"] != null)
            {
                string strTextColor = (string)ctx.Request.Params["TextColor"];
                try
                {
                    if (strTextColor == "")
                    {
                        TextColor = Color.Black;
                    }
                    else
                    {
                        TextColor = System.Drawing.ColorTranslator.FromHtml(strTextColor);
                    }
                }
                catch
                {
                }
            }
            try
            {



                Bitmap BaseBmp = null;
                if (ImagePath.ToLower().StartsWith("resources:"))
                {
                    System.IO.Stream strm = this.GetType().Assembly.GetManifestResourceStream(ImagePath.Remove(0, 10));
                    BaseBmp = (Bitmap)Bitmap.FromStream(strm);
                    BaseBmp = (Bitmap)BaseBmp.Clone();
                    strm.Close();
                    strm.Dispose();
                }
                else
                {
                    //BaseBmp = (Bitmap)Bitmap.FromFile(ctx.Server.MapPath(ImagePath));
                    System.Net.WebClient client = new System.Net.WebClient();
                    byte[] imgData = client.DownloadData(ImageUri);
                    System.IO.Stream strm = new System.IO.MemoryStream(imgData);
                    BaseBmp = (Bitmap)Bitmap.FromStream(strm);
                    BaseBmp = (Bitmap)BaseBmp.Clone();
                    strm.Close();
                    strm.Dispose();
                }

                if (!RenderAsEnable)
                {
                    BaseBmp = GetGrayScaleImage(BaseBmp);
                }
                Font Fnt = null;

                if (Bold)
                {
                    Fnt = new Font(FntFamilly, FontSize, FontStyle.Bold, GraphicsUnit.Point);
                }
                else
                {
                    Fnt = new Font(FntFamilly, FontSize, GraphicsUnit.Point);
                }

                SizeF TextSize = new SizeF(100.0f, 30.0f);
                Bitmap ResultBmp = new Bitmap(1, 1); //only need to do the MeasureString
                using (Graphics gf = Graphics.FromImage(ResultBmp))
                {
                    TextSize = gf.MeasureString(Text, Fnt);
                }

                if (buttonWidth == 0)
                {
                    buttonWidth = (int)TextSize.Width + 12;//need chage it by the mesure text
                }
                if (buttonHeight == 0)
                {
                    if (BaseBmp.Height > ((int)TextSize.Height + 4))
                    {
                        buttonHeight = BaseBmp.Height;
                    }
                    else
                    {
                        buttonHeight = (int)TextSize.Height + 4;
                    }
                }

                ResultBmp = new Bitmap(buttonWidth, buttonHeight); //Recreate the image with the correct size
                //ResultBmp.SetResolution(BaseBmp.HorizontalResolution, BaseBmp.VerticalResolution); 

                int TextLocationX = 0;
                int TextLocationY = 0;

                using (Graphics gf = Graphics.FromImage(ResultBmp))
                {

                    TextLocationX = (int)((buttonWidth / 2) - (TextSize.Width / 2));
                    TextLocationY = (int)((buttonHeight / 2) - (TextSize.Height / 2));
                    float BorderSize = 10.0f;
                    GraphicsUnit N = GraphicsUnit.Pixel;
                    RectangleF Dest = ResultBmp.GetBounds(ref N);
                    RectangleF Src = BaseBmp.GetBounds(ref N); // new Rectangle(0, 0, BaseBmp.Width + 1, BaseBmp.Height + 1);

                    RectangleF LeftDest = new RectangleF(0.0f, 0.0f, BorderSize, Dest.Height);
                    RectangleF LeftSrc = new RectangleF(0.0f, 0.0f, BorderSize, Src.Height);

                    RectangleF RightDest = new RectangleF(Dest.Width - BorderSize, 0.0f, BorderSize, Dest.Height);
                    RectangleF RightSrc = new RectangleF(Src.Width - BorderSize, 0.0f, BorderSize, Src.Height);

                    RectangleF CenterDest = new RectangleF(BorderSize, 0.0f, Dest.Width - (BorderSize * 2), Dest.Height);
                    RectangleF CenterSrc = new RectangleF(BorderSize, 0.0f, Src.Width - (BorderSize * 2), Src.Height);

                    //Src.Width += 10;
                    //Dest.Width -= 10;
                    gf.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    gf.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    gf.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    gf.DrawImage(BaseBmp, CenterDest, CenterSrc, N);
                    gf.DrawImage(BaseBmp, LeftDest, LeftSrc, N);
                    gf.DrawImage(BaseBmp, RightDest, RightSrc, N);
                    gf.DrawString(Text, Fnt, new SolidBrush(TextColor), TextLocationX, TextLocationY);
                    if (DropShadow)
                    {
                        gf.DrawString(Text, Fnt, new SolidBrush(Color.Gray), TextLocationX + 1, TextLocationY + 1);
                    }
                    gf.Flush();
                }

                System.IO.MemoryStream MemStream = new System.IO.MemoryStream();
                ResultBmp.Save(MemStream, System.Drawing.Imaging.ImageFormat.Png);
                MemStream.WriteTo(ctx.Response.OutputStream);
                ctx.Response.ContentType = "image/png";
            }
            catch (Exception ex)
            {
                ctx.Response.Write(ex.Message + "<br/>" + ex.StackTrace + "<br/>" + ex.StackTrace + "<br/>");
                Exception innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    ctx.Response.Write("<hr/>" + innerEx.Message + "<br/>" + innerEx.StackTrace + "<br/>");
                    innerEx = innerEx.InnerException;
                }
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }

        private bool Abort()
        {
            return false;
        }

        /// <summary>
        /// This method draws a grayscale image from a given Image-
        /// instance and gives back the Bitmap of it.
        /// </summary>
        /// <param name="img">the source-image</param>
        /// <returns>Bitmap-Object with grayscale image</returns>
        private Bitmap GetGrayScaleImage(System.Drawing.Image img)
        {
            Bitmap grayBitmap = new Bitmap(img.Width, img.Height);

            System.Drawing.Imaging.ImageAttributes imgAttributes = new System.Drawing.Imaging.ImageAttributes();

            System.Drawing.Imaging.ColorMatrix gray = new System.Drawing.Imaging.ColorMatrix(
                new float[][] {
         new float[] { 0.300f, 0.300f, 0.300f, 0, 0 },
         new float[] { 0.588f, 0.588f, 0.588f, 0, 0}, 
         new float[] { 0.111f, 0.111f, 0.111f, 0, 0 }, 
         new float[] { 0, 0, 0, 1, 0 }, 
         new float[] { 0, 0, 0, 0, 1}, 

        }
            );

            imgAttributes.SetColorMatrix(gray);

            using (Graphics g = Graphics.FromImage(grayBitmap))
            {

                g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height),
                    0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttributes);
            }
            return grayBitmap;
        }
    }
}
