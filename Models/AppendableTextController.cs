using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfTools.Controls;

namespace WpfTools.Models
{
    public class AppendableTextController : IDisposable
    {
        internal AppendableText View { get; set; }


        public void Write(string text)
        {
            this.WriteMain(text, null);
        }
        public void Write(string text, Brush brush)
        {
            this.WriteMain(text, new TextBrush(brush, 0, text.Length));
        }
        public void Write(string text, params TextBrush[] brush)
        {
            this.WriteMain(text, brush);
        }


        public void WriteLine(string text)
        {
            this.WriteMain(text + "\n", null);
        }
        public void WriteLine(string text, Brush brush)
        {
            this.WriteMain(text + "\n", new TextBrush(brush, 0, text.Length));
        }
        public void WriteLine(string text, params TextBrush[] brush)
        {
            this.WriteMain(text + "\n", brush);
        }

        private void WriteMain(string text, params TextBrush[] brush)
        {
            if (this.View == null)
            {
                return;
            }
            this.View.Write(text, brush);
        }

        public void AppendLine(FormattedText text)
        {
            this.View?.AppendLine(text);
        }

        public void ReplaceLine(FormattedText text)
        {
            this.View?.ReplaceLine(text);
        }


        public void ScrollToBottom()
        {
            this.View?.ScrollToBottom();
        }

        public void Dispose()
        {
            this.View = null;
        }
    }

    public class TextBrush
    {
        public Brush Brush { get; }
        public int StartIndex { get; }
        public int Length { get; }
        public bool IsBold { get; }

        public TextBrush(Brush brush, int startIndex, int length)
            : this(brush, false, startIndex, length)
        {
        }

        public TextBrush(bool bold, int startIndex, int length)
            : this(null, bold, startIndex, length)
        {
        }

        public TextBrush(Brush brush, bool bold, int startIndex, int length)
        {
            this.Brush = brush;
            this.IsBold = bold;
            this.StartIndex = startIndex;
            this.Length = length;
        }

        public TextBrush AddOffset(int offset)
        {
            return new TextBrush
                (this.Brush, this.IsBold, this.StartIndex + offset, this.Length);
        }
    }
}
