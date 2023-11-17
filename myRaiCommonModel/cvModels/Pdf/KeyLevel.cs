using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class KeyLevel : RenderableItem
    {
        private int _size;

        public KeyLevel()
        {
            this._size = 8;
        }

        public int size
        {
            get
            {
                return this._size;
            }
            set
            {
                this._size = value;
            }
        }

        public string key { get; set; }
        public int level { get; set; }
        public int range { get; set; }
        public bool favorite { get; set; }

        public override void Render(ColumnText ct)
        {
            Image offImage = Image.GetInstance(Cons.OFF);
            Image onImage = Image.GetInstance(Cons.ON);

            offImage.ScaleToFit(this.size, this.size);
            onImage.ScaleToFit(this.size, this.size);
            
            Phrase p = new Phrase(key, new FontManager().Normal);

            for (int i = 0; i < this.level; i++)
            {
                p.Add(new Chunk(onImage, this.size, -this.size / 4, true));
            }

            for (int i = 0; i < this.range - this.level; i++)
            {
                p.Add(new Chunk(offImage, this.size, -this.size / 4, true));
            }

            if (favorite)
            {
                Image starImage = Image.GetInstance(Cons.STAR);
                starImage.ScaleToFit(this.size, this.size);
                p.Add(new Chunk(starImage, this.size + 5, -this.size / 4, true));
            }
            
            Paragraph pp = new Paragraph(p) { IndentationLeft = this.IndentationLeft };

            ct.AddElement(pp);
        }

        public override void Render(Document document)
        {
            Image offImage = Image.GetInstance(Cons.OFF);
            Image onImage = Image.GetInstance(Cons.ON);

            offImage.ScaleToFit(this.size, this.size);
            onImage.ScaleToFit(this.size, this.size);

            //onImage.setColo

            Phrase p = new Phrase(key, new FontManager().Normal);

            for (int i = 0; i < this.level; i++)
            {
                p.Add(new Chunk(onImage, this.size, -this.size / 4, true));
            }

            for (int i = 0; i < this.range - this.level; i++)
            {
                p.Add(new Chunk(offImage, this.size, -this.size / 4, true));
            }

            if (favorite)
            {
                Image starImage = Image.GetInstance(Cons.STAR);
                starImage.ScaleToFit(this.size, this.size);
                p.Add(new Chunk(starImage, this.size + 5, -this.size / 4, true));
            }

            Paragraph pp = new Paragraph(p) { IndentationLeft = this.IndentationLeft };

            document.Add(pp);
        }
    }
}