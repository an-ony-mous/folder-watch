using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FolderWatch {
    /// <summary>
    /// フォルダ内のファイル監視
    /// </summary>
    public class FolderWatchAsync {
        /// <summary>
        /// フォルダパスとファイル数
        /// </summary>
        public Dictionary<string, int> DirTables { set; get; }

        /// <summary>
        /// キャンセル受付オブジェクト
        /// </summary>
        private CancellationTokenSource Source { get; }

        /// <summary>
        /// キャンセルトークン
        /// </summary>
        private CancellationToken Token { get; }

        /// <summary>
        /// フォルダ監視用
        /// </summary>
        private FileSystemWatcher DirWatcher { get; }

        /// <summary>
        /// ファイル監視用
        /// </summary>
        private FileSystemWatcher FileWatcher { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="pCancell"></param>
        public FolderWatchAsync() {
            // キャンセル受付オブジェクト
            Source = new CancellationTokenSource();
            // キャンセルトークン設定
            Token = Source.Token;
            // フォルダ監視用
            DirWatcher = new FileSystemWatcher();
            // ファイル監視用
            FileWatcher = new FileSystemWatcher();
            // フォルダパスとフォルダ内のファイル数
            DirTables = new Dictionary<string, int>();
        }

        /// <summary>
        /// メイン処理
        /// </summary>
        public void Execute() {
            // 監視対象フォルダ
            var TargetDir = Directory.CreateDirectory(SystemCore.Setting.TargetDir).FullName;
            // 起動時に全フォルダのチェック
            var eAllDirs = Directory.GetDirectories(TargetDir, "*", SearchOption.TopDirectoryOnly);
            //
            foreach (var ePath in eAllDirs) {
                // フォルダパスからフォルダ名を取得
                var eDirName = Path.GetFileName(ePath);
                // ファイル一覧取得
                var eFiles = Directory.GetFiles(ePath, "*", SearchOption.TopDirectoryOnly);
                // 辞書に追加
                DirTables.Add(ePath, eFiles.Count());
                // コンソール出力
                Console.WriteLine($"{eDirName}のファイル数 {eFiles.Count()}");
            }
            // サブフォルダの監視をしない
            DirWatcher.IncludeSubdirectories = false;
            // 対象のフォルダパス
            DirWatcher.Path = TargetDir;
            // どんな種類の変更を監視するか
            DirWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.CreationTime;
            // 新規作成
            DirWatcher.Created += DirChanged;
            // 変更
            DirWatcher.Deleted += DirChanged;
            // 削除
            DirWatcher.Renamed += DirChanged;
            // イベント開始
            DirWatcher.EnableRaisingEvents = true;


            // サブフォルダも監視できるようにする
            FileWatcher.IncludeSubdirectories = true;
            // 対象のフォルダパス
            FileWatcher.Path = TargetDir;
            // どんな種類の変更を監視するか
            FileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;
            // 全ファイル対象
            FileWatcher.Filter = "*.*";
            // ファイルが作成された場合
            FileWatcher.Created += FileChanged;
            // ファイルが削除された場合
            FileWatcher.Deleted += FileChanged;
            // イベント開始
            FileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// フォルダの変更イベント
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void DirChanged(object source, FileSystemEventArgs e) {
            // 対象フォルダパス
            var eDirPath = e.FullPath;
            // 変更タイプ
            switch (e.ChangeType) {
                // フォルダが作成された場合
                case WatcherChangeTypes.Created:
                    DirTables.Add(eDirPath, 0);
                    break;
                // フォルダが削除された場合
                case WatcherChangeTypes.Deleted:
                    DirTables[eDirPath] = 0;
                    break;
                // フォルダが変更された場合
                case WatcherChangeTypes.Changed:
                    break;
                // フォルダ名が変更された場合
                case WatcherChangeTypes.Renamed:
                    var eRenamed = e as RenamedEventArgs;
                    RenameKey(DirTables, eRenamed.OldFullPath, eRenamed.FullPath);
                    break;
            }
        }

        /// <summary>
        /// 辞書のKey変更
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="pDictionary"></param>
        /// <param name="pFromKey"></param>
        /// <param name="pToKey"></param>
        public void RenameKey<TKey, TValue>(IDictionary<TKey, TValue> pDictionary, TKey pFromKey, TKey pToKey) {
            TValue pValue = pDictionary[pFromKey];
            pDictionary.Remove(pFromKey);
            pDictionary[pToKey] = pValue;
        }

        /// <summary>
        /// ファイルの変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileChanged(object sender, FileSystemEventArgs e) {
            // フォルダパス取得
            var eThisDir = Path.GetDirectoryName(e.FullPath);
            // ファイル一覧取得
            var eFiles = Directory.GetFiles(eThisDir, "*", SearchOption.TopDirectoryOnly);
            // ファイル数取得
            var eFileCount = eFiles.Count();
            // 登録されている辞書に存在するかチェック
            var eAction = DirTables.Where(x => x.Key == eThisDir).FirstOrDefault();
            // 存在しない(フォルダが作成された場合)
            if (eAction.Key == null) {
                // 新規辞書に追加
                DirTables.Add(eThisDir, eFileCount);
            }
            // 存在する場合
            else {
                // 最新
                DirTables[eThisDir] = eFileCount;
            }
        }

        /// <summary>
        /// キャンセル受付
        /// </summary>
        public void CancellationRequest() {
            // キャンセル受付
            Token.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// キャンセル処理
        /// </summary>
        public void Cancel() {
            // キャンセル実行
            Source.Cancel();
            // イベントの開放
            DirWatcher.EnableRaisingEvents = false;
            FileWatcher.EnableRaisingEvents = false;
        }
    }
}
