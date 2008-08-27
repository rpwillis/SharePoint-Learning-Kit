    using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.PoolCore
{
    public class clsPoolObject <T>
        where T : IPoolable
    {
        #region "Variables"

        private T m_connectionObject = default(T);
        private State m_state;
        private TimeSpan m_creationTime;
        private TimeSpan m_acquiredTime;
        private TimeSpan m_releasedTime;

        #endregion

        #region "Properties and Methods"
        public clsPoolObject()
        {
            m_state = State.Free;
            creationTime = new TimeSpan(DateTime.Now.Ticks);
            acquiredTime = new TimeSpan(DateTime.Now.Ticks);
            releasedTime = new TimeSpan();
        }

        ~clsPoolObject()
        {
        }

        public State state
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        public TimeSpan creationTime
        {
            get
            {
                return m_creationTime;
            }
            set
            {
                m_creationTime = value;
            }
        }

        public TimeSpan acquiredTime
        {
            get
            {
                return m_acquiredTime;
            }
            set
            {
                m_acquiredTime = value;
            }
        }

        public TimeSpan releasedTime
        {
            get
            {
                return m_releasedTime;
            }
            set
            {
                m_releasedTime = value;
            }
        }

        public T connectionObject
        {
            get 
            {
                return m_connectionObject;
            }
            set 
            {
                m_connectionObject = value;
            }
        }
        #endregion
        public enum State
        {
            Used = 0,
            Free = 1
        }
    }
}