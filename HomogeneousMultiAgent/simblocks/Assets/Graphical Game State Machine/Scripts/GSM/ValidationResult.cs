using System.Collections.Generic;
using UnityEngine;

namespace GSM
{
    public class ValidationResult
    {
        public const int UNREACHABLE_STATE = 1;
        public const int ABSORBING_STATE = 2;
        public const int EMPTY_TRIGGER = 3;
        public const int DUPLICATE_STATE_NAME = 4;
        public const int DUPLICATE_TRIGGER = 5;
        public const int MISSING_START_STATE = 6;
        public const int UNNECESSARY_EDGE = 7;

        private readonly List<Result> results = new List<Result>();

        public void AddResult(int code, string message, WarnLevel level)
        {
            results.Add(new Result() { Code = code, Message = message, WarnLevel = level });
        }


        public void PrintResults()
        {
            foreach (var result in results)
            {
                result.Print();
            }
        }


        public class Result
        {
            public int Code { get; internal set; }
            public string Message { get; internal set; }
            public WarnLevel WarnLevel { get; internal set; }

            public void Print()
            {
                string msg = "Result Code " + Code + ": " + Message;
                switch (WarnLevel)
                {
                    case WarnLevel.Information:
                        Debug.Log(msg);
                        break;
                    case WarnLevel.Warn:
                        Debug.LogWarning(msg);
                        break;
                    case WarnLevel.Fatal:
                        Debug.LogError(msg);
                        break;
                    default:
                        break;
                }
            }
        }

        public enum WarnLevel : int
        {
            Information = 0, Warn = 1, Fatal = 2
        }

    }
}
