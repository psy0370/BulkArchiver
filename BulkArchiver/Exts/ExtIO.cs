using System.IO;

namespace BulkArchiver.Exts
{
    public class ExtIO
    {
        /// <summary>
        /// 指定フォルダ内に存在するファイルの読み取り属性を再帰的に解除します。
        /// </summary>
        /// <param name="path">対象フォルダのパスを設定します。</param>
        public static void RemoveReadOnly(string path)
        {
            var di = new DirectoryInfo(path)
            {
                Attributes = FileAttributes.Normal
            };

            var files = di.EnumerateFiles();
            foreach (var file in files)
            {
                file.Attributes = FileAttributes.Normal;
            }

            var directories = Directory.EnumerateDirectories(path);
            foreach (var directory in directories)
            {
                RemoveReadOnly(directory);
            }
        }

        /// <summary>
        /// 指定フォルダ内に存在するthumbs.dbを再帰的に削除します。
        /// </summary>
        /// <param name="path">対象フォルダのパスを設定します。</param>
        public static void DeleteThumbsDb(string path)
        {
            File.Delete(Path.Combine(path, "thumbs.db"));

            var directories = Directory.EnumerateDirectories(path);
            foreach (string directory in directories)
            {
                DeleteThumbsDb(directory);
            }
        }
    }
}
