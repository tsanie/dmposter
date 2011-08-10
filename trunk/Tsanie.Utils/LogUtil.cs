using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tsanie.Utils {
    public class LogUtil {
        private static StreamWriter _logWriter = null;
        private static string _logFile = null;

        public static string LogFile {
            get { return _logFile; }
            set {
                if (value != null) {
                    _logFile = value;
                    if (_logWriter != null)
                        _logWriter.Dispose();
                    _logWriter = new StreamWriter(_logFile, true, Encoding.UTF8);
                } else {
                    if (_logWriter != null) {
                        _logWriter.Dispose();
                        _logWriter = null;
                    }
                }
            }
        }

        public static void Error(Exception ex, Action<Exception> exFunc) {
            if (_logWriter != null)
                WriteError(ex, 0);
            if (exFunc != null)
                exFunc(ex);
        }
        public static void Info(string message) {
            if (_logWriter != null)
                WriteInfo(message);
        }

        private static void WriteError(Exception ex, int indent) {
            if (ex == null)
                return;
            if (indent > 0) {
                _logWriter.Write('└');
                _logWriter.Write(new string('─', indent));
            }
            _logWriter.WriteLine("--------------------------------------------------");
            _logWriter.WriteLine("[{0:MM-dd HH:mm:ss.ffff}] - {1}: {2}", DateTime.Now, ex.GetType().FullName, ex.Message);
            _logWriter.WriteLine("--------------------------------------------------");
            _logWriter.WriteLine(ex.StackTrace);
            _logWriter.WriteLine();
            _logWriter.Flush();
            //WriteError(ex.InnerException, indent + 1);
        }
        private static void WriteInfo(string message) {
            _logWriter.WriteLine("--------------------------------------------------");
            _logWriter.WriteLine("[{0:MM-dd HH:mm:ss.ffff}] - {1}", DateTime.Now, message);
            _logWriter.WriteLine("--------------------------------------------------");
            _logWriter.WriteLine();
            _logWriter.Flush();
        }
    }
}
