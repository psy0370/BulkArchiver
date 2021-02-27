using BulkArchiver.Models;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BulkArchiver
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly Archivers archivers = new Archivers();
        private readonly ObservableCollection<Folder> folders = new ObservableCollection<Folder>();
        private string currentDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private CancellationTokenSource tokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 起動時の処理を定義します。
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            archivers.Load("./Resources/Archiver.txt");
            ArchiverCombo.ItemsSource = archivers;
            ArchiverCombo.SelectedIndex = 0;

            FolderList.ItemsSource =folders;
        }

        /// <summary>
        /// フォルダをドラッグしたときの処理を定義します。
        /// </summary>
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        /// <summary>
        /// フォルダをドロップしたときの処理を定義します。
        /// </summary>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            var entries = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var entry in entries)
            {
                if (Directory.Exists(entry))
                {
                    var directories = Directory.EnumerateDirectories(entry);

                    foreach (var directory in directories)
                    {
                        folders.Add(new Folder
                        {
                            TargetPath = directory,
                            IsArchived = false
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 開始ボタンをクリックしたときの処理を定義します。
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // 未処理フォルダのみ抽出
            var list = folders.Where(x => !x.IsArchived).ToArray();

            if(list.Any())
            {
                // フォルダ選択ダイアログを生成
                var dialog = new CommonOpenFileDialog
                {
                    Title = "出力フォルダを選択してください",
                    InitialDirectory = currentDir,
                    Multiselect = false,
                    IsFolderPicker = true
                };

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var outDir = dialog.FileName;
                    currentDir = outDir;

                    var archiver = (Archiver)ArchiverCombo.SelectedItem;

                    tokenSource = new CancellationTokenSource();
                    var token = tokenSource.Token;

                    Progress.Maximum = list.Length;
                    Progress.Value = 0;

                    Task.Factory.StartNew(() =>
                    {
                        foreach (var folder in list)
                        {
                            if (token.IsCancellationRequested)
                            {
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    Progress.IsIndeterminate = false;
                                }));

                                break;
                            }

                            folder.Archive(outDir, archiver);

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                Progress.Value++;
                            }));
                        }

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            CancelButton.IsEnabled = false;
                            FolderList.IsReadOnly = false;
                        }));

                        MessageBox.Show("終了しました", "圧縮", MessageBoxButton.OK, MessageBoxImage.Information);
                    });

                    CancelButton.IsEnabled = true;
                    FolderList.IsReadOnly = true;
                }
            }
        }

        /// <summary>
        /// キャンセルボタンをクリックしたときの処理を定義します。
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelButton.IsEnabled = false;
            Progress.IsIndeterminate = true;

            tokenSource.Cancel();
        }

        /// <summary>
        /// 削除ボタンをクリックしたときの処理を定義します。
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            folders.Clear();
        }
    }
}
