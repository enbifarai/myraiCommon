using iTextSharp.text;
using iTextSharp.text.pdf;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class FontManager
    {
        //public static string FontPath = HttpContext.Current.Server.MapPath("~/assets/fontG/open-sans-v13-latin-300.ttf");
        public static string FontPath = CommonHelper.GetFilePath("~/assets/fontG/open-sans-v13-latin-300.ttf");

        private BaseFont customfont;

        private BaseColor baseColor;

        public FontManager(string path, BaseColor color)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                FontPath = path;
            }

            if (color != null)
            {
                baseColor = color;
            }

            customfont = BaseFont.CreateFont(FontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            
        }

        public FontManager()
        {
            baseColor = BaseColor.BLACK;


            customfont = BaseFont.CreateFont(FontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        }

        public Font LeftColumnTitle
        {
            get
            {
                return new Font(customfont, 10, Font.BOLD, baseColor);
            }
        }

        public Font LeftColumnValue
        {
            get
            {
                return new Font(customfont, 10, Font.NORMAL, baseColor);
            }
        }

        public Font H1
        {
            get
            {
                return new Font(customfont, 24, Font.BOLD, baseColor);
            }
        }

        public Font H2
        {
            get
            {
                return new Font(customfont, 18, Font.BOLD, baseColor);
            }
        }

        public Font H3
        {
            get
            {
                return new Font(customfont, 14, Font.BOLD, baseColor);
            }
        }

        public Font H4
        {
            get
            {
                return new Font(customfont, 10, Font.BOLD, baseColor);
            }
        }

        public Font N12
        {
            get
            {
                return new Font(customfont, 12, Font.NORMAL, baseColor);
            }
        }

        public Font Normal
        {
            get
            {
                return new Font(customfont, 10, Font.NORMAL, baseColor);
            }
        }

        public Font Bold
        {
            get
            {
                return new Font(customfont, 10, Font.BOLD, baseColor);
            }
        }

        public Font Italic
        {
            get
            {
                return new Font(customfont, 10, Font.ITALIC, baseColor);
            }
        }

        public Font SmallItalic
        {
            get
            {
                return new Font(customfont, 7, Font.ITALIC, baseColor);
            }
        }


    }
}