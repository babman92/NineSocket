using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineSocket.Data
{
    public class NineResult
    {
        public string Code { get; set; }
        Dictionary<string, string> Result;

        public NineResult(string code, string key, string message)
        {
            Code = key;
            Result = new Dictionary<string, string>();
            Result.Add(key, message);
        }

        public NineResult()
        {
            Code = string.Empty;
            Result = new Dictionary<string, string>();
        }

        public void AddData(string key, string message)
        {
            Result.Add(key, message);
        }

        public string GetData(string key)
        {
            string value = string.Empty;
            try
            {
                if (Result.TryGetValue(key, out value))
                    return value;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return value;
        }
    }
}
