using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BulkArchiver.Models
{
    /// <summary>
    /// アーカイバを表すクラス。
    /// </summary>
    public class Archiver
    {
        /// <summary>
        /// 名称を取得または設定します。
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 実行ファイルのフルパスを取得または設定します。
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 圧縮時のコマンドオプションを取得または設定します。
        /// </summary>
        public string Option { get; set; }
    }

    /// <summary>
    /// アーカイバの一覧を表すクラス。
    /// </summary>
    public class Archivers : List<Archiver>
    {
        // カテゴリ設定のパターン
        private const string LineReg = "^(?<name>.+?)\t(?<path>.+?)\t(?<option>.+?)$";

        /// <summary>
        /// 指定ファイルからアーカイバ設定を取得します。
        /// </summary>
        /// <param name="path">アーカイバ設定を取得するファイルのパスを設定します。</param>
        /// <returns>取得できた場合はtrue、それ以外の場合はfalse。</returns>
        public bool Load(string path)
        {
            Clear();

            using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                string line;
                var reg = new Regex(LineReg);

                while ((line = reader.ReadLine()) != null)
                {
                    var match = reg.Match(line);

                    if (!match.Success)
                    {
                        return false;
                    }

                    if (!File.Exists(match.Groups["path"].Value))
                    {
                        return false;
                    }

                    Add(new Archiver()
                    {
                        Name = match.Groups["name"].Value,
                        Path = match.Groups["path"].Value,
                        Option = match.Groups["option"].Value
                    });
                }
            }

            return true;
        }
    }
}
