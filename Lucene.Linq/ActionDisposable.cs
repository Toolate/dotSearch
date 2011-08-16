using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucene.Linq {
    internal class ActionDisposable : IDisposable {
        readonly Action action;
        public ActionDisposable(Action action) {
            this.action = action;
        }

        #region IDisposable Members

        public void Dispose() {
            action();
        }

        #endregion
    }
}
