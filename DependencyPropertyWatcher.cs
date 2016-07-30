using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;
//using Windows.UI.Xaml;
//using Windows.UI.Xaml.Data;

namespace Boredbone.XamlTools
{

    /// <summary>
    /// 依存関係プロパティの変更を監視するクラス
    /// </summary>
    /// <typeparam name="T">監視対象の依存関係プロパティ値の型</typeparam>
    public class DependencyPropertyWatcher<T> : DependencyObject, IDisposable
    {
        #region Value 依存関係プロパティ
        /// <summary>
        /// 監視対象値 依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register(
            "Value",
            typeof(object),
            typeof(DependencyPropertyWatcher<T>),
            new PropertyMetadata(
                null,
                (s, e) =>
                {
                    var control = s as DependencyPropertyWatcher<T>;
                    if (control != null)
                    {
                        control.OnValueChanged(control);
                    }
                }));

        /// <summary>
        /// 監視対象値 変更イベントハンドラ
        /// </summary>
        private void OnValueChanged(object sender)
        {
            DependencyPropertyWatcher<T> source = (DependencyPropertyWatcher<T>)sender;

            if (source.PropertyChanged != null)
            {
                source.PropertyChanged(source, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 監視対象値
        /// </summary>
        public T Value
        {
            get { return (T)this.GetValue(ValueProperty); }
        }
        #endregion //Value 依存関係プロパティ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target">監視対象</param>
        /// <param name="propertyPath">監視対象プロパティ</param>
        public DependencyPropertyWatcher(DependencyObject target, string propertyPath)
        {
            this.Target = target;
            BindingOperations.SetBinding(
                this,
                ValueProperty,
                new Binding() { Source = target, Path = new PropertyPath(propertyPath), Mode = BindingMode.OneWay });
        }

        /// <summary>
        /// プロパティ変更イベント
        /// </summary>
        public event EventHandler PropertyChanged;

        /// <summary>
        /// 監視対象
        /// </summary>
        public DependencyObject Target { get; private set; }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose()
        {
            this.ClearValue(ValueProperty);
        }
    }
}
