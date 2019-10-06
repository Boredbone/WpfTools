using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Boredbone.XamlTools.Behaviors
{
    public class SizeBehavior : Behavior<Control>
    {

        #region Width

        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register(nameof(Width), typeof(double),
                typeof(SizeBehavior), new PropertyMetadata(0.0));

        #endregion

        #region Height

        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register(nameof(Height), typeof(double),
                typeof(SizeBehavior), new PropertyMetadata(0.0));

        #endregion
        


        /// <summary>
        /// アタッチ時の初期化処理
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.SizeChanged += (o, e) =>
            {
                this.Width = e.NewSize.Width;
                this.Height = e.NewSize.Height;
            };
        }

    }
}