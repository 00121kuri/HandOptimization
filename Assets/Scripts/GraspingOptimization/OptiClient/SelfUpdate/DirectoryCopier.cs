using System.IO;
using UnityEngine;

namespace GraspingOptimization
{
    public class DirectoryCopier
    {
        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            // ソースディレクトリが存在しない場合は処理を終了
            if (!Directory.Exists(sourceDir))
            {
                return;
            }

            // ターゲットディレクトリが存在しない場合は作成
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // ソースディレクトリ内のすべてのファイルをコピー
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var dest = Path.Combine(targetDir, Path.GetFileName(file));
                try
                {
                    File.Copy(file, dest, true); // true で既存のファイルを上書き
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                }
            }

            // ソースディレクトリ内のすべてのサブディレクトリをコピー
            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var dest = Path.Combine(targetDir, Path.GetFileName(directory));
                CopyDirectory(directory, dest);
            }
        }
    }
}