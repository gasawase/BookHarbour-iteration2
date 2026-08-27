using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;


namespace Assets.Scripts.Extensions
{
    public static class WebRequestExtensions
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation op)
        {
            return new UnityWebRequestAwaiter(op);
        }
    }

    public struct UnityWebRequestAwaiter : INotifyCompletion
    {
        private UnityWebRequestAsyncOperation asyncOperation;
        public bool IsCompleted => asyncOperation.isDone;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation op)
        {
            asyncOperation = op;
        }
        public void GetResult()
        {

        }

        public void OnCompleted(Action actionToComplete)
        {
            asyncOperation.completed += (_) => { actionToComplete(); };// _ is the value that Unity hands you; we don't need it but we need to invoke the action so we do this
        }
    }
}
