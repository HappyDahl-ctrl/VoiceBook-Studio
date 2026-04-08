using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VoiceBookStudio.Models;

namespace VoiceBookStudio.Views
{
    /// <summary>
    /// Accessible dialog for choosing one of the 14 book section types.
    /// Items are grouped by Front Matter / Body / Back Matter.
    /// </summary>
    public partial class SectionTypeDialog : Window
    {
        /// <summary>The type the user confirmed. Null if cancelled.</summary>
        public SectionType? SelectedType { get; private set; }

        // ----------------------------------------------------------------
        // List-item DTO (keeps XAML bindings simple)
        // ----------------------------------------------------------------

        private sealed class TypeItem
        {
            public SectionType Type        { get; init; }
            public string      DisplayName { get; init; } = string.Empty;
            public string      GroupName   { get; init; } = string.Empty;
        }

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------

        public SectionTypeDialog(SectionType current = SectionType.Chapter)
        {
            InitializeComponent();

            var items = SectionTypeHelper.AllTypes
                .Select(t => new TypeItem
                {
                    Type        = t,
                    DisplayName = SectionTypeHelper.GetDisplayName(t),
                    GroupName   = SectionTypeHelper.GetGroup(t)
                })
                .ToList();

            // Group by Front / Body / Back matter using CollectionView
            var cvs = new CollectionViewSource { Source = items };
            cvs.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TypeItem.GroupName)));
            TypeListBox.ItemsSource = cvs.View;

            // Pre-select current type
            TypeItem? pre = items.FirstOrDefault(i => i.Type == current);
            if (pre != null)
            {
                TypeListBox.SelectedItem = pre;
                TypeListBox.ScrollIntoView(pre);
            }

            Loaded += (_, _) => TypeListBox.Focus();
        }

        // ----------------------------------------------------------------
        // Handlers
        // ----------------------------------------------------------------

        private void OkButton_Click(object sender, RoutedEventArgs e) => Confirm();

        private void TypeListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => Confirm();

        private void Confirm()
        {
            if (TypeListBox.SelectedItem is TypeItem item)
            {
                SelectedType = item.Type;
                DialogResult = true;
            }
        }
    }
}
