using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Configuration;
using System.Threading;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.PoolCore
{
    public class clsPool <T> : Hashtable
        where T : IPoolable, new()
    {

        #region "Variables"

        private static clsPool<T> m_Pool = null;

        #endregion

        #region "Properties and Methods"

        static clsPool() 
        {
        }

        public clsPool()
        {
            ThreadExecute();
        }

        public T GetObject(clsPoolConnectionInfo connectionInfo)
        {
            clsPoolCluster<T> cluster = null;
            int poolRetrys = 0;
            string connectionKey = connectionInfo.key;
            lock (this)
            {
                if (base.ContainsKey(connectionKey)) 
                {
                    cluster = (clsPoolCluster<T>)this[connectionKey];
                }
                else
                {
                    cluster = new clsPoolCluster<T>();
                    this.Add(connectionKey, cluster);
                }
            }
            T tempConnectionObject = cluster.GetObject(connectionInfo);
            while (tempConnectionObject == null)
            {
                poolRetrys++;
                if (poolRetrys == clsPoolParameters<T>.DefaultGetRetrys)
                {
                    //throw new clsPoolException(connectionInfo);
                    ///TODO: Add a new type of exception
                    throw new Exception(Resources.ErrorMessages.errConnection + connectionInfo.key);
                }
                System.Threading.Thread.Sleep(clsPoolConnectionInfo.GetRandomSleep(clsPoolParameters<T>.SleepTime, poolRetrys));
                tempConnectionObject = cluster.GetObject(connectionInfo);
            }
            return tempConnectionObject;
        }

        public void Execute()
        {
            while (true)
            {
                try
                {

                    foreach (clsPoolCluster<T> cluster in this.Values)
                    {

                        System.Diagnostics.Debug.WriteLine(Resources.ErrorMessages.beforeExecution);
                        System.Diagnostics.Debug.WriteLine(Resources.ErrorMessages.usedObjects + cluster.used.ToString());
                        System.Diagnostics.Debug.WriteLine(Resources.ErrorMessages.freeObjects + cluster.free.ToString());

                        cluster.Execute();

                        System.Diagnostics.Debug.WriteLine(Resources.ErrorMessages.afterExecution);
                        System.Diagnostics.Debug.WriteLine(Resources.ErrorMessages.usedObjects + cluster.used.ToString());
                        System.Diagnostics.Debug.WriteLine(Resources.ErrorMessages.freeObjects + cluster.free.ToString());
                    }
                }
                finally 
                {
                    Thread.Sleep(30000);
                }
                
            } 
        }

        public void ThreadExecute()
        {
            Thread threadPool = new Thread(new ThreadStart(this.Execute));
            threadPool.Start();
        }

        public void ReleaseObject(T connectionObject)
        {
            clsPoolCluster<T> cluster = null;
            string connectionKey = connectionObject.ConnectionInfo.key;
            lock (this)
            {
                if (base.ContainsKey(connectionKey))
                {
                    cluster = (clsPoolCluster<T>)this[connectionKey];
                    cluster.ReleaseObject(connectionObject);
                }
                else
                {
                    throw new Exception(Resources.ErrorMessages.errObjectNotFound);
                }
            }
        }

        public static clsPool<T> SingletonPool
        {
            get
            {
                if (m_Pool == null)
                {
                    m_Pool = new clsPool<T>();
                }
                return m_Pool;
            }
        }

        #endregion
    }
}