using log4net;
using log4net.Appender;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace FolderWatch {
    /// <summary>
    /// 共通プロパティ
    /// </summary>
    public class SystemCore {
        /// <summary>
        /// ロギング
        /// </summary>
        public static ILog ILog4Net => SetLog();

        /// <summary>
        /// アプリケーションフォルダ
        /// </summary>
        public static string AppDir => SetAppDir();

        /// <summary>
        /// アプリケーションフォルダ設定
        /// </summary>
        /// <returns></returns>
        private static string SetAppDir() {
            var eThisPath = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(eThisPath);
        }

        /// <summary>
        /// システムフォルダ
        /// </summary>
        public static string SystemDir => SetSystemDir();

        /// <summary>
        /// システムフォルダ設定
        /// </summary>
        /// <returns></returns>
        private static string SetSystemDir() {
            return Path.Combine(AppDir, "System");
        }

        /// <summary>
        /// システム設定情報
        /// </summary>
        public static SettingModel Setting { get; } = SetSetting();

        /// <summary>
        /// システム設定情報読込
        /// </summary>
        /// <returns></returns>
        private static SettingModel SetSetting() {
            // 処理結果
            var eResult = new SettingModel();
            // システム設定情報ファイルパス(開発環境)
            var eFilePath = Path.Combine(SystemDir, "Setting.json");
            // ファイルが存在しない場合
            if (!File.Exists(eFilePath)) {
                // システムエラー
                throw new Exception("システム設定情報ファイルが見つかりません");
            }
            // ファイル読込
            var eJsontext = File.ReadAllText(eFilePath);
            // 設定情報を読み込む
            eResult = JsonConvert.DeserializeObject<SettingModel>(eJsontext);
            return eResult;
        }


        /// <summary>
        /// ログ設定情報処理(初回のみ)
        /// </summary>
        /// <param name="pAssembly"></param>
        public static ILog SetLog() {
            // ログ情報が生成済みの場合
            if (ILog4Net != null) {
                return ILog4Net;
            }
            // ログフォルダ作成
            var eLogDir = Path.Combine(AppDir, "Logs");
            if (!Directory.Exists(eLogDir)) {
                Directory.CreateDirectory(eLogDir);
            }
            // アペンダー生成
            var eAppender = new RollingFileAppender();
            // 出力するファイル名
            eAppender.File = Path.Combine(eLogDir, "FolderWatch");
            // ファイル追記モード
            eAppender.AppendToFile = true;
            // ログファイル名を固定にする
            eAppender.StaticLogFileName = true;
            // ログファイルの最大世代
            eAppender.MaxSizeRollBackups = 30;
            // ファイルの最大サイズ
            eAppender.MaximumFileSize = "1MB";
            // ファイル拡張子固定
            eAppender.PreserveLogFileNameExtension = true;
            // ログの排他制御
            eAppender.LockingModel = new FileAppender.MinimalLock();
            // 文字コード指定
            eAppender.Encoding = Encoding.UTF8;
            // プログラム実行毎に切換
            eAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
            // ファイル内のフォーマット
            eAppender.Layout = new log4net.Layout.PatternLayout(@"%d : %m%n");
            // 設定を有効にする
            eAppender.ActivateOptions();
            // Log4Netにロガーを設定
            var eLog4Net = LogManager.GetLogger(eAppender.File);
            var eLogger = (log4net.Repository.Hierarchy.Logger)eLog4Net.Logger;
            // 出力レベルの設定
            eLogger.Level = log4net.Core.Level.All;
            // 出力先を追加
            eLogger.AddAppender(eAppender);
            // 設定を有効にする
            eLogger.Hierarchy.Configured = true;

            return eLog4Net;
        }
    }

    /// <summary>
    /// 設定モデル
    /// </summary>
    public class SettingModel {
        /// <summary>
        /// 監視対象フォルダパス
        /// </summary>
        [JsonProperty("TargetDir")]
        public string TargetDir { get; set; }
    }
}
