using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.PoolCore
{
    public class clsPoolCluster <T>
        where T : IPoolable, new()
    {

        #region "Variables"

        private int m_maxObject;
        private LinkedList<clsPoolObject<T>> m_listFree;
        private LinkedList<clsPoolObject<T>> m_listUsed;

        #endregion

        #region "Properties and Methods"

        public clsPoolCluster()
        {
            m_maxObject = clsPoolParameters<T>.MaxPoolSize;
            m_listFree = new LinkedList<clsPoolObject<T>>();
            m_listUsed = new LinkedList<clsPoolObject<T>>();
        }

        public int free
        {
            get
            {
                return m_listFree.Count;
            }
        }

        public int used
        {
            get
            {
                return m_listUsed.Count;
            }
        }

        public int maxObject
        {
            get
            {
                return m_maxObject;
            }
        }

        public T GetObject(clsPoolConnectionInfo connectionInfo)
        {
            clsPoolObject<T> poolObject = null;
            lock (this)
            {
                if (free > 0)
                {
                    poolObject = m_listFree.First.Value;
                    m_listFree.RemoveFirst();
                    poolObject.state = clsPoolObject<T>.State.Used;
                    m_listUsed.AddLast(poolObject);
                }
                else 
                {
                    if ((free + used) < maxObject)
                    {
                        T connectionObject = new T();
                        poolObject = new clsPoolObject<T>();
                        connectionObject.ConnectionInfo = connectionInfo;
                        try
                        {
                            connectionObject.Acquire();
                        }
                        catch (System.Exception ex)
                        {
                            throw new Exception(Resources.ErrorMessages.errAquireConnection, ex);
                        }
                        poolObject.state = clsPoolObject<T>.State.Used;
                        poolObject.connectionObject = connectionObject;
                        m_listUsed.AddLast(poolObject);
                    }
                    else 
                    {
                        return default(T);
                    }
                }
            }
            return poolObject.connectionObject;
        }

        public void ReleaseObject(T connectionObject) 
        {
            bool foundInList = false;
            clsPoolObject<T> pooledObject = null;  
            lock (this)
            {
                foreach (clsPoolObject<T> listPoolObject in m_listUsed) 
                {
                    if (listPoolObject.connectionObject.UniqueID == connectionObject.UniqueID)
                    {
                        pooledObject = listPoolObject;
                        foundInList = true;
                        pooledObject.state = clsPoolObject<T>.State.Free;
                        m_listFree.AddLast(pooledObject);
                    }
                }
                if (foundInList)
                    m_listUsed.Remove(pooledObject);
            }
        }

        public void Execute()
        {
            lock (this)
            {
                TimeSpan currentTime = new TimeSpan(DateTime.Now.Ticks);
                TimeSpan tempTime = new TimeSpan();
                List<clsPoolObject<T>> listFreeToDelete = new List<clsPoolObject<T>>();
                List<clsPoolObject<T>> listUsedToDelete = new List<clsPoolObject<T>>();
                int usedAgingTime = clsPoolParameters<T>.UsedAgingTime;
                int freeAgingTime = clsPoolParameters<T>.FreeAgingTime;
                //Clean collection of pool objects free
                foreach (clsPoolObject<T> listFreePoolObject in m_listFree)
                {
                    tempTime = currentTime;
                    tempTime = tempTime.Subtract(listFreePoolObject.acquiredTime);
                    if (tempTime.TotalMilliseconds > freeAgingTime)
                    {
                        listFreeToDelete.Add(listFreePoolObject);
                    }
                }
                foreach (clsPoolObject<T> freeToDelete in listFreeToDelete)
                {
                    m_listFree.Remove(freeToDelete);
                }
                //Clean colleaction of pool objects used
                foreach (clsPoolObject<T> listUsedPoolObject in m_listUsed)
                {
                    tempTime = currentTime;
                    tempTime = tempTime.Subtract(listUsedPoolObject.acquiredTime);
                    if (tempTime.TotalMilliseconds  > usedAgingTime)
                    {
                        listUsedPoolObject.connectionObject.Release(listUsedPoolObject.connectionObject);
                        listUsedToDelete.Add(listUsedPoolObject);
                    }
                }
                foreach (clsPoolObject<T> usedToDelete in listUsedToDelete)
                {
                    m_listUsed.Remove(usedToDelete);
                }
            }
        }
        #endregion
    }
}