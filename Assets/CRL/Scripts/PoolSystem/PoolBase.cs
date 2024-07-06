using System.Collections.Generic; 
using UnityEngine;

namespace Crux.CRL.PoolSystem
{  
    //TODO : run update for instances in pool
    public class PoolBase : MonoBehaviour
    {
        public Stack<PooledObjectBase> stack { get; private set; }

        //PooledObjectBase _source; 
        //int _instanceNumber; 

        public virtual void Initialize(PooledObjectBase source, int instanceNumber = 0)
        {
            //this.name = "BasePool";
            //this._source = source;
            //this._instanceNumber = instanceNumber;
            stack = new Stack<PooledObjectBase>();
        }
    }
}