using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace GitPilot3.UserControlles;

public partial class SearchableSelection : UserControl
{
    private string _selectedItem = "Select an option";
    public EventHandler SelectedItemChanged;
    public string SelectedItem 
    { 
        get => _selectedItem;
        set 
        { 
            _selectedItem = value;
            SetSelectedDisplayItem();
        } 
    }

    private string _searchText = string.Empty;
    public List<string> Items { get; set; } = new List<string>
    {
        "Option 1",
        "Option 2",
        "Option 3",
        "Option 4",
        "Option 5"
    };

    private List<string> FilteredItems => Items.Where(i => string.IsNullOrEmpty(_searchText) || i.Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchableSelection"/> class.
    /// Better support with width 200 and height 25
    /// </summary>
    public SearchableSelection()
    {
        InitializeComponent();
        SetupComponent();
    }

    private void SetupComponent()
    {
        SelectableContainer.Children.Clear();
        SelectableContainer.Width = this.Width;
        SelectableContainer.Height = this.Height;
        SelectableContainer.PointerPressed += OnSelectableContainerClicked;
        SetSelectedDisplayItem();
        SetDropDownItems();
    }

    private void SetDropDownItems()
    {
        var flyout = new Flyout
        {
            Content = new StackPanel
            {
                Width = this.Width,
                Margin = new Thickness(0)
            },
        };
        flyout.Closed += OnFlyoutClosed;
        AddItemsToFlyout(flyout);

        FlyoutBase.SetAttachedFlyout(SelectableContainer, flyout);
    }

    private void AddItemsToFlyout(Flyout flyout)
    {
        ((StackPanel)flyout.Content).Children.Clear();
        foreach (var item in FilteredItems)
        {
            var button = new Button
            {
                Content = item,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Thickness(0),
                Padding = new Thickness(5)
            };
            button.Click += (s, e) =>
            {
                SelectedItem = item;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                SetSelectedDisplayItem();
                flyout.Hide();
            };
            ((StackPanel)flyout.Content).Children.Add(button);
        }
    }

    private void OnFlyoutClosed(object? sender, EventArgs e)
    {
        SelectableContainer.Children.Clear();
        SetSelectedDisplayItem();
    }

    private void OnSelectableContainerClicked(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is Control control)
        {
            _searchText = string.Empty;
            FlyoutBase.ShowAttachedFlyout(control);
            TurnContainerToSearchMode();
        }
    }

    private void TurnContainerToSearchMode()
    {
        SelectableContainer.Children.Clear();
        var textBox = new TextBox
        {
            Width = this.Width,
            Height = this.Height,
            Watermark = "Search...",
            Margin = new Thickness(0),
            Padding = new Thickness(5),
            Text = _searchText
        };
        textBox.PointerPressed += (s, e) => e.Handled = true;
        textBox.TextChanged += (s, e) =>
        {
            _searchText = textBox.Text;
            var flyout = FlyoutBase.GetAttachedFlyout(SelectableContainer) as Flyout;
            AddItemsToFlyout(flyout!);
        };
        SelectableContainer.Children.Add(textBox);
        textBox.Focus();
    }

    private void SetSelectedDisplayItem()
    {
        SelectableContainer.Children.Clear();

        var dockPanel = new DockPanel{
            Width = this.Width,
            Height = this.Height,
        };


        dockPanel.Children.Add(
            new TextBlock
            {
                Text = SelectedItem,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(5,0,0,0)
            }
        );

        dockPanel.Children.Add(
            new PathIcon
            {
                Data = Avalonia.Media.Geometry.Parse("M 0 0 L 8 0 L 4 4 Z"),
                Width = 10,
                Height = 10,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Margin = new Thickness(0,0,5,0)
            }
        );

        SelectableContainer.Children.Add(dockPanel);
    }
}