using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;

namespace LuminaDesktop
{
    public partial class MouseGestureSettingsWindow : Window
    {
        private AppSettings _settings;
        private string? _selectedProcess;

        public MouseGestureSettingsWindow()
        {
            InitializeComponent();
            _settings = SettingsService.Load();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshProcessList();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            SettingsService.Save(_settings);
            
            // Apply immediately
            var app = (App)System.Windows.Application.Current;
            app.ApplySettings(_settings);
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RefreshProcessList()
        {
            lstProcesses.ItemsSource = null;
            lstProcesses.ItemsSource = _settings.MouseGestures.Keys.ToList();
            if (_selectedProcess != null && _settings.MouseGestures.ContainsKey(_selectedProcess))
            {
                lstProcesses.SelectedItem = _selectedProcess;
            }
        }

        private void LstProcesses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProcess = lstProcesses.SelectedItem as string;
            RefreshGesturesList();
        }

        private void RefreshGesturesList()
        {
            if (_selectedProcess == null)
            {
                txtSelectedProcessTitle.Text = "Gestures";
                icGestures.ItemsSource = null;
                return;
            }

            txtSelectedProcessTitle.Text = $"Gestures for {_selectedProcess}";
            
            var gestures = _settings.MouseGestures[_selectedProcess];
            var displayList = new List<KeyValuePair<string, string>>();
            foreach(var kvp in gestures)
            {
                displayList.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value));
            }
            
            icGestures.ItemsSource = displayList;
        }

        private void BtnAddProcess_Click(object sender, RoutedEventArgs e)
        {
            var proc = txtNewProcess.Text.Trim();
            if (string.IsNullOrEmpty(proc)) return;

            // Normalize (e.g. "chrome.exe" -> "chrome.exe")
            if (!_settings.MouseGestures.ContainsKey(proc))
            {
                _settings.MouseGestures[proc] = new Dictionary<string, string>();
                _selectedProcess = proc;
                txtNewProcess.Clear();
                RefreshProcessList();
            }
        }

        private void BtnRemoveProcess_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProcess != null)
            {
                _settings.MouseGestures.Remove(_selectedProcess);
                _selectedProcess = null;
                RefreshProcessList();
            }
        }

        private void BtnAddGesture_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProcess == null) return;

            var dirItem = cmbNewDirection.SelectedItem as ComboBoxItem;
            var dir = dirItem?.Content?.ToString()?.Trim();
            var key = txtNewHotkey.Text.Trim();

            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(key)) return;

            _settings.MouseGestures[_selectedProcess][dir] = key;
            
            txtNewHotkey.Clear();
            RefreshGesturesList();
        }

        private void BtnRemoveGesture_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProcess == null) return;

            if (sender is Button btn && btn.Tag is string dir)
            {
                _settings.MouseGestures[_selectedProcess].Remove(dir);
                RefreshGesturesList();
            }
        }
    }
}
