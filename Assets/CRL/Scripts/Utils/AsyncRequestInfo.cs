using UnityEngine;

namespace Crux.CRL.Utils
{
    /// <summary>
    /// Contains status information about the async request.
    /// </summary>
    public class AsyncRequestInfo
    {
        public bool IsCompleted { get; private set; }
        public Status RequestStatus { get; private set; }
        //public ResponseBase Response { get; private set; }

        /// <summary>
        /// Time.realtimeSinceStartup the request was created.
        /// </summary>
        public float RequestTime { get; private set; }

        public enum Status
        {
            Pending = 0,
            FailedBeforeSend,
            FailedAfterSend,
            Success
        }

        public AsyncRequestInfo()
        {
            IsCompleted = false;
            RequestStatus = Status.Pending;
            RequestTime = Time.realtimeSinceStartup;
        }

        public void SetStatus(Status status)
        {
            RequestStatus = status;
        }

        public void SetCompletion(bool isComplete)
        {
            IsCompleted = isComplete;
        }

        //public void SetResponse(ResponseBase response)
        //{
        //    Response = response;
        //}

        /// <summary>
        /// Determines if more than (arg1) seconds have passed since the request was created.
        /// </summary>
        /// <param name="timeOutDuration">The timeout duration.</param>
        /// <returns>True more time has passed than the timeOutDuration.</returns>
        public bool IsTimedOut(float timeOutDuration)
        {
            return Time.realtimeSinceStartup > RequestTime + timeOutDuration;
        }

    }
}