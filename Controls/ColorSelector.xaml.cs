﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace WpfTools.Controls
{
    /// <summary>
    /// ColorSelector.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorSelector : UserControl, IDisposable
    {
        #region SelectedColor

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorSelector),
            new PropertyMetadata(Colors.White, new PropertyChangedCallback(OnSelectedColorChanged)));

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as ColorSelector;
            var value = e.NewValue as Color?;

            if (thisInstance != null && value != null)
            {
                thisInstance.viewModel.SetColor(value.Value);
            }

        }

        #endregion

        private readonly ColorSelectorViewModel viewModel;
        private readonly CompositeDisposable disposables;

        public ColorSelector()
        {
            InitializeComponent();

            this.disposables = new CompositeDisposable();

            this.viewModel = new ColorSelectorViewModel().AddTo(this.disposables);
            this.rootGrid.DataContext = this.viewModel;

            this.viewModel.SelectedColor.Subscribe(x => this.SelectedColor = x).AddTo(this.disposables);
        }

        public void Dispose()
        {
            this.disposables.Dispose();
        }
    }

    class ColorSelectorViewModel : IDisposable, INotifyPropertyChanged
    {
        [Range(0, 255)]
        public ReactiveProperty<byte> A { get; }
        [Range(0, 255)]
        public ReactiveProperty<byte> R { get; }
        [Range(0, 255)]
        public ReactiveProperty<byte> G { get; }
        [Range(0, 255)]
        public ReactiveProperty<byte> B { get; }

        public ReadOnlyReactiveProperty<Color> SelectedColor { get; }
        public ReactiveCommand PresetCommand { get; }

        public ObservableCollection<Color> Presets { get; }

        private readonly CompositeDisposable disposables;

        private ReactiveProperty<bool> Updating { get; }

        public ColorSelectorViewModel()
        {
            this.disposables = new CompositeDisposable();

            this.A = new ReactiveProperty<byte>((byte)0xFF).AddTo(this.disposables);
            this.R = new ReactiveProperty<byte>((byte)0xFF).AddTo(this.disposables);
            this.G = new ReactiveProperty<byte>((byte)0xFF).AddTo(this.disposables);
            this.B = new ReactiveProperty<byte>((byte)0xFF).AddTo(this.disposables);

            this.Updating = new ReactiveProperty<bool>(false).AddTo(this.disposables);

            this.SelectedColor = Observable
                .CombineLatest(this.A, this.R, this.G, this.B)
                .CombineLatest(this.Updating, (x, _) => x)
                .Where(_ => !this.Updating.Value)
                .Select(x => Color.FromArgb(x[0], x[1], x[2], x[3]))
                .ToReadOnlyReactiveProperty()
                .AddTo(this.disposables);

            //this.SelectedColor.Subscribe(x =>
            //{
            //    this.A.Value = x.A;
            //    this.R.Value = x.R;
            //    this.G.Value = x.G;
            //    this.B.Value = x.B;
            //})
            //.AddTo(this.disposables);

            this.Presets = new ObservableCollection<Color>(new[]
            {
                Colors.White,
                Colors.Red,
                Colors.Magenta,
                Colors.Purple,
                Colors.Blue,
                Colors.Cyan,
                Colors.Green,
                Colors.Lime,
                Colors.Yellow,
                Colors.Orange,
                Colors.Black,
                //Color.FromRgb(255, 255, 255),
                //Color.FromRgb(0, 255, 255),
                //Color.FromRgb(255, 0, 255),
                //Color.FromRgb(255, 255, 0),
                //Color.FromRgb(0, 0, 255),
                //Color.FromRgb(255, 0, 0),
                //Color.FromRgb(0, 255, 0),
                //Color.FromRgb(122, 255, 255),
                //Color.FromRgb(255, 122, 255),
                //Color.FromRgb(255, 255, 122),
                //Color.FromRgb(122, 122, 255),
                //Color.FromRgb(255, 122, 122),
                //Color.FromRgb(122, 255, 122),
                //Color.FromRgb(122, 122, 122),
                //Color.FromRgb(0, 0, 0),
            });

            this.PresetCommand = new ReactiveCommand().AddTo(this.disposables);
            this.PresetCommand.Subscribe(x =>
            {
                var color = x as Color?;
                if (color != null)
                {
                    this.SetColor(color.Value);
                }
            })
            .AddTo(this.disposables);
        }

        public void SetColor(Color color)
        {
            if (color == this.SelectedColor.Value)
            {
                return;
            }
            this.Updating.Value = true;
            this.A.Value = color.A;
            this.R.Value = color.R;
            this.G.Value = color.G;
            this.B.Value = color.B;
            this.Updating.Value = false;
        }

        public void Dispose()
        {
            this.disposables.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    public class ColorToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
