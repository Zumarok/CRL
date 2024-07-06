using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crux.CRL.NotificationSystem
{
    public class NotificationManager : IDisposable
    {
        private static NotificationManager _instance;

        private NotificationManager() { }

        public static NotificationManager Instance => _instance ?? (_instance = new NotificationManager());

        private readonly Dictionary<string, Action<INotificationWrapper>> _notificationDict = new Dictionary<string, Action<INotificationWrapper>>();

        //private Dictionary<Delegate, Action<INotificationWrapper>> _notificationSearchDict = new Dictionary<Delegate, Action<INotificationWrapper>>();
        private readonly Dictionary<string, Dictionary<Delegate, Action<INotificationWrapper>>> _notificationSearchDict = new Dictionary<string, Dictionary<Delegate, Action<INotificationWrapper>>>();

        /// <summary>
        /// Add notification listener
        /// The call back function can have 0 - 4 parameters
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pFunc"></param>
        public void AddListener(string pKey, Action pFunc)
        {
            Action<INotificationWrapper> wrapperDel = (p) => pFunc();

            InternalAddListener(pKey, pFunc, wrapperDel);
        }

        public void AddListener<TNotification>(string pKey, Action<TNotification> pFunc)
        {
            //Convert Action<TNotification> to Action<INotificationWrapper>
            //1. Create a INotificationWrapper type p, convert p to NotificationWrapper<TNotification>
            //2. Use p.notification as parameter in Action<TNotification>
            //3. Save the whole function into Action<INotificationWrapper>
            Action<INotificationWrapper> wrapperDel = (p) => pFunc(((NotificationWrapper<TNotification>)p).notification);

            InternalAddListener(pKey, pFunc, wrapperDel);
        }

        public void AddListener<TNotification1, TNotification2>(string pKey, Action<TNotification1, TNotification2> pFunc)
        {
            Action<INotificationWrapper> wrapperDel = (p) => pFunc(((NotificationWrapper<TNotification1, TNotification2>)p).notification1, ((NotificationWrapper<TNotification1, TNotification2>)p).notification2);

            InternalAddListener(pKey, pFunc, wrapperDel);
        }

        public void AddListener<TNotification1, TNotification2, TNotification3>(string pKey, Action<TNotification1, TNotification2, TNotification3> pFunc)
        {
            Action<INotificationWrapper> wrapperDel = (p) => pFunc(((NotificationWrapper<TNotification1, TNotification2, TNotification3>)p).notification1, ((NotificationWrapper<TNotification1, TNotification2, TNotification3>)p).notification2, ((NotificationWrapper<TNotification1, TNotification2, TNotification3>)p).notification3);

            InternalAddListener(pKey, pFunc, wrapperDel);
        }

        public void AddListener<TNotification1, TNotification2, TNotification3, TNotification4>(string pKey, Action<TNotification1, TNotification2, TNotification3, TNotification4> pFunc)
        {
            Action<INotificationWrapper> wrapperDel = (p) => pFunc(((NotificationWrapper<TNotification1, TNotification2, TNotification3, TNotification4>)p).notification1, 
                ((NotificationWrapper<TNotification1, TNotification2, TNotification3, TNotification4>)p).notification2, 
                ((NotificationWrapper<TNotification1, TNotification2, TNotification3, TNotification4>)p).notification3,
                ((NotificationWrapper<TNotification1, TNotification2, TNotification3, TNotification4>)p).notification4);

            InternalAddListener(pKey, pFunc, wrapperDel);
        }

        /// <summary>
        /// Remove nofitication listener
        /// </summary>
        /// <typeparam name="TNotification"></typeparam>
        /// <param name="pKey"></param>
        /// <param name="pFunc"></param>
        public void RemoveListener(string pKey, Action pFunc)
        {
            InternalRemoveListener(pKey, pFunc);
        }

        public void RemoveListener<TNotification>(string pKey, Action<TNotification> pFunc)
        {
            InternalRemoveListener(pKey, pFunc);
        }

        public void RemoveListener<TNotification1, TNotification2>(string pKey, Action<TNotification1, TNotification2> pFunc)
        {
            InternalRemoveListener(pKey, pFunc);
        }

        public void RemoveListener<TNotification1, TNotification2, TNotification3>(string pKey, Action<TNotification1, TNotification2, TNotification3> pFunc)
        {
            InternalRemoveListener(pKey, pFunc);
        }

        public void RemoveListener<TNotification1, TNotification2, TNotification3, TNotification4>(string pKey, Action<TNotification1, TNotification2, TNotification3, TNotification4> pFunc)
        {
            InternalRemoveListener(pKey, pFunc);
        }


        NotificationWrapper _cachedWrapper = new NotificationWrapper();

        /// <summary>
        /// Send notification
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pNotification"></param>
        public void SendNotification(string pKey)
        {
            InternalSendNotification(pKey, _cachedWrapper);
        }

        public void SendNotification<TNotification>(string pKey, TNotification pNotification)
        {
            InternalSendNotification(pKey, new NotificationWrapper<TNotification>(pNotification));
        }

        public void SendNotification<TNotification1, TNotification2>(string pKey, TNotification1 pNotification1, TNotification2 pNotification2)
        {
            InternalSendNotification(pKey, new NotificationWrapper<TNotification1, TNotification2>(pNotification1, pNotification2));
        }

        public void SendNotification<TNotification1, TNotification2, TNotification3>(string pKey, TNotification1 pNotification1, TNotification2 pNotification2, TNotification3 pNotification3)
        {
            InternalSendNotification(pKey, new NotificationWrapper<TNotification1, TNotification2, TNotification3>(pNotification1, pNotification2, pNotification3));
        }

        public void SendNotification<TNotification1, TNotification2, TNotification3, TNotification4>(string pKey, TNotification1 pNotification1, TNotification2 pNotification2, TNotification3 pNotification3, TNotification4 pNotification4)
        {
            InternalSendNotification(pKey, new NotificationWrapper<TNotification1, TNotification2, TNotification3, TNotification4>(pNotification1, pNotification2, pNotification3, pNotification4));
        }


        #region Private Functions
        private void InternalAddListener(string pKey, Delegate pFunc, Action<INotificationWrapper> pAction)
        {
            //Check if listener dictionary has the key.
            //If not, create a new delegate with the key.
            if (_notificationDict.ContainsKey(pKey))
            {
                //Check if the listener is already registered or not.
                //If not, add listener to the coresponding delegate and add listener to the search dictionary.
                if(!_notificationSearchDict[pKey].ContainsKey(pFunc))
                {
                    _notificationDict[pKey] += pAction;
                    _notificationSearchDict[pKey].Add(pFunc, pAction);
                }
                else
                {
                    Debug.LogWarning("The listener is already registered!");
                }
            }
            else
            {
                _notificationDict.Add(pKey, pAction);
                _notificationSearchDict.Add(pKey, new Dictionary<Delegate, Action<INotificationWrapper>>());
                _notificationSearchDict[pKey].Add(pFunc, pAction);
            }
        }

        public void InternalRemoveListener(string pKey, Delegate pFunc)
        {
            //Check if listener dictionary has the key.
            //If not, print error.
            if (_notificationDict.ContainsKey(pKey))
            {
                //Check if the search dictionary has the callback function.
                //If not, print error
                if (_notificationSearchDict[pKey].ContainsKey(pFunc))
                {
                    //Remove the listener form the delegate.
                    _notificationDict[pKey] -= _notificationSearchDict[pKey][pFunc];
                    //Remove the listener from the search dictionary.
                    _notificationSearchDict[pKey].Remove(pFunc);

                    //If there is no listener in the delegate, remove the delegate.
                    if (_notificationDict[pKey] == null)
                    {
                        //FPDebug.Log(string.Format("Remove notification key: {0}", pKey));
                        _notificationDict.Remove(pKey);
                        _notificationSearchDict.Remove(pKey);
                    }
                }
                else
                {
                    Debug.LogError( string.Format( "The listener is not registered! func: {0}", pFunc.ToString() ) );
                }
            }
            else
            {
                Debug.LogError( string.Format("The listener type is not registered! key: {0}", pKey ) );
            }
        }

        private void InternalSendNotification(string pKey, INotificationWrapper pNotificationWapper)
        {
            if (_notificationDict.ContainsKey(pKey))
            {
                if (_notificationDict[pKey] != null)
                {
                    //Trigger the delegate in the listener dictionary.
                    _notificationDict[pKey](pNotificationWapper);
                }
                else
                {
                    Debug.LogError(string.Format("The listener is not registered! key: {0} wrapper: {1}", pKey, pNotificationWapper.ToString()));
                }
            }
            else
            {
                Debug.LogWarning(string.Format("The listener type is not registered! key: {0}", pKey));
            }
        }
        #endregion

        public void Dispose()
        {
            _notificationDict.Clear();
            _notificationSearchDict.Clear();
        }
    }
}