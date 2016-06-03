﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Foster.SlidesToImages
{
    public class Converter
    {
        private String PathToDll { get; set; }
        public Size NewSize { get; set; }

        public Converter(string pathToLibrary = "")
        {
            if (!String.IsNullOrEmpty(pathToLibrary))
            {
                PathToDll = pathToLibrary;
            }
            else
            {
                PathToDll = System.Web.HttpContext.Current.Server.MapPath("~/FosterSlidesToImagesLib/");
            }
        }

        /// <summary>
        /// Convert specified PDF file to Images (1 image per pdf page)
        /// </summary>
        /// <param name="pdfPath"></param>
        /// <returns></returns>
        public IEnumerable<Image> ConvertPDFToImages(string pdfPath)
        {
            return ExtractImages(pdfPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        IEnumerable<Image> ExtractImages(string file)
        {
            var imagesList = new List<Image>();
            Ghostscript.NET.Rasterizer.GhostscriptRasterizer rasterizer = null;
            Ghostscript.NET.GhostscriptVersionInfo vesion = 
                new Ghostscript.NET.GhostscriptVersionInfo(new Version(0, 0, 0), 
                    PathToDll + @"\gsdll32.dll", 
                    string.Empty, 
                    Ghostscript.NET.GhostscriptLicense.GPL);

            using (rasterizer = new Ghostscript.NET.Rasterizer.GhostscriptRasterizer())
            {
                rasterizer.Open(file, vesion, false);
                for (int i = 1; i <= rasterizer.PageCount; i++)
                {
                    Image img = rasterizer.GetPage(300, 300, i);

                    EncoderParameter qualityParam = 
                        new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 60L);

                    EncoderParameters encoderParams = new EncoderParameters {
                        Param = new EncoderParameter[] { qualityParam }
                    };

                    var imageStream = new MemoryStream();
                    img.Save(imageStream, GetEncoderInfo("image/jpeg"), encoderParams);

                    Image imageExported = new Bitmap(imageStream);
                    if(NewSize != null)
                    {
                        imageExported = Util.ResizeImage(imageExported, NewSize);
                    }
                    imagesList.Add(imageExported);
                }
                rasterizer.Close();
            }
            return imagesList;
        }

        /// <summary> 
        /// Returns the image codec with the given mime type 
        /// </summary> 
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }
    }
}
