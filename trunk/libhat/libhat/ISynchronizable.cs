using System;
using System.Collections.Generic;
using System.Text;

namespace libhat {
    /// <summary>
    /// Provides interface to merge objects
    /// </summary>
    public interface ISynchronizable {
        /// <summary>
        /// Merge THIS and inputObject.  
        /// </summary>
        /// <param name="inputObject"></param>
        void Merge( object inputObject );

    }
}
