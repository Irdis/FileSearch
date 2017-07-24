using System;
using System.Threading;
using System.Xml;

namespace In.FileSearch.Extension.XmlExtension
{
    public class XmlExtensionUnit : ISearchExtensionUnit
    {
        private readonly string _filePath;
        private readonly string _name;
        private readonly string _value;
        private readonly ISearchExtensionCallback _callback;

        private volatile bool _canceled;
        private readonly ManualResetEvent _finished = new ManualResetEvent(false);

        public ISearchExtensionCallback Callback => _callback;


        public XmlExtensionUnit(string filePath, string name, string value, ISearchExtensionCallback callback)
        {
            _filePath = filePath;
            _name = name;
            _value = value;
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
                bool result = false;
                using (var sr = XmlReader.Create(_filePath))
                {
                    while (sr.Read())
                    {
                        if (sr.GetAttribute(_name) == _value)
                        {
                            result = true;
                            break;
                        }
                        if (_canceled)
                        {
                            break;
                        }
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