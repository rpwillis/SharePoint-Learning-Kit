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
    public class ImageHandler : IHttpHandler
    {


        public void ProcessRequest(HttpContext ctx)
        {
            // let's cache this for 1 day
            ctx.Response.ContentType = "image/png";
            ctx.Response.Cache.SetCacheability(HttpCacheability.Public);
            ctx.Response.Cache.SetExpires(DateTime.Now.AddDays(1));

            string ImageForumResource = ctx.Request.Params["ImageForumResource"];
            string ImageDocumentTypeIcon = ctx.Request.Params["ImageDocumentTypeIcon"];
            string ImageDocumentModeIcon = ctx.Request.Params["ImageDocumentModeIcon"];

            string ObjectClass = ctx.Server.UrlDecode(ctx.Request.Params["ObjectClass"]);
            string ObjectProperty = ctx.Request.Params["ObjectProperty"];
            string ObjectGUID = ctx.Request.Params["ObjectGUID"];

            string PBarWidth = ctx.Request.Params["PBarWidth"];
            string PBarHeight = ctx.Request.Params["PBarHeight"];
            string PBarTubeColor = ctx.Request.Params["PBarTubeColor"];
            string PBarContainerColor = ctx.Request.Params["PBarFillColor"];
            string PBarTextColor = ctx.Request.Params["PBarTextColor"];
            string IsProgressBar = ctx.Request.Params["IsProgressBar"];
            string FixedImageResourceName = ctx.Request.Params["FixedImageURL"];
            string PBarHorizontal = ctx.Request.Params["PBarHorizontal"];
            
            if (ObjectClass != null && FixedImageResourceName != null)
            {
                try
                {
                    System.Type objType = System.Type.GetType(ObjectClass);
                    System.Resources.ResourceManager rm = new global::System.Resources.ResourceManager("Axelerate.SlkCourseManagerLogicalLayer.Resources.ActivityStatusImages", objType.Assembly);
                    Bitmap bmp = (Bitmap)rm.GetObject(FixedImageResourceName);
                    System.IO.MemoryStream MemStream = new System.IO.MemoryStream();
                    bmp.Save(MemStream, ImageFormat.Png);
                    MemStream.WriteTo(ctx.Response.OutputStream);
                }
                catch (Exception exc)
                {
                    ctx.Response.Write(exc.Message + "<br/>" + exc.StackTrace + "<br/>" + exc.StackTrace + "<br/>");
                    Exception innerEx = exc.InnerException;
                    while (innerEx != null)
                    {
                        ctx.Response.Write("<hr/>" + innerEx.Message + "<br/>" + innerEx.StackTrace + "<br/>");
                        innerEx = innerEx.InnerException;
                    }
                }
            }
            if (ObjectGUID != null && ObjectGUID.Trim() != "" && ObjectProperty != null && ObjectClass != null)
            {
                ctx.Response.ContentType = "image/png";
                Bitmap bmp;

                try
                {
                    Object item = ReflectionHelper.GetSharedBusinessClassProperty(ObjectClass, "TryGetObjectByGUID", new object[] { ObjectGUID, null });
                    if (item != null)
                    {
                        if (IsProgressBar != null)
                        {
                            System.IO.MemoryStream MemStream = new System.IO.MemoryStream();
                            object objProperty;

                            const string strRegularExpression = "(\\d+)(\\.(\\d+))?";
                            System.Text.RegularExpressions.Regex regExPattern = new System.Text.RegularExpressions.Regex(strRegularExpression);
                            System.Text.RegularExpressions.Match matPattern = regExPattern.Match(ObjectProperty);

                            if (matPattern.Success)
                            {
                                objProperty = System.Convert.ToDouble(matPattern.Value);
                            }
                            else objProperty = System.Convert.ToDouble(DataBinder.Eval(item, ObjectProperty));

                            double percent = System.Convert.ToDouble(objProperty);
                            if (percent > 0 && percent < 1)
                            {
                                int imageWidth, imageHeight;
                                bool horizontal;
                                Color penTubeColor;
                                Color penFillColor;
                                Color penTextColor;

                                #region Validation and variable values
                                if (PBarHorizontal == null)
                                {
                                    horizontal = true;
                                }
                                else horizontal = System.Convert.ToBoolean(PBarHorizontal);
                                if (PBarWidth == null || PBarHeight == null)
                                {
                                    imageHeight = 25;
                                    imageWidth = 50;
                                }
                                else
                                {
                                    imageHeight = System.Convert.ToInt32(PBarHeight);
                                    imageWidth = System.Convert.ToInt32(PBarWidth);
                                }
                                if (PBarTubeColor != null)
                                {
                                    penTubeColor = ColorTranslator.FromHtml(PBarTextColor);
                                }
                                else penTubeColor = Color.AntiqueWhite;
                                if (PBarTextColor != null)
                                {
                                    penTextColor = ColorTranslator.FromHtml(PBarTextColor);
                                }
                                else penTextColor = Color.Black;
                                if (PBarContainerColor != null)
                                {
                                    penFillColor = ColorTranslator.FromHtml(PBarTextColor);
                                }
                                else penFillColor = Color.Peru;
                                bmp = new Bitmap(imageWidth, imageHeight);
                                #endregion

                                using (Graphics gf = Graphics.FromImage(bmp))
                                {
                                    gf.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                    drawProgressBar(gf, percent, horizontal, bmp.Width, bmp.Height);
                                    gf.Flush();
                                }

                                bmp.Save(MemStream, ImageFormat.Png);
                                MemStream.WriteTo(ctx.Response.OutputStream);
                            }
                        }
                        else
                        {
                            object objProperty = DataBinder.Eval(item, ObjectProperty);
                            bmp = (Bitmap)objProperty;
                            if (bmp != null)
                            {
                                System.IO.MemoryStream MemStream = new System.IO.MemoryStream();
                                bmp.Save(MemStream, ImageFormat.Png);
                                MemStream.WriteTo(ctx.Response.OutputStream);
                            }
                            else
                            {
                                bmp = null;// (Bitmap)Resources.Shared.ImgNoContactPicture;
                                System.IO.MemoryStream MemStream = new System.IO.MemoryStream();
                                bmp.Save(MemStream, ImageFormat.Png);
                                MemStream.WriteTo(ctx.Response.OutputStream);
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    ctx.Response.Write(exc.Message + "<br/>" + exc.StackTrace + "<br/>" + exc.StackTrace + "<br/>");
                    Exception innerEx = exc.InnerException;
                    while (innerEx != null)
                    {
                        ctx.Response.Write("<hr/>" + innerEx.Message + "<br/>" + innerEx.StackTrace + "<br/>");
                        innerEx = innerEx.InnerException;
                    }
                }
            }
            else
            {
                ctx.Response.ContentType = "text/plain";
                ctx.Response.Write(Resources.ErrorMessages.errNothingToDisplay);
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

        #region Drawing Methods
        private void drawProgressBar(Graphics graph, double percentage, bool direction, int pbWidth, int pbHeight)
        {
            if (direction)
            {
                if (percentage > 0)
                    drawFilledTubeH(graph, Color.Black, Color.Red, Color.AntiqueWhite, percentage, pbWidth, pbHeight);
                drawEmptyTubeH(graph, Color.Black, pbWidth, pbHeight);
            }
            else
            {
                if (percentage > 0)
                    drawFilledTubeV(graph, Color.Black, Color.Red, Color.AntiqueWhite, percentage, pbWidth, pbHeight);
                drawEmptyTubeV(graph, Color.Black, pbWidth, pbHeight);
            }
        }
        private void drawEmptyTubeH(Graphics graph, Color penTubeColor, int width, int height)
        {
            float arcWidth = width * 10 / 100;
            float arcHeight = (float)(height - 1);
            float ellipseWidth = arcWidth * 2 - 1;
            float tubeLenght = width - ellipseWidth;

            Pen tubePen = new Pen(penTubeColor);
            graph.DrawArc(tubePen, 0, 0, ellipseWidth, arcHeight, 90, 180);
            graph.DrawLine(tubePen, arcWidth, 0, width - arcWidth, 0);
            graph.DrawLine(tubePen, arcWidth, arcHeight, width - arcWidth, arcHeight);
            graph.DrawEllipse(tubePen, tubeLenght, 0, ellipseWidth - 1, arcHeight);
        }
        private void drawFilledTubeH(Graphics graph, Color penTubeColor, Color penFillColor, Color penTextColor, double percent, int width, int height)
        {
            float arcWidth = width * 10 / 100;
            float arcHeight = (float)(height - 1);
            float ellipseWidth = arcWidth * 2 - 1;
            float tubeLenght = width - ellipseWidth;

            float fpercent = System.Convert.ToSingle(percent);

            if (percent != 0)
            {
                Pen tubePen = new Pen(penFillColor);
                graph.FillEllipse(tubePen.Brush, 0, 0, ellipseWidth, arcHeight);
                graph.FillRectangle(tubePen.Brush, arcWidth, 0, tubeLenght * fpercent, arcHeight);
                graph.FillEllipse(tubePen.Brush, tubeLenght * fpercent, 0, ellipseWidth - 1, arcHeight);
                Pen tubePen2 = new Pen(penTubeColor);
                graph.DrawEllipse(tubePen2, tubeLenght * fpercent, 0, ellipseWidth - 1, arcHeight);
            }
            Pen tubePen3 = new Pen(penTextColor);
            Font f = new Font("Verdana", System.Convert.ToSingle(arcHeight * 0.35));
            string percentageString = (percent * 100).ToString() + "%";
            SizeF stringSize = graph.MeasureString(percentageString, f);
            PointF textLocation = new PointF((tubeLenght / 2) - (stringSize.Width / 2), (arcHeight / 2) - (stringSize.Height / 2));
            graph.DrawString(percentageString, f, tubePen3.Brush, textLocation);
        }
        private void drawEmptyTubeV(Graphics graph, Color penTubeColor, int width, int height)
        {
            float arcHeight = height * 10 / 100;
            float arcWidth = (float)(width - 1);
            float ellipseHeight = arcHeight * 2;
            float tubeLenght = height - ellipseHeight;

            Pen tubePen = new Pen(penTubeColor);
            graph.DrawArc(tubePen, 0, tubeLenght, arcWidth, ellipseHeight - 1, 0, 180);
            graph.DrawLine(tubePen, 0, arcHeight, 0, height - arcHeight);
            graph.DrawLine(tubePen, arcWidth, arcHeight, arcWidth, height - arcHeight);
            graph.DrawEllipse(tubePen, 0, 0, arcWidth, ellipseHeight);
        }
        private void drawFilledTubeV(Graphics graph, Color penTubeColor, Color penFillColor, Color penTextColor, double percent, int width, int height)
        {
            float arcHeight = height * 10 / 100;
            float arcWidth = (float)(width - 1);
            float ellipseHeight = arcHeight * 2;
            float tubeLenght = height - ellipseHeight;

            float fpercent = System.Convert.ToSingle(percent);

            if (percent != 0)
            {
                Pen tubePen = new Pen(penFillColor);
                graph.FillEllipse(tubePen.Brush, 0, tubeLenght, arcWidth, ellipseHeight);
                graph.FillRectangle(tubePen.Brush, 0, (tubeLenght - (tubeLenght * fpercent)) + arcHeight, arcWidth, tubeLenght * fpercent);
                graph.FillEllipse(tubePen.Brush, 0, (tubeLenght - (tubeLenght * fpercent)), arcWidth - 1, ellipseHeight);
                Pen tubePen2 = new Pen(penTubeColor);
                graph.DrawEllipse(tubePen2, 0, (tubeLenght - (tubeLenght * fpercent)), arcWidth - 1, ellipseHeight);
            }
            Pen tubePen3 = new Pen(penTextColor);
            Font f = new Font("Verdana", System.Convert.ToSingle(arcWidth * 0.20), FontStyle.Bold);
            string percentageString = (percent * 100).ToString() + "%";
            SizeF stringSize = graph.MeasureString(percentageString, f);
            PointF textLocation = new PointF((arcWidth / 2) - (stringSize.Width / 2), (tubeLenght / 2) - (stringSize.Height / 2) + ellipseHeight);
            graph.DrawString(percentageString, f, tubePen3.Brush, textLocation);
        }
        #endregion
    }
}
