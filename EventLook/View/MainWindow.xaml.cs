﻿using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using EventLook.View;
using EventLook.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace EventLook
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<MainViewModel>();

            // Here we send a message which is caught by the view model.  The message contains a reference
            // to the CollectionViewSource which is instantiated when the view is instantiated (before the view model).
            WeakReferenceMessenger.Default.Send(new ViewCollectionViewSourceMessageToken() { CVS = (CollectionViewSource)(this.Resources["X_CVS"]) });
            
            var showWindowService = new ShowWindowService<DetailWindow, DetailViewModel>(){ Owner = this };
            WeakReferenceMessenger.Default.Send(new DetailWindowMessageToken() { ShowWindowService = showWindowService });

            ContentRendered += (s, e) => { ((MainViewModel)DataContext).OnLoaded(); };
            ((MainViewModel)DataContext).Refreshing += OnRefreshing;
            ((MainViewModel)DataContext).Refreshed += OnRefreshed;

            ProcessCommandLine();
        }

        private void ProcessCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                var fileName = args[1];
                if (File.Exists(fileName))
                {
                    var extension = System.IO.Path.GetExtension(fileName);
                    if (extension == ".evtx")
                    {
                        WeakReferenceMessenger.Default.Send(new FileToBeProcessedMessageToken() { FilePath = fileName });
                    }
                }
            }
        }

        private void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }
        private void OnDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null)
            {
                foreach (string uriString in files)
                {
                    // Pass the file name to the MainViewModel
                    WeakReferenceMessenger.Default.Send(new FileToBeProcessedMessageToken() { FilePath = uriString });
                }
            }
        }
        private async void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (isRefreshing)
                return;

            if (sender is DataGrid dataGrid)
            {
                // Without a delay it may fail to scroll to the selected event when resetting filters.
                await Task.Delay(1);
                if (dataGrid.SelectedItem != null)
                    dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            }
        }
        private void CheckTruncate_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItem != null)
                dataGrid1.ScrollIntoView(dataGrid1.SelectedItem);
        }
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                if (Ex1.IsExpanded == false)
                {
                    Ex1.IsExpanded = true;
                    await Task.Delay(1);
                }
                textBoxMsgFilter.Focus();
            }
        }
        private bool isRefreshing = false;
        private void OnRefreshing()
        {
            isRefreshing = true;
            ScrollToTop();
        }
        private void OnRefreshed()
        {
            isRefreshing = false;
            
            // If the user is choosing the Event Log Source, we shouldn't interrupt.
            if (!ComboBox_Source.IsDropDownOpen && dataGrid1.Items.Count > 0)
            {
                // After refresh, SelectedIndex is -1 unless you click the dataGrid during refresh.
                SelectRow.SelectRowByIndex(dataGrid1, dataGrid1.SelectedIndex < 0 ? 0 : dataGrid1.SelectedIndex);
            }
        }

        private void ScrollToTop()
        {
            dataGrid1.SelectedIndex = 0;
            if (dataGrid1.SelectedItem != null)
                dataGrid1.ScrollIntoView(dataGrid1.SelectedItem);
        }
    }
}
