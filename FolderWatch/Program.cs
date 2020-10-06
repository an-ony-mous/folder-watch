using System;
using System.IO;
using System.Linq;

namespace FolderWatch {
    public class Program {

        /// <summary>
        /// メイン処理
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args) {
            // コンソール出力
            Console.WriteLine($"監視対象 {SystemCore.Setting.TargetDir}");
            // フォルダ内のファイル監視
            var eWatch = new FolderWatchAsync();
            try {
                // 監視の実行
                eWatch.Execute();
                // キャンセル(ESCボタン)を押下されるまで待機
                while (true) {
                    // キャンセル受付
                    eWatch.CancellationRequest();
                    // キー入力
                    var eReadKey = Console.ReadKey();
                    // ESCボタンの場合
                    if (eReadKey.Key == ConsoleKey.Escape) {
                        // キャンセル実行
                        eWatch.Cancel();
                    }
                }
            }
            catch (OperationCanceledException) {
                Console.WriteLine("処理を中断します");
            }

            // 処理結果を出力
            foreach (var eDirTables in eWatch.DirTables) {
                // フォルダパスからフォルダ名を取得
                var eDirName = Path.GetFileName(eDirTables.Key);

                if (Directory.Exists(eDirTables.Key)) {
                    // ファイル一覧取得
                    var eFiles = Directory.GetFiles(eDirTables.Key, "*", SearchOption.TopDirectoryOnly);
                    // コンソール出力
                    Console.WriteLine($"{eDirName}のファイル数 {eFiles.Count()}");
                }
                else {
                    // コンソール出力
                    Console.WriteLine($"{eDirName}のファイル数 0");
                }
            }
            Console.ReadKey();
        }
    }
}
