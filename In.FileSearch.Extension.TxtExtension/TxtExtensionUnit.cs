using System;
using System.IO;
using System.Text;
using System.Threading;

namespace In.FileSearch.Extension.TxtExtension
{
    public class TxtExtensionUnit : ISearchExtensionUnit
    {
        private readonly ISearchExtensionCallback _callback;
        private readonly string _filePath;
        private readonly string _substring;
        private volatile bool _canceled;
        private readonly ManualResetEvent _finished = new ManualResetEvent(false);

        public ISearchExtensionCallback Callback => _callback;

        public TxtExtensionUnit(string filePath, string substring, ISearchExtensionCallback callback)
        {
            _filePath = filePath;
            _substring = substring;
            _callback = callback;
        }

        public void Start()
        {
            if (_canceled)
            {
                _callback.Canceled();
                _finished.Set();
                return;
            }
            try
            {
                const int size = 2 * 1024;
                var subLength = _substring.Length;
                var bufferSize = size + subLength - 1;
                var buffer = new char[bufferSize];
                var offset = 0;
                var result = false;
                using (var sr = new StreamReader(_filePath, Encoding.Default, true, bufferSize))
                {
                    int read;
                    while ((read = sr.ReadBlock(buffer, offset, bufferSize - offset)) > 0)
                    {
                        var str = new string(buffer, 0, offset + read);
                        if (str.IndexOf(_substring, StringComparison.Ordinal) >= 0)
                        {
                            result = true;
                            break;
                        }
                        if (read < bufferSize - offset)
                        {
                            break;
                        }
                        if (_canceled)
                        {
                            break;
                        }
                        offset = subLength - 1;
                        Array.Copy(buffer, size, buffer, 0, offset);
                    }
                }
                if (!_canceled)
                {
                    _callback.Result(result);
                }
                else
                {
                    _callback.Canceled();
                }
            }
            catch (Exception)
            {
                _callback.Error();
            }
            finally
            {
                _finished.Set();
            }
        }

        public void Cancel()
        {
            _canceled = true;
            _finished.WaitOne();
        }

    }
}