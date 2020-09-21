using BulkArchiver.Exts;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace BulkArchiver.Models
{
    /// <summary>
    /// フォルダを表すクラス。
    /// </summary>
    public class Folder : INotifyPropertyChanged
    {
        private string targetPath;
        private bool isArchived;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// フォルダのフルパスを取得または設定します。
        /// </summary>
        public string TargetPath
        {
            get
            {
                return targetPath;
            }
            set
            {
                if (targetPath != value)
                {
                    targetPath = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }
        /// <summary>
        /// 圧縮済みかどうかを取得または設定します。
        /// </summary>
        public bool IsArchived
        {
            get
            {
                return isArchived;
            }
            set
            {
                if (isArchived != value)
                {
                    isArchived = value;
                    NotifyPropertyChanged("IsArchived");
                }
            }
        }

        /// <summary>
        /// プロパティが変更された時にイベントハンドラを呼び出します。
        /// </summary>
        /// <param name="property">変更されたプロパティ名</param>
        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// <see cref="TargetPath"/>内のファイルを圧縮します。
        /// </summary>
        /// <param name="outDir">出力先を設定します。</param>
        /// <param name="archiver">圧縮するアーカイバを設定します。</param>
        public void Archive(string outDir, Archiver archiver)
        {
            // 圧縮済みなら何もしない
            if (IsArchived)
            {
                return;
            }

            // 読み取り専用属性を解除
            ExtIO.RemoveReadOnly(TargetPath);
            // thumbs.dbを削除
            ExtIO.DeleteThumbsDb(TargetPath);

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = archiver.Path,
                    Arguments = string.Format(archiver.Option, Path.Combine(outDir, Path.GetFileName(TargetPath))),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = TargetPath
                }
            })
            {
                process.Start();
                process.WaitForExit();
            }

            // 終了時に圧縮済みをマーク
            IsArchived = true;
        }
    }
}
