using System.Text;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.PoolCore
{
    public interface IPoolable
    {
        clsPoolConnectionInfo ConnectionInfo
        {
            get;
            set;
        }
    
        void Release(IPoolable poolable);

        void Acquire();

        System.Guid UniqueID
        {
            get;
        }  
    }
}