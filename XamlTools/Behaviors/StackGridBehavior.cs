using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Boredbone.XamlTools.Behaviors
{

    public class StackGridBehavior : Behavior<Grid>
    {
        private bool isInitialized = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.Loaded += (o, e) => this.Initialize();
            }
            this.Initialize();
        }

        private void Initialize()
        {

            if (this.isInitialized || this.AssociatedObject == null)
            {
                return;
            }

            var grid = this.AssociatedObject;

            if (grid.Children.Count == 0)
            {
                return;
            }

            grid.RowDefinitions.Clear();

            var columnLength = grid.ColumnDefinitions.Count;

            var rowUsedIndex = new int[columnLength];
            var rowLength = 0;

            foreach (var child in grid.Children)
            {
                var item = (UIElement)child;
                if (Grid.GetRow(item) == 0)
                {
                    var column = Grid.GetColumn(item);
                    if (column < columnLength)
                    {
                        if (rowUsedIndex[column] == rowLength)
                        {
                            // Add RowDefinition
                            var rowDefinition = new RowDefinition();
                            rowDefinition.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
                            grid.RowDefinitions.Add(rowDefinition);
                            rowLength++;
                        }

                        Grid.SetRow(item, rowLength - 1);
                        rowUsedIndex[column] = rowLength;
                    }
                }
            }

            this.isInitialized = true;

        }
    }
}
